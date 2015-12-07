using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Model
{
    class Indexer : iIndexer
    {

        public void saveTerms(Dictionary<string, Term> terms)
        {
            throw new NotImplementedException();
            //add the terms received to a temp dictionary
            //calculate doc's info
            //save doc info to docs dictionary on memory
            //after X time, write all data to posting file
            //keep dictionary on memory for indexing
        }

        public void startIndexing()
        {
            throw new NotImplementedException();
        }

        public void saveTerms(Dictionary<Term, Positions> terms)
        {
            throw new NotImplementedException();
        }
    }
}
