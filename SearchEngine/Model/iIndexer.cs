using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Model
{
    interface iIndexer
    {
		void startIndexing(SortedDictionary<string, Term> d_DateTerms, SortedDictionary<string, Term> d_WordTerms, SortedDictionary<string, Term> d_PercentallTerms, SortedDictionary<string, Term> d_PriceTerms, SortedDictionary<string, Term> d_NumberTerms);
    }
}
