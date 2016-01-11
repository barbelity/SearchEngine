using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SearchEngine.Model
{
    class Parse
    {
        // used for events
        public delegate void ModelFunc(int type, string value);
        public event ModelFunc ModelChanged;

        static Dictionary<string, string> months = new Dictionary<string, string>();
        static private Dictionary<string, bool> StopWords;
        //Mutex mStopwords = new Mutex();
        static private string filesPath;
        static StemmerInterface stemmer = new Stemmer();

        internal static void getTermsFromString(string query)
        {
            getTerms(ref query, datesInOrderRegex, "Date", "Query");
            getTerms(ref query, datesMonthFirstRegex, "Date", "Query");
            getTerms(ref query, yearsRegex, "Year", "Query");
            getTerms(ref query, rangeReg, "Range", "Query");
            getTerms(ref query, percentReg, "Percent", "Query");
            getTerms(ref query, priceReg, "Price", "Query");
            getTerms(ref query, numReg, "Number", "Query");
            getTerms(ref query, namesReg, "Name", "Query");
            getTerms(ref query, quoteRegex, "Quote", "Query");
            getTerms(ref query, capsRegex, "CapsHeadline", "Query");
            getTerms(ref query, wordRegex, "Word", "Query");
        }

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
        static Regex NumberReg = new Regex(@"\d+(,\d{3})*(\.\d+)?(\s\d+\/\d+)?");

        #endregion

        static Indexer _indexer;
        /// <summary>
        /// constractor
        /// </summary>
        /// <param name="Path">path to data files</param>
        /// <param name="indexer">instanc of indexer</param>
        /// <param name="stemming">yes/no stemming</param>
        public Parse(string Path, Indexer indexer, bool stemming)
        {
            use_stem = stemming;
            filesPath = Path;
            _indexer = indexer;
            StopWords = ReadFile.readStopWords(Path + @"\stop_words.txt");
            addMonths();
        }


        Thread[] a_Threads = new Thread[8];
        Thread t_indexer = null;
        /// <summary>
        /// main func for parsing
        /// </summary>
        public void startParsing()
        {
            ModelChanged(1, "Started parsing");
            DateTime start = DateTime.Now;
            string[] paths = ReadFile.getFilesPaths(filesPath);
            int i = 0, j = 0;
            foreach (string filePath in paths)
            {
                if (stop) return;
                //runs every file in a new thread
                a_Threads[i] = new Thread(new ParameterizedThreadStart(ThreadParsing));
                a_Threads[i].Start(filePath);

                if (++i == 8)
                {
                    j += i;
                    i = 0;
                    foreach (Thread t in a_Threads)
                    {
                        t.Join();
                    }
                    //every 40 files start indexing
                    if (j == 40)
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
                if (t != null)
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
            //save all list data for import
            _indexer.saveLists();
            float time = (DateTime.Now.Minute * 60 + DateTime.Now.Second - start.Minute * 60 - start.Second) / 60;
            int numOfTerms = _indexer.getNumOfTerms();
            ModelChanged(1, "Finshed parsing and indexing docs after " + time + " min\n" + "Number of Docs: " + d_docs.Count + "\nNumber of Terms: " + numOfTerms);

        }

        internal static bool stop = false;
        internal void kill()
        {
            stop = true;
            if (a_Threads != null)
                for (int i = 0; i < a_Threads.Length; i++)
                    if (a_Threads[i] != null)
                        a_Threads[i].Join();

            if (t_indexer != null) t_indexer.Join();
        }

        /// <summary>
        /// calls indexing used in a new thread
        /// </summary>
        static void startIndexing()
        {
            _indexer.startIndexing(ref d_terms);
        }

        /// <summary>
        /// threaded parsing, run on a singel file, split docs and save to dic
        /// </summary>
        /// <param name="o_filePath">file path</param>
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

        /// <summary>
        /// parse a singel doc for ThreadParsing
        /// </summary>
        /// <param name="docRaw"></param>
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
            lock (d_docs)
            {
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
            numOfTerms += getTerms(ref text, quoteRegex, "Quote", docName);
            numOfTerms += getTerms(ref text, capsRegex, "CapsHeadline", docName);
            numOfTerms += getTerms(ref text, wordRegex, "Word", docName);
            //update numOfTerms - if we put it in constructor of doc it saves time
            doc.termsCount = numOfTerms;
            doc.d_TermsCount = null;
        }


        /// <summary>
        /// adds a term to dictionary
        /// </summary>
        /// <param name="d_terms">dictionary to add to</param>
        /// <param name="term">term string to add</param>
        /// <param name="docName">docName</param>
        /// <param name="index">index of term</param>
        /// <param name="numOfTerms">numOfTerms in doc</param>
        /// <param name="type">term type</param>
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


        /// <summary>
        /// find and saves terms by regex
        /// </summary>
        /// <param name="text">text of doc</param>
        /// <param name="regex">regex to use</param>
        /// <param name="type">type of terms it finds</param>
        /// <param name="docName">the doc to search in</param>
        /// <returns></returns>
        static private int getTerms(ref string text, Regex regex, string type, string docName)
        {
            MatchCollection terms = regex.Matches(text);
            int numOfTerms = 0;
            string termString;
            foreach (Match term in terms)
            {
                termString = term.ToString().ToLower().Replace('\n', ' ').Trim(charsToTrim);

                // Stop words
                if (StopWords.ContainsKey(termString) || termString.Length <= 2)
                    continue;

                if (!(type[0] == 'W') && !(type[0] == 'N') && !(type[0] == 'Q') && !(type[0] == 'C'))
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

                    case "Number":
                        string[] termSplit = termString.Split(' ');
                        Double numformated;
                        Double.TryParse(termSplit[0].ToString(), out numformated);
                        if (termSplit.Length > 1)
                        {
                            if (termSplit[1][0] == 'm')
                            {
                                numformated = numformated * 1000000;
                            }
                            else if (termSplit[1][0] == 'b')
                            {
                                numformated = numformated * 1000000000;
                            }
                            else if (termSplit[1][0] == 't')
                            {
                                numformated = numformated * 1000000000000;
                            }
                            else if (termSplit[1][0] == 'h')
                            {
                                numformated = numformated * 100;
                            }
                        }

                        addTermToDic(d_abNumTerms, numformated.ToString(), docName, term.Index, ref numOfTerms, "Number");
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
                        {
                            lock (stemmer)
                            {
                                termString = stemmer.stemTerm(termString);
                            }
                        }


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

        /// <summary>
        /// uses to fill months dic for forrmating dates
        /// </summary>
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



    }
}