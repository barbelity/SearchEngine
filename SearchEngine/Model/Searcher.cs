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
        private Ranker ranker;

        Indexer _indexer;
        Dictionary<string, int> mutableQuery;
        SortedDictionary<string, QueryTerm> d_queryTerms;



        public Searcher(Ranker ranker)
        {
            this.ranker = ranker;
        }

        internal void searchDocs(string query, Indexer indexer, int toMonth, int fromMonth)
        {
            _indexer = indexer;
            mutableQuery = new Dictionary<string, int>();
            string[] splitedQuery = query.Split(' ');
            for (int i = 0; i < splitedQuery.Length; i++)
            {
                if (splitedQuery[i].Contains('%'))
                {
                    if (mutableQuery.ContainsKey(splitedQuery[i]))
                    {
                        mutableQuery[splitedQuery[i]] += 1;
                    }
                    else
                    {
                        mutableQuery[splitedQuery[i]] = 1;
                    }
                    splitedQuery[i] = "";
                }

            }
            query = string.Join(" ", splitedQuery);
            Parse.d_abNumTerms = new SortedDictionary<string, Term>();
            Parse.d_cfTerms = new SortedDictionary<string, Term>();
            Parse.d_gmTerms = new SortedDictionary<string, Term>();
            Parse.d_nrTerms = new SortedDictionary<string, Term>();
            Parse.d_szTerms = new SortedDictionary<string, Term>();
            Parse.d_docs["Query"] = new Doc("Query", "");
            Parse.d_docs["Query"].d_TermsCount = new Dictionary<string, int>();
            Parse.getTermsFromString(" " + query + " ");
            d_queryTerms = new SortedDictionary<string, QueryTerm>();
            Thread[] a_Threads = new Thread[5];
            a_Threads[0] = new Thread(() => getReleventDocs(Parse.d_abNumTerms, indexer.mainIndexList1, "abNumsPosting.txt"));
            a_Threads[1] = new Thread(() => getReleventDocs(Parse.d_cfTerms, indexer.mainIndexList2, "cfPosting.txt"       ));
            a_Threads[2] = new Thread(() => getReleventDocs(Parse.d_gmTerms, indexer.mainIndexList3, "gmPosting.txt"       ));
            a_Threads[3] = new Thread(() => getReleventDocs(Parse.d_nrTerms, indexer.mainIndexList4, "nrPosting.txt"       ));
            a_Threads[4] = new Thread(() => getReleventDocs(Parse.d_szTerms, indexer.mainIndexList5, "szPosting.txt"       ));
            foreach (Thread t in a_Threads)
            {
                t.Join();
            }
        }



        private void getReleventDocs(SortedDictionary<string, Term> dict, SortedList<string, int> mainIndexList1, string fileName)
        {
            using (StreamReader postingReader = new StreamReader(_indexer.postingPath + "\\" + fileName))
            {
                string line;
                foreach (var listPair in mainIndexList1) //for every term in this dictionary
                {
                    line = postingReader.ReadLine();

                    if (dict.ContainsKey(listPair.Key)) // this term in query
                        lock (d_queryTerms)
                        {
                            d_queryTerms[listPair.Key] = new QueryTerm(getTermFromString(line));
                            d_queryTerms[listPair.Key].td = dict[listPair.Key].d_locations["Query"].ToString().Count(f => f == ',');
                        }
                    else if (mutableQuery.Count != 0)//this is a part of term
                        foreach (var pair in mutableQuery)
                            if (listPair.Key.Contains(pair.Key))
                                lock (d_queryTerms)
                                    if (!d_queryTerms.ContainsKey(listPair.Key))
                                    {
                                        d_queryTerms[listPair.Key] = new QueryTerm(getTermFromString(line));
                                        d_queryTerms[listPair.Key].td = mutableQuery[listPair.Key];
                                    }
                }
            }
        }

        private Term getTermFromString(string line)
        {
            throw new NotImplementedException();
        }
    }
}
