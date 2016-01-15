using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Model
{
    class Ranker
    {

		private List<string> StartRanking(List<QueryDoc> queryDocs, int numOfDocsInEngine)
		{
			List<string> ans = new List<string>();
			Dictionary<string, int> docRanks = new Dictionary<string, int>();

			//this dictionary will be initialized for each document, holds term
			Dictionary<string, int> docTerms = new Dictionary<string, int>();

			int wiq = 0;
			int wij = 0;


			foreach (QueryDoc qd in queryDocs)
			{
				foreach (QueryTerm qt in qd.queryTerm)
				{

				}





			}





			return ans;

		}



    }
}
