using System;
using System.Collections.Generic;
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


        private char[] charsToTrim = { ',', '.', ' ', ';', ':', '~', '|', '\n' };

        //regexs
        Regex numReg = new Regex(@"\d+(,\d{3})*(\.\d+)?(\s\d+\/\d+)?(\s(million|trillion|billion|hundreds))?");
        Regex rangeReg = new Regex(@"\d+(,\d{3})*(\.\d+)?(\s\d+\/\d+)?\-\d+(,\d{3})*(\.\d+)?(\s\d+\/\d+)?");
        Regex percentReg = new Regex(@"\d+(,\d{3})*(\.\d+)?(\s\d+\/\d+)?(\%|\s(percent|percents|percentage))");
        Regex priceReg = new Regex(@"(Dollars\s|\$)\d+(,\d{3})*(\.\d+)?(\s\d+\/\d+)?(m|bn|\smillion|\sbillion)?");
        Regex namesReg = new Regex(@"([A-Z][a-z]{1,})+");
        Regex datesRegex = new Regex(@"(([1-9]\d?(th)?\s)?(jan(uary)?|feb(ruary)?|mar(ch)?|apr(il)?|may|june?|july?|aug(ust)?|sep(tember)?|oct(ober)?|nov(ember)?|dec(ember)?)\s\d{1,4}(,\s\d{1,4})?)|\s\d{4}\s", RegexOptions.IgnoreCase);

        //my regexs
        Regex quoteRegex = new Regex(@"\x22(\s|\w|\d)*\x22"); // for quotes
        Regex capsRegex = new Regex(@"[A-Z]{2,}(\s[A-Z]{2,})*"); // for initials (like "JS") or headlines ("EMPLOYERS STAY OUT OF CIP DEBATE") 

        //for date fix
        Regex justMonthReg = new Regex(@"(jan(uary)?|feb(ruary)?|mar(ch)?|apr(il)?|may|june?|july?|aug(ust)?|sep(tember)?|oct(ober)?|nov(ember)?|dec(ember)?)", RegexOptions.IgnoreCase);


        public Parse(string path)
        {
            StopWords = ReadFile.readStopWords(path + @"\StopWords.txt");
            addMonths();
        }


        public void ParserDocs(List<string> docs)
        {

            foreach (var docString in docs)
            {
                Doc doc = new Doc(docString);

            }
            //WaitHandle.WaitAll(doneEvents);
            Console.WriteLine("All calculations are complete.");
        }


        internal void startParsing(string filesPath)
        {
            string[] paths = ReadFile.getFilesPaths(filesPath);
            foreach (string filePath in paths)
            {
                string[] docs = ReadFile.fileToDocString(filePath);
                foreach (string doc in docs)
                {
                    Dictionary<Term, Positions> temp = parseDoc(doc);
                }
            }
        }



        public Dictionary<Term, Positions> parseDoc(string doc)
        {
            //get name
            string[] split = doc.Split(new string[] { "<DOCNO>" }, StringSplitOptions.None);
            split = split[1].Split(new string[] { "</DOCNO>" }, StringSplitOptions.None);
            string name = split[0];
            //get text
            split = split[1].Split(new string[] { "<TEXT>" }, StringSplitOptions.None);
            split = split[1].Split(new string[] { "</TEXT>" }, StringSplitOptions.None);
            string text = split[0];
            //get terms from text
            Dictionary<Term, Positions> terms = new Dictionary<Term, Positions>();
            int numOfTerms = getTerms(ref text, ref terms, datesRegex, "Date", name);
            return terms;

        }

        private int getTerms(ref string text, ref Dictionary<Term, Positions> d_terms, Regex regex, string type, string docName)
        {
            MatchCollection terms = regex.Matches(text);
            int numOfTerms = 0;
            foreach (Match term in terms)
            {
                string termString = term.ToString().ToLower().Replace('\n', ' ').Trim(charsToTrim);

                // Stop words
                if (StopWords.ContainsKey(termString) || term.Length <= 2)
                    continue;

                if (type == "Date" || type == "Price")
                {
                    string clearTerm = new String('*', term.Length);
                    text = text.Substring(0, term.Index) + clearTerm + text.Substring(term.Index + term.Length);
                }
                if (type == "Date") termString = fixDate(termString);


                //if (RetrievalEngineProject.MainWindow.use_stem)
                //    termString = stem.stemTerm(termString);

                Term tempTerm = new Term(type, termString);
                if (d_terms.ContainsKey(tempTerm))
                {

                    d_terms[tempTerm].addPosition(term.Index);


                }
                else
                {
                    // Creating a new Term
                    Term t = new Term(type, termString);
                    d_terms[tempTerm] = new Positions(docName);
                    numOfTerms++;

                }



            }
            return numOfTerms;
        }

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

        public bool startParseing(string path)
        {
            throw new NotImplementedException();
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
            months.Add("may", "05");
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
