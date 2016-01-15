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

		//string is doc name, int is df
		public List<KeyValuePair<string, int>> termInDoc { get; set; }

		//how many occurences of term in query
		public int queryOccurence { get; set; }


		public QueryTerm(Term t)
		{
			term = t;
			termInDoc = new List<KeyValuePair<string, int>>();
			queryOccurence = 1;
		}

		public QueryTerm(Term t, List<KeyValuePair<string, int>> tid, int occ)
        {
			term = t;
			termInDoc = tid;
			queryOccurence = occ;

        }



	}
}
