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

		//occurences in current document
		public int tf { get; set; }

		//number of docs this term exists
		public int td { get; set; }

		//how many occurences of term in query
		public int queryOccurence { get; set; }


		public QueryTerm(Term term, int tf, int td, int queryOccurence)
		{
			this.term = term;
			this.tf = tf;
			this.td = td;
			this.queryOccurence = queryOccurence;
		}

	}
}
