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

        private Dictionary<string, bool> StopWords;
        Mutex mStopwords = new Mutex();

        private char[] charsToTrim = { ',', '.', ' ', ';', ':', '~', '|', '\n' };

        //regexs
        Regex numReg = new Regex(@"(\s|\()\d+(,\d{3})*(\.\d+)?(\s\d+\/\d+)?(\s(million|trillion|billion|hundreds))?");
        Regex rangeReg = new Regex(@"(\s|\()\d+(,\d{3})*(\.\d+)?(\s\d+\/\d+)?\-\d+(,\d{3})*(\.\d+)?(\s\d+\/\d+)?");
        Regex percentReg = new Regex(@"(\s|\()\d+(,\d{3})*(\.\d+)?(\s\d+\/\d+)?(\%|\s(percent|percents|percentage))");
        Regex priceReg = new Regex(@"(\s|\()(Dollars\s|\$)\d+(,\d{3})*(\.\d+)?(\s\d+\/\d+)?(m|bn|\smillion|\sbillion)?");
        Regex namesReg = new Regex(@"((\s|\()[A-Z][a-z]{1,})+");
        Regex datesRegex = new Regex(@"(([1-9]\d?(th)?\s)?(jan(uary)?|feb(ruary)?|mar(ch)?|apr(il)?|may|june?|july?|aug(ust)?|sep(tember)?|oct(ober)?|nov(ember)?|dec(ember)?)\s\d{1,4}(,\s\d{1,4})?)|\s\d{4}\s", RegexOptions.IgnoreCase);

        //my regexs
        Regex quoteRegex = new Regex(@"(\s|\()\x22(\s|\w|\d)*\x22"); // for quotes
        Regex capsRegex = new Regex(@"(\s|\()[A-Z]{2,}(\s[A-Z]{2,})*"); // for initials (like "JS") or headlines ("EMPLOYERS STAY OUT OF CIP DEBATE") 


        public Parse(string path)
        {
            StopWords = ReadFile.readStopWords(path + @"\StopWords.txt");
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
            new Regex(@"(jan(uary)?|feb(ruary)?|mar(ch)?|apr(il)?|may|june?|july?|aug(ust)?|sep(tember)?|oct(ober)?|nov(ember)?|dec(ember)?)", RegexOptions.IgnoreCase)
        }

        public bool startParseing(string path)
        {
            throw new NotImplementedException();
        }

        List<string> addMonths(List<string> month)
        {
            month.Add("january");
            month.Add("february");
            month.Add("march");
            month.Add("april");
            month.Add("may");
            month.Add("june");
            month.Add("july");
            month.Add("august");
            month.Add("september");
            month.Add("october");
            month.Add("november");
            month.Add("december");
            month.Add("jan");
            month.Add("feb");
            month.Add("mar");
            month.Add("apr");
            month.Add("may");
            month.Add("jun");
            month.Add("jul");
            month.Add("aug");
            month.Add("sep");
            month.Add("oct");
            month.Add("nov");
            month.Add("dec");
            return month;
        }

    }
}
