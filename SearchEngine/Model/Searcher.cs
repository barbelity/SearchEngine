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
            Parse.getTermsFromString(query);
            
        }
    }
}
