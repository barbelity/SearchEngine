using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Model
{
    class Searcher
    {
        private Ranker ranker;

        public Searcher(Ranker ranker)
        {
            this.ranker = ranker;
        }

        internal void searchDocs(string query, Indexer indexer, int toMonth, int fromMonth)
        {
            Parse.d_abNumTerms = new SortedDictionary<string, Term>();
            Parse.d_cfTerms = new SortedDictionary<string, Term>();
            Parse.d_gmTerms = new SortedDictionary<string, Term>();
            Parse.d_nrTerms = new SortedDictionary<string, Term>();
            Parse.d_szTerms = new SortedDictionary<string, Term>();
            Parse.d_docs["Query"] = new Doc("Query", "");
            Parse.d_docs["Query"].d_TermsCount = new Dictionary<string, int>();
            Parse.getTermsFromString(" "+query+" ");
            List<Doc> l_relevantDocs = new List<Doc>();
            getReleventDocs(l_relevantDocs, Parse.d_abNumTerms, indexer.mainIndexList1);
            getReleventDocs(l_relevantDocs, Parse.d_cfTerms, indexer.mainIndexList2);
            getReleventDocs(l_relevantDocs, Parse.d_gmTerms, indexer.mainIndexList3);
            getReleventDocs(l_relevantDocs, Parse.d_nrTerms, indexer.mainIndexList4);
            getReleventDocs(l_relevantDocs, Parse.d_szTerms, indexer.mainIndexList5);



        }

        private void getReleventDocs(List<Doc> l_relevantDocs, SortedDictionary<string, Term> dict, SortedList<string, int> mainIndexList1)
        {
            
            foreach (var pair in mainIndexList1)
            {
                if (dict.ContainsKey(pair.Key))
                {

                }
            }
        }
    }
}
