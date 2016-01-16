using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SearchEngine.Model
{
    class Searcher
    {
        public delegate void Func(int type, string value);
        public event Func SearcherChanged;

        volatile bool _kill = false;

        Ranker ranker;
        char[] delimiterChars = { ' ' };
        internal Indexer _indexer;
        Dictionary<string, int> mutableQuery;
        SortedDictionary<string, QueryTerm> d_queryTerms;
        Dictionary<string, QueryDoc> queryDocs;
        List<string> result = new List<string>();
        internal int toMonth;
        internal int fromMonth;
        internal string queryText;

        public Searcher(Ranker ranker)
        {
            this.ranker = ranker;
            addMonths();
        }

        internal void searchDocs()
        {

            mutableQuery = new Dictionary<string, int>();
            string[] splitedQuery = queryText.Split(' ');
            for (int i = 0; i < splitedQuery.Length; i++)
            {
                string mutableTerm;
                if (splitedQuery[i][0] == '%' && splitedQuery[i][splitedQuery[i].Length - 1] == '%')
                {
                    mutableTerm = splitedQuery[i].Substring(1, splitedQuery[i].Length - 2);
                    if (mutableQuery.ContainsKey(mutableTerm))
                    {
                        mutableQuery[mutableTerm] += 1;
                    }
                    else
                    {
                        mutableQuery[mutableTerm] = 1;
                    }
                    splitedQuery[i] = "";
                }

            }
            queryText = string.Join(" ", splitedQuery);
            Parse.d_abNumTerms = new SortedDictionary<string, Term>();
            Parse.d_cfTerms = new SortedDictionary<string, Term>();
            Parse.d_gmTerms = new SortedDictionary<string, Term>();
            Parse.d_nrTerms = new SortedDictionary<string, Term>();
            Parse.d_szTerms = new SortedDictionary<string, Term>();
            Parse.d_docs["Query"] = new Doc("Query", "");
            Parse.d_docs["Query"].d_TermsCount = new Dictionary<string, int>();
            Parse.getTermsFromString(" " + queryText + " ");
            d_queryTerms = new SortedDictionary<string, QueryTerm>();
            Thread[] a_Threads = new Thread[5];
            a_Threads[0] = new Thread(() => getReleventDocs(Parse.d_abNumTerms, _indexer.mainIndexList1, "abNumsPosting.txt"));
            a_Threads[1] = new Thread(() => getReleventDocs(Parse.d_cfTerms, _indexer.mainIndexList2, "cfPosting.txt"));
            a_Threads[2] = new Thread(() => getReleventDocs(Parse.d_gmTerms, _indexer.mainIndexList3, "gmPosting.txt"));
            a_Threads[3] = new Thread(() => getReleventDocs(Parse.d_nrTerms, _indexer.mainIndexList4, "nrPosting.txt"));
            a_Threads[4] = new Thread(() => getReleventDocs(Parse.d_szTerms, _indexer.mainIndexList5, "szPosting.txt"));
            foreach (Thread t in a_Threads)
            {
                t.Start();
            }
            foreach (Thread t in a_Threads)
            {
                t.Join();
            }
            //remove irrelevant dates and creates queryDocs list
            queryDocs = new Dictionary<string, QueryDoc>();
            List<QueryTerm> l_temp;
            DateTime date;
            if (_kill) return;
            foreach (var stringQTermPair in d_queryTerms)
            {
                if (_kill) return;
                foreach (var docNameLocationsPair in stringQTermPair.Value.term.d_locations)
                {
                    if (!queryDocs.ContainsKey(docNameLocationsPair.Key))
                    {
                        if (Parse.d_docs[docNameLocationsPair.Key].date.Length < 5)//no date
                        {
                            l_temp = new List<QueryTerm>();
                            l_temp.Add(stringQTermPair.Value);
                            queryDocs[docNameLocationsPair.Key] = new QueryDoc(docNameLocationsPair.Key, l_temp);
                            //Console.WriteLine(docNameLocationsPair.Key + " doc was added");
                        }
                        else try
                            {
                                string sdate = Parse.d_docs[docNameLocationsPair.Key].date.Split(delimiterChars)[1].ToLower();
                                if (months.ContainsKey(sdate) && (toMonth == 0 || months[sdate] <= toMonth) && months[sdate] >= fromMonth)
                                {
                                    l_temp = new List<QueryTerm>();
                                    l_temp.Add(stringQTermPair.Value);
                                    queryDocs[docNameLocationsPair.Key] = new QueryDoc(docNameLocationsPair.Key, l_temp);
                                }
                                else if (!months.ContainsKey(sdate)){
                                    date = Convert.ToDateTime(Parse.d_docs[docNameLocationsPair.Key].date);
                                    if ((toMonth == 0 || date.Month <= toMonth) && date.Month >= fromMonth)
                                    {
                                        l_temp = new List<QueryTerm>();
                                        l_temp.Add(stringQTermPair.Value);
                                        queryDocs[docNameLocationsPair.Key] = new QueryDoc(docNameLocationsPair.Key, l_temp);
                                        //Console.WriteLine(docNameLocationsPair.Key + " doc was added");
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                l_temp = new List<QueryTerm>();
                                l_temp.Add(stringQTermPair.Value);
                                queryDocs[docNameLocationsPair.Key] = new QueryDoc(docNameLocationsPair.Key, l_temp);
                            }
                    }
                    else
                    {
                        queryDocs[docNameLocationsPair.Key].queryTerm.Add(stringQTermPair.Value);
                        //Console.WriteLine(docNameLocationsPair.Key + " doc has a term added");
                    }
                }
            }
            result = ranker.StartRanking(queryDocs, Parse.d_docs);
            SearcherChanged(2, "");

        }

        internal List<string> getResult()
        {
            return result;
        }

        private void getReleventDocs(SortedDictionary<string, Term> dict, SortedList<string, int> mainIndexList1, string fileName)
        {
            using (StreamReader postingReader = new StreamReader(_indexer.postingPath + "\\" + fileName))
            {
                string line;
                foreach (var listPair in mainIndexList1) //for every term in this dictionary
                {
                    if (_kill) return;
                    line = postingReader.ReadLine();

                    if (dict.ContainsKey(listPair.Key)) // this term in query
                        lock (d_queryTerms)
                        {

                            d_queryTerms[listPair.Key] = new QueryTerm(_indexer.convertPostingStringToTerm(line));
                            d_queryTerms[listPair.Key].td = dict[listPair.Key].d_locations["Query"].ToString().Count(f => f == ',');
                            //Console.WriteLine(d_queryTerms[listPair.Key].term.termString + " term was added");
                        }
                    else if (mutableQuery.Count != 0)//this is a part of term
                        foreach (var pair in mutableQuery)
                            if (listPair.Key.Contains(pair.Key))
                                lock (d_queryTerms)
                                    if (!d_queryTerms.ContainsKey(listPair.Key))
                                    {
                                        d_queryTerms[listPair.Key] = new QueryTerm(_indexer.convertPostingStringToTerm(line));
                                        d_queryTerms[listPair.Key].td = pair.Value;
                                        //Console.WriteLine(d_queryTerms[listPair.Key].term.termString + " mutable term was added");
                                    }
                                    else
                                    {
                                        d_queryTerms[listPair.Key].td += pair.Value;
                                        //Console.WriteLine(d_queryTerms[listPair.Key].term.termString + " mutable term was added tf");
                                    }
                }
            }
        }

        internal void kill()
        {
            _kill = true;
        }
        static Dictionary<string, int> months = new Dictionary<string, int>();
        private void addMonths()
        {
            months.Add("january", 1);
            months.Add("february", 2);
            months.Add("march", 3);
            months.Add("april", 4);
            months.Add("may", 5);
            months.Add("june", 6);
            months.Add("july", 7);
            months.Add("august", 8);
            months.Add("september", 09);
            months.Add("october", 10);
            months.Add("november", 11);
            months.Add("december", 12);
            months.Add("jan", 1);
            months.Add("feb", 2);
            months.Add("mar", 3);
            months.Add("apr", 4);
            months.Add("jun", 6);
            months.Add("jul", 7);
            months.Add("aug", 8);
            months.Add("sep", 9);
            months.Add("oct", 10);
            months.Add("nov", 11);
            months.Add("dec", 12);

        }
    }
}
