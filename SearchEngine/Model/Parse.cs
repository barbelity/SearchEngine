using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SearchEngine.Model
{
    class Parse : iParse
    {
        static Dictionary<string, string> months = new Dictionary<string, string>();
        static private Dictionary<string, bool> StopWords;
        //Mutex mStopwords = new Mutex();
        static private string filesPath;
        static StemmerInterface stemmer = new Stemmer();
        static SortedDictionary<string, Term>[] d_terms = new SortedDictionary<string, Term>[5];
        public static SortedDictionary<string, Term> d_abNumTerms = new SortedDictionary<string, Term>();
        public static SortedDictionary<string, Term> d_cfTerms = new SortedDictionary<string, Term>();
        public static SortedDictionary<string, Term> d_gmTerms = new SortedDictionary<string, Term>();
        public static SortedDictionary<string, Term> d_nrTerms = new SortedDictionary<string, Term>();
        public static SortedDictionary<string, Term> d_szTerms = new SortedDictionary<string, Term>();
        public static Dictionary<string, Doc> d_docs = new Dictionary<string, Doc>();
        

        static bool use_stem = false;

        static private char[] charsToTrim = { ',', '.', ' ', ';', ':', '~', '|', '\n' };
        #region Regex's
        //regexs
        static Regex numReg = new Regex(@"\d+(,\d{3})*(\.\d+)?(\s\d+\/\d+)?(\s(million|trillion|billion|hundreds))?");
        static Regex rangeReg = new Regex(@"\s(between\s)?\d+(\-|\sand\s)\d+", RegexOptions.IgnoreCase);
        static Regex percentReg = new Regex(@"\s\d+(,\d{3})*(\.\d+)?(\s\d+\/\d+)?(\%|\s(percent|percents|percentage))");
        static Regex priceReg = new Regex(@"\s(Dollars\s|\$)\d+(,\d{3})*(\.\d+)?(\s\d+\/\d+)?(m|bn|\smillion|\sbillion)?");
        static Regex namesReg = new Regex(@"\s([A-Z][a-z]{1,}\s)+");
        static Regex wordRegex = new Regex(@"[a-zA-Z]+");

        //Regex datesRegex = new Regex(@"([1-9]\d?(th)?\s)?(jan(uary)?|feb(ruary)?|mar(ch)?|apr(il)?|may|june?|july?|aug(ust)?|sep(tember)?|oct(ober)?|nov(ember)?|dec(ember)?)(\s\d{1,4})?(,\s\d{1,4})?)|\s\d{4})\s", RegexOptions.IgnoreCase);
        //Regex datesRegex = new Regex(@"(\D([1-9]\d?(th)?\s)?(jan(uary)?|feb(ruary)?|mar(ch)?|apr(il)?|may|june?|july?|aug(ust)?|sep(tember)?|oct(ober)?|nov(ember)?|dec(ember)?)(\s\d{1,4})?(,\s\d{4})?)", RegexOptions.IgnoreCase);

        static Regex yearsRegex = new Regex(@"\s[1-2]\d{3}\s");
        static Regex datesInOrderRegex = new Regex(@"([1-2]\d|3[0-2]|[1-9])(th)?\s(jan(uary)?|feb(ruary)?|mar(ch)?|apr(il)?|may|june?|july?|aug(ust)?|sep(tember)?|oct(ober)?|nov(ember)?|dec(ember)?)(\s\d{2,4})?", RegexOptions.IgnoreCase);
        static Regex datesMonthFirstRegex = new Regex(@"(jan(uary)?|feb(ruary)?|mar(ch)?|apr(il)?|may|june?|july?|aug(ust)?|sep(tember)?|oct(ober)?|nov(ember)?|dec(ember)?)\s(\d{1,4})(\,\s\d{4})?", RegexOptions.IgnoreCase);
        //my regexs
        static Regex quoteRegex = new Regex(@"\x22(\s|\w|\d)*\x22"); // for quotes
        static Regex capsRegex = new Regex(@"[A-Z]{2,}(\s[A-Z]{2,})*"); // for initials (like "JS") or headlines ("EMPLOYERS STAY OUT OF CIP DEBATE") 

        //for date fix
        static Regex justMonthReg = new Regex(@"(jan(uary)?|feb(ruary)?|mar(ch)?|apr(il)?|may|june?|july?|aug(ust)?|sep(tember)?|oct(ober)?|nov(ember)?|dec(ember)?)", RegexOptions.IgnoreCase);

        static Regex justANumberReg = new Regex(@"\d+");

        #endregion

        static iIndexer _indexer;
        public Parse(string Path, iIndexer indexer)
        {
            filesPath = Path;
            _indexer = indexer;
            StopWords = ReadFile.readStopWords(Path + @"\stop_words.txt");
            addMonths();
        }

        Thread[] a_Threads = new Thread[15];
        Thread t_indexer = null;
        public void startParsing()
        {

            string[] paths = ReadFile.getFilesPaths(filesPath);
            int i=0,j = 0;
            foreach (string filePath in paths)
            {
                a_Threads[i] = new Thread(new ParameterizedThreadStart(ThreadParsing));
                a_Threads[i].Start(filePath);

                if (++i == 15)
                {
                    j += i;
                    i = 0;
                    foreach (Thread t in a_Threads)
                    {
                        t.Join();
                    }

                    if (j == 60)
                    {
                        j = 0;
                        if (!(t_indexer == null))
                        {
                            t_indexer.Join();
                        }

                        d_terms[0] = d_abNumTerms;
                        d_terms[1] = d_cfTerms;
                        d_terms[2] = d_gmTerms;
                        d_terms[3] = d_nrTerms;
                        d_terms[4] = d_szTerms;
                        t_indexer = new Thread(new ThreadStart(startIndexing));
                        t_indexer.Start();
                        d_abNumTerms = new SortedDictionary<string, Term>();
                        d_cfTerms = new SortedDictionary<string, Term>();
                        d_gmTerms = new SortedDictionary<string, Term>();
                        d_nrTerms = new SortedDictionary<string, Term>();
                        d_szTerms = new SortedDictionary<string, Term>();
                    }
                    
                }



            }
            foreach (Thread t in a_Threads)
            {
                t.Join();
            }
            if (!(t_indexer == null))
            {
                t_indexer.Join();
            }
            d_terms[0] = d_abNumTerms;
            d_terms[1] = d_cfTerms;
            d_terms[2] = d_gmTerms;
            d_terms[3] = d_nrTerms;
            d_terms[4] = d_szTerms;
            t_indexer = new Thread(new ThreadStart(startIndexing));
            t_indexer.Start();
            t_indexer.Join();
            for (i = 0; i < 5; i++)
            {
                d_terms[i] = null;
            }
            d_abNumTerms = null;
            d_cfTerms = null;
            d_gmTerms = null;
            d_nrTerms = null;
            d_szTerms = null;
            System.Console.WriteLine("finished all at:" + DateTime.Now);

        }

        static void startIndexing()
        {
            _indexer.startIndexing(ref d_terms);
        }

        public static void ThreadParsing(object o_filePath)
        {
            string filePath = (string)o_filePath;
            //if(System.IO.Path.GetFileName(filePath)== "stop_words.txt") continue;
            string[] docs = ReadFile.fileToDocString(filePath);
            foreach (string doc in docs)
            {
                if (doc.Length > 3)
                {
                    parseDoc(doc);

                }
            }
        }



        static public void parseDoc(string docRaw)
        {

            //get name
            
            string[] split = docRaw.Split(new string[] { "<DOCNO>" }, StringSplitOptions.None);
            split = split[1].Split(new string[] { "</DOCNO>" }, StringSplitOptions.None);
            string docName = split[0].Trim(charsToTrim);
            Doc doc;
            //get text + date
            split = split[1].Split(new string[] { "<TEXT>" }, StringSplitOptions.None);
            int dateStartIdx = split[0].IndexOf("<DATE1>");
            if (dateStartIdx > 0)
            {
                string date = split[0].Substring(dateStartIdx + 7, split[0].IndexOf("</DATE1>") - dateStartIdx - 7);
                doc = new Doc(docName, date.Trim(charsToTrim));
            }
            else
                doc = new Doc(docName);

            split = split[1].Split(new string[] { "</TEXT>" }, StringSplitOptions.None);
            string text = split[0];

            doc.d_TermsCount = new Dictionary<string, int>();
            lock (d_docs) { 
            d_docs[docName] = doc;
            }
            //get terms from text
            int numOfTerms = 0;//stores the number of terms in doc
            numOfTerms += getTerms(ref text, datesInOrderRegex, "Date", docName);
            numOfTerms += getTerms(ref text, datesMonthFirstRegex, "Date", docName);
            numOfTerms += getTerms(ref text, yearsRegex, "Year", docName);
            numOfTerms += getTerms(ref text, rangeReg, "Range", docName);
            numOfTerms += getTerms(ref text, percentReg, "Percent", docName);
            numOfTerms += getTerms(ref text, priceReg, "Price", docName);
            numOfTerms += getTerms(ref text, numReg, "Number", docName);
            numOfTerms += getTerms(ref text, namesReg, "Name", docName);
            numOfTerms += getTerms(ref text, wordRegex, "Word", docName);
            //update numOfTerms - if we put it in constructor of doc it saves time
            doc.termsCount = numOfTerms;
            doc.d_TermsCount = null;
        }

        static private void addTermToDic(SortedDictionary<string, Term> d_terms, string term, string docName, int index, ref int numOfTerms, string type)
        {
            lock (d_terms)
            {
                if (!d_terms.ContainsKey(term))
                {
                    d_terms[term] = new Term(type, term);
                    numOfTerms++;
                }
                d_terms[term].addPosition(docName, index);
            }
            int tfDoc;
            // change max tf in doc if needed
            Doc doc = d_docs[docName];
            if (doc.d_TermsCount.ContainsKey(term))
            {
                tfDoc = ++doc.d_TermsCount[term];
            }
            else
            {
                doc.d_TermsCount[term] = 1;
                tfDoc = 1;
            }

            if (doc.maxtfCount < tfDoc)
            {
                doc.maxtfString = term;
                doc.maxtfCount = tfDoc;
            }
        }

        static private int getTerms(ref string text, Regex regex, string type, string docName)
        {
            MatchCollection terms = regex.Matches(text);
            int numOfTerms = 0;
            foreach (Match term in terms)
            {
                string termString = term.ToString().ToLower().Replace('\n', ' ').Trim(charsToTrim);

                // Stop words
                if (StopWords.ContainsKey(termString) || termString.Length <= 2)
                    continue;

                 if (!(type == "Word") && !(type == "Number"))
                {
                    string clearTerm = new String('#', term.Length - 2);
                    text = text.Substring(0, term.Index) + " " + clearTerm + " " + text.Substring(term.Index + term.Length);
                }
                DateTime convertedDate;
                switch (type)
                {
                    case "Range":
                        MatchCollection numbers = justANumberReg.Matches(termString);
                        int a, b;
                        int.TryParse(numbers[1].ToString(), out b);
                        int.TryParse(numbers[0].ToString(), out a);
                        if (Math.Abs(a - b) < 20)
                        {
                            for (; a <= b; a++)
                            {
                                addTermToDic(d_abNumTerms, a.ToString(), docName, term.Index, ref numOfTerms, "Number");
                            }
                        }
                        else
                        {
                            addTermToDic(d_abNumTerms, a.ToString(), docName, term.Index, ref numOfTerms, "Number");
                            addTermToDic(d_abNumTerms, b.ToString(), docName, term.Index, ref numOfTerms, "Number");
                        }
                        break;
                    case "Percent":
                        string[] percentSplit = termString.Split(' ', '%');
                        float percent;
                        float.TryParse(percentSplit[0], out percent);
                        addTermToDic(d_abNumTerms, (percent * 0.01).ToString("P"), docName, term.Index, ref numOfTerms, "Percent");
                        break;
                    case "Price":
                        MatchCollection number = numReg.Matches(termString);
                        float price;
                        float.TryParse(number[0].ToString(), out price);
                        if (termString.Contains('m'))
                        {
                            price = price * 1000000;
                        }
                        else if (termString.Contains('n'))
                        {
                            price = price * 1000000000;
                        }
                        addTermToDic(d_abNumTerms, price.ToString("C", new CultureInfo("en-US")), docName, term.Index, ref numOfTerms, "Price");
                        break;
                    case "Date":
                        try
                        {
                            //with th
                            int thIndex = termString.IndexOf("th");
                            if (thIndex >= 0)
                            {
                                termString = termString.Remove(thIndex, 2);
                            }
                            convertedDate = Convert.ToDateTime(termString);
                            termString = convertedDate.ToShortDateString();
                            addTermToDic(d_abNumTerms, termString, docName, term.Index, ref numOfTerms, "Date");
                        }
                        catch (Exception e)
                        {
                            //manually convert
                            string dd, mm, yyyy;
                            string[] termStringSplited = termString.Split(' ');
                            if (months.ContainsKey(termStringSplited[1]))
                            {
                                dd = termStringSplited[0];
                                mm = months[termStringSplited[1]];
                                if (termStringSplited.Length == 3)
                                {
                                    yyyy = termStringSplited[2];
                                }
                                else
                                {
                                    yyyy = "2015";
                                }
                            }
                            else
                            {
                                mm = months[termStringSplited[0]];
                                if (termStringSplited.Length == 3)
                                {
                                    dd = termStringSplited[1].Trim(',');
                                    yyyy = termStringSplited[2];
                                }
                                else
                                {
                                    if (termStringSplited[1].Length <= 2)
                                    {
                                        dd = termStringSplited[1];
                                        yyyy = "2015";
                                    }
                                    else
                                    {
                                        yyyy = termStringSplited[1];
                                        dd = "01";
                                    }
                                }
                            }
                            addTermToDic(d_abNumTerms, dd + "/" + mm + "/" + yyyy, docName, term.Index, ref numOfTerms, "Date");
                        }
                        break;
                    case "Year":
                        convertedDate = new DateTime(int.Parse(termString), 1, 1);
                        termString = convertedDate.ToShortDateString();
                        addTermToDic(d_abNumTerms, termString, docName, term.Index, ref numOfTerms, "Date");
                        break;

                    default:
                        //stemmer
                        if (use_stem)
                            termString = stemmer.stemTerm(termString);
                        //insert to correct dictionary
                        if (termString[0] >= 's')
                            addTermToDic(d_szTerms, termString, docName, term.Index, ref numOfTerms, type);
                        else if (termString[0] >= 'n')
                            addTermToDic(d_nrTerms, termString, docName, term.Index, ref numOfTerms, type);
                        else if (termString[0] >= 'g')
                            addTermToDic(d_gmTerms, termString, docName, term.Index, ref numOfTerms, type);
                        else if (termString[0] >= 'c')
                            addTermToDic(d_cfTerms, termString, docName, term.Index, ref numOfTerms, type);
                        else
                            addTermToDic(d_abNumTerms, termString, docName, term.Index, ref numOfTerms, type);
                        break;
                }
            }

            return numOfTerms;
        }

        private void addMonths()
        {
            months.Add("january", "01");
            months.Add("february", "02");
            months.Add("march", "03");
            months.Add("april", "04");
            months.Add("may", "05");
            months.Add("june", "06");
            months.Add("july", "07");
            months.Add("august", "08");
            months.Add("september", "09");
            months.Add("october", "10");
            months.Add("november", "11");
            months.Add("december", "12");
            months.Add("jan", "01");
            months.Add("feb", "02");
            months.Add("mar", "03");
            months.Add("apr", "04");
            months.Add("jun", "06");
            months.Add("jul", "07");
            months.Add("aug", "08");
            months.Add("sep", "09");
            months.Add("oct", "10");
            months.Add("nov", "11");
            months.Add("dec", "12");

        }

        /*
private string fixDate(string termString)
{
    string dd, mm, yyyy;
    MatchCollection matches = justMonthReg.Matches(termString);

    if (matches.Count != 0)
    {
        mm = months[matches[0].ToString().ToLower()];
        matches = numReg.Matches(termString);

        dd = matches[0].ToString();
        if (dd.Length < 2) dd = "0" + dd;

        if (matches.Count < 2)
        {
            yyyy = "xxxx";
        }
        else
        {
            yyyy = matches[1].ToString();
            if (yyyy.Length == 2)
            {
                yyyy = "19" + yyyy;
            }
        }
    }
    else
    {
        mm = "xx";
        dd = "xx";
        yyyy = termString;

    }

    return dd + "/" + mm + "/" + yyyy;
}
*/

        /*
        /// <summary>
        /// gets term dates
        /// </summary>
        /// <param name="text">docText</param>
        /// <param name="d_terms">term dictionary</param>
        /// <param name="docName">document name</param>
        /// <returns></returns>
        private int getDatesTerms(ref string text, ref Dictionary<string, Term> d_terms, string name)
        {

            string dd, mm, yyyy;
            int numOfTerms = 0;
            // dates are in order like "15th may 1999"
            MatchCollection terms = datesInOrderRegex.Matches(text);
            foreach (Match term in terms)
            {
                string termString = term.ToString().ToLower().Replace('\n', ' ').Trim(charsToTrim);
                string[] termStringSplited = termString.Split(' ');
                dd = termStringSplited[0];
                if (dd.Length == 1)
                {
                    dd = "0" + dd;
                }
                mm = months[termStringSplited[1].ToLower()];
                if (termStringSplited.Length == 3)
                {
                    yyyy = termStringSplited[2];
                }
                else
                {
                    yyyy = "xxxx";
                }
                addTermToDic(dd + "/" + mm + "/" + yyyy, ref d_terms, name, term.Index, ref numOfTerms, "Date");
                //clear term
                string clearTerm = new String('*', term.Length);
                text = text.Substring(0, term.Index) + clearTerm + text.Substring(term.Index + term.Length);
            }
            // month is first like "may 1, 1999"
            terms = datesMonthFirstRegex.Matches(text);
            foreach (Match term in terms)
            {
                string termString = term.ToString().ToLower().Replace('\n', ' ').Trim(charsToTrim);
                string[] termStringSplited = termString.Split(' ', ',');
                mm = months[termStringSplited[0].ToLower()];

                if (termStringSplited[1].Length > 2)
                {
                    yyyy = termStringSplited[1];
                    dd = "xx";
                }
                else
                {
                    dd = termStringSplited[1];
                    if (dd.Length == 1)
                    {
                        dd = "0" + dd;
                    }
                    if (termStringSplited.Length == 4)
                    {
                        yyyy = termStringSplited[3];
                    }
                    else
                    {
                        yyyy = "xxxx";
                    }

                }

                addTermToDic(dd + "/" + mm + "/" + yyyy, ref d_terms, name, term.Index, ref numOfTerms, "Date");
                //clear term
                string clearTerm = new String('*', term.Length);
                text = text.Substring(0, term.Index) + clearTerm + text.Substring(term.Index + term.Length);
            }

            // just years
            terms = yearsRegex.Matches(text);
            foreach (Match term in terms)
            {
                string termString = term.ToString().ToLower().Replace('\n', ' ');
                string[] termStringSplited = termString.Split(' ');
                yyyy = termStringSplited[1];
                dd = "xx";
                mm = "xx";

                addTermToDic(dd + "/" + mm + "/" + yyyy, ref d_terms, name, term.Index, ref numOfTerms, "Date");
                //clear term
                string clearTerm = new String('*', term.Length);
                text = text.Substring(0, term.Index) + clearTerm + text.Substring(term.Index + term.Length);

            }
            return numOfTerms;
        }
        */

    }
}