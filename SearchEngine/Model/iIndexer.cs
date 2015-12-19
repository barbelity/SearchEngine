using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Model
{
    interface iIndexer
    {
		void startIndexing(ref SortedDictionary<string, Term>[] d_terms);
        void saveLists();
    }
}
