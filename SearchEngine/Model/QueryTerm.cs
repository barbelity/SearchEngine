using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Model
{
	class QueryTerm
	{

       //related term
		public Term term { get; set; }


		//number of docs this term exists
		public int td { get; set; }

		//how many occurences of term in query
		public int queryOccurence { get; set; }


		public QueryTerm(Term term, int td, int queryOccurence)
		{
			this.term = term;

			this.td = td;
			this.queryOccurence = queryOccurence;
		}

        public QueryTerm(Term term)
        {
            this.term = term;

            this.td = 0;
            this.queryOccurence = 1;
        }

    }
}
