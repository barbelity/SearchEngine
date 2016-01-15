using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Model
{
	class QueryDoc
	{

		//name of document
		public string docName { get; set; } 

		//doc's value related to current query
		public QueryTerm queryTerm { get; set; }

		public QueryDoc(string docName, QueryTerm queryTerm)
		{
			this.docName = docName;
			this.queryTerm = queryTerm;
		}
	}
}
