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
        Dictionary<string, string> months = new Dictionary<string, string>();
        private Dictionary<string, bool> StopWords;
        Mutex mStopwords = new Mutex();
        string filesPath;
        StemmerInterface stemmer = new Stemmer();

        private char[] charsToTrim = { ',', '.', ' ', ';', ':', '~', '|', '\n' };
        #region Regex's
        //regexs
        Regex numReg = new Regex(@"\d+(,\d{3})*(\.\d+)?(\s\d+\/\d+)?(\s(million|trillion|billion|hundreds))?");
        Regex rangeReg = new Regex(@"\s(between\s)?\d+(\-|\sand\s)\d+", RegexOptions.IgnoreCase);
        Regex percentReg = new Regex(@"\s\d+(,\d{3})*(\.\d+)?(\s\d+\/\d+)?(\%|\s(percent|percents|percentage))");
        Regex priceReg = new Regex(@"\s(Dollars\s|\$)\d+(,\d{3})*(\.\d+)?(\s\d+\/\d+)?(m|bn|\smillion|\sbillion)?");
        Regex namesReg = new Regex(@"\s([A-Z][a-z]{1,}\s)+");
        Regex wordRegex = new Regex(@"[a-zA-Z]+");

        //Regex datesRegex = new Regex(@"([1-9]\d?(th)?\s)?(jan(uary)?|feb(ruary)?|mar(ch)?|apr(il)?|may|june?|july?|aug(ust)?|sep(tember)?|oct(ober)?|nov(ember)?|dec(ember)?)(\s\d{1,4})?(,\s\d{1,4})?)|\s\d{4})\s", RegexOptions.IgnoreCase);
        //Regex datesRegex = new Regex(@"(\D([1-9]\d?(th)?\s)?(jan(uary)?|feb(ruary)?|mar(ch)?|apr(il)?|may|june?|july?|aug(ust)?|sep(tember)?|oct(ober)?|nov(ember)?|dec(ember)?)(\s\d{1,4})?(,\s\d{4})?)", RegexOptions.IgnoreCase);

        Regex yearsRegex = new Regex(@"\s[1-2]\d{3}\s");
        Regex datesInOrderRegex = new Regex(@"([1-2]\d|3[0-2]|[1-9])(th)?\s(jan(uary)?|feb(ruary)?|mar(ch)?|apr(il)?|may|june?|july?|aug(ust)?|sep(tember)?|oct(ober)?|nov(ember)?|dec(ember)?)(\s\d{2,4})?", RegexOptions.IgnoreCase);
        Regex datesMonthFirstRegex = new Regex(@"(jan(uary)?|feb(ruary)?|mar(ch)?|apr(il)?|may|june?|july?|aug(ust)?|sep(tember)?|oct(ober)?|nov(ember)?|dec(ember)?)\s(\d{1,4})(\,\s\d{4})?", RegexOptions.IgnoreCase);
        //my regexs
        Regex quoteRegex = new Regex(@"\x22(\s|\w|\d)*\x22"); // for quotes
        Regex capsRegex = new Regex(@"[A-Z]{2,}(\s[A-Z]{2,})*"); // for initials (like "JS") or headlines ("EMPLOYERS STAY OUT OF CIP DEBATE") 

        //for date fix
        Regex justMonthReg = new Regex(@"(jan(uary)?|feb(ruary)?|mar(ch)?|apr(il)?|may|june?|july?|aug(ust)?|sep(tember)?|oct(ober)?|nov(ember)?|dec(ember)?)", RegexOptions.IgnoreCase);

        Regex justANumberReg = new Regex(@"\d+");

        #endregion

        public SortedDictionary<string, Term> d_allTerms = new SortedDictionary<string, Term>();
        Dictionary<string, Doc> d_docs = new Dictionary<string, Doc>();
        bool use_stem = false;

        public Parse(string path)
        {
            filesPath = path;
			StopWords = ReadFile.readStopWords(@"C:\Users\Bar\Desktop\engineFiles\stopWords\stopwords.txt");

            addMonths();
        }



        public void startParsing()
        {
			Indexer indexer = new Indexer();
			int docsCount = 0;
			string[] paths = ReadFile.getFilesPaths(filesPath);
            foreach (string filePath in paths)
            {
                string[] docs = ReadFile.fileToDocString(filePath);
                foreach (string doc in docs)
                {
                    if (doc.Length > 3)
                    {
                        parseDoc(doc);
						docsCount++;
						if (docsCount == 2000)
						{
							indexer.saveTerms(d_allTerms);
							d_allTerms = new SortedDictionary<string, Term>();
							docsCount = 0;
						}
                        //int j = 1;
                    }
                }
            }
			indexer.saveTerms(d_allTerms);
			d_allTerms = new SortedDictionary<string, Term>();
			docsCount = 0;
            //int i = 1;
        }



        public void parseDoc(string docRaw)
        {
            //get name
            string[] split = docRaw.Split(new string[] { "<DOCNO>" }, StringSplitOptions.None);
            split = split[1].Split(new string[] { "</DOCNO>" }, StringSplitOptions.None);
            string docName = split[0].Trim(charsToTrim);



            //get text + date
            split = split[1].Split(new string[] { "<TEXT>" }, StringSplitOptions.None);
            /*

            int dateStartIdx = split[0].IndexOf("<DATE1>");
            if (dateStartIdx > 0)
            {
                string date = split[0].Substring(dateStartIdx + 7, split[0].IndexOf("</DATE1>") - dateStartIdx - 7);
                d_docs[docName] = new Doc(docName, date.Trim(charsToTrim));
            }
            else
            {
                d_docs[docName] = new Doc(docName);
            }
            */

            split = split[1].Split(new string[] { "</TEXT>" }, StringSplitOptions.None);
            string text = split[0];
            //get terms from text
            //Dictionary<string, Term> d_terms = new Dictionary<string, Term>();
            //string t = "The 1999 23-25 23 January-23 a b 44% Ziv Kaspersky edition of the skypee 10.6 percent : Dollars 20.6m Dollars 5.3bn fgdf dfgdf  $100 million : Dollars 900,000 , Dollars 1.7320d January 23, 1999. feb 23, oct 1988, 1 oct 1988 between 18 and 24";
            int numOfTerms = getTerms(ref text, datesInOrderRegex, "Date", docName);
            numOfTerms += getTerms(ref text, datesMonthFirstRegex, "Date", docName);
            numOfTerms += getTerms(ref text, yearsRegex, "Year", docName);
            numOfTerms += getTerms(ref text, rangeReg, "Range", docName);
            numOfTerms += getTerms(ref text, percentReg, "Percent", docName);
            numOfTerms += getTerms(ref text, priceReg, "Price", docName);
            numOfTerms += getTerms(ref text, numReg, "Number", docName);
            numOfTerms += getTerms(ref text, namesReg, "Name", docName);
            numOfTerms += getTerms(ref text, wordRegex, "Word", docName);
            //return d_terms;

        }


        private void addTermToDic(string term, string docName, int index, ref int numOfTerms, string type)
        {
            if (!d_allTerms.ContainsKey(term))
            {
                d_allTerms[term] = new Term(type, term);
                numOfTerms++;

            }
            d_allTerms[term].addPosition(docName, index);
            /*
            // change max tf in doc if needed
            int tfDoc = d_allTerms[term].returnTF(docName);
            if (d_docs[docName].maxtfCount < tfDoc)
            {
                d_docs[docName].maxtfString = term;
                d_docs[docName].maxtfCount = tfDoc;
            }
             */

        }

        private int getTerms(ref string text, Regex regex, string type, string docName)
        {
            MatchCollection terms = regex.Matches(text);
            int numOfTerms = 0;
            foreach (Match term in terms)
            {
                string termString = term.ToString().ToLower().Replace('\n', ' ').Trim(charsToTrim);

                // Stop words
                if (StopWords.ContainsKey(termString) || termString.Length <= 2)
                    continue;




                if (type == "Price" || type == "Range" || type == "Percent" || type == "Price" || type == "Date" || type == "Year" || type == "Name")
                {
                    string clearTerm = new String('#', term.Length - 2);
                    text = text.Substring(0, term.Index) + " " + clearTerm + " " + text.Substring(term.Index + term.Length);
                }

                if (type == "Range")
                {
                    MatchCollection numbers = justANumberReg.Matches(termString);

                    int a, b;
                    int.TryParse(numbers[1].ToString(), out b);
                    int.TryParse(numbers[0].ToString(), out a);
                    if (Math.Abs(a - b) < 20)
                    {
                        for (; a <= b; a++)
                        {
                            addTermToDic(a.ToString(), docName, term.Index, ref numOfTerms, "Number");
                        }
                    }
                    else
                    {
                        addTermToDic(a.ToString(), docName, term.Index, ref numOfTerms, "Number");
                        addTermToDic(b.ToString(), docName, term.Index, ref numOfTerms, "Number");
                    }

                }
                else if (type == "Percent")
                {
                    string[] percentSplit = termString.Split(' ', '%');
                    float percent;
                    float.TryParse(percentSplit[0], out percent);
                    addTermToDic((percent * 0.01).ToString("P"), docName, term.Index, ref numOfTerms, "Percent");

                }
                else if (type == "Price")
                {

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

                    addTermToDic(price.ToString("C", new CultureInfo("en-US")), docName, term.Index, ref numOfTerms, "Price");

                }
                else if (type == "Date")
                {
                    try
                    {
                        //with th
                        int thIndex = termString.IndexOf("th");
                        if (thIndex >= 0)
                        {
                            termString = termString.Remove(thIndex, 2);
                        }

                        DateTime convertedDate = Convert.ToDateTime(termString);
                        termString = convertedDate.ToShortDateString();
                        addTermToDic(termString, docName, term.Index, ref numOfTerms, "Date");
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


                        addTermToDic(dd + "/" + mm + "/" + yyyy, docName, term.Index, ref numOfTerms, "Date");

                    }
                }
                else if (type == "Year")
                {
                    DateTime convertedDate = new DateTime(int.Parse(termString), 1, 1);
                    termString = convertedDate.ToShortDateString();
                    addTermToDic(termString, docName, term.Index, ref numOfTerms, "Date");
                }
                else // everything else
                {
                    //stemmer
                    if (use_stem)
                        termString = stemmer.stemTerm(termString);
                    addTermToDic(termString, docName, term.Index, ref numOfTerms, type);
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
