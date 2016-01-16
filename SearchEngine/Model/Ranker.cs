using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Model
{
    class Ranker
    {
		private Dictionary<string, double> termsData;

		/// <summary>
		/// Constructor
		/// </summary>
		public Ranker()
		{
			termsData = new Dictionary<string, double>();
		}

		/// <summary>
		/// evaluates the relevant documents according to a specific query
		/// </summary>
		/// <param name="queryDocs">list of relevant documents to be evaluated with the required data for the evaluating process</param>
		/// <param name="dDocs">documents metada</param>
		/// <returns>list of QueryDocs, sorted from the most relevant to the least</returns>
		private List<string> StartRanking(List<QueryDoc> queryDocs, Dictionary<string, Doc> dDocs)
		{
			List<string> ans = new List<string>();
			SortedList<double, List<string>> docsRanks = new SortedList<double, List<string>>();

			//Dictionary<string, double> docRanks = new Dictionary<string, double>();
			////this dictionary will be initialized for each document, holds term
			//Dictionary<string, int> docTerms = new Dictionary<string, int>();

			int wiq, maxTf;
			double wij, idf, tfij;
			double sigmaWijWiq = 0;
			double sigmaWijSqr = 0;
			double sigmaWiqSqr = 0;
			int numOfDocsInEngine = dDocs.Count;

			foreach (QueryDoc qd in queryDocs)
			{
				maxTf = dDocs[qd.docName].maxtfCount;

				foreach (QueryTerm qt in qd.queryTerm)
				{

					//reuse idf values to avoid recalculation
					if (!(termsData.ContainsKey(qt.term.termString)))
					{
						idf = Math.Log((numOfDocsInEngine / qt.td), 2);
						termsData[qt.term.termString] = idf;
					}
					else
						idf = termsData[qt.term.termString];

					wiq = qt.queryOccurence;
					//term frequency in doc normalized by maxTf in doc
					tfij = (qt.term.d_docTf[qd.docName]) / maxTf;
					wij = idf * tfij;

					sigmaWijWiq += wiq * wij;
					sigmaWijSqr += Math.Pow(wij, 2);
					sigmaWiqSqr += Math.Pow(wiq, 2);
				}

				//calculate cosine
				double cosineDenominator = sigmaWijSqr + sigmaWiqSqr;
				cosineDenominator = Math.Sqrt(cosineDenominator);
				double cosine = sigmaWijWiq / cosineDenominator;
				
				//insert result to docsRanks
				if (docsRanks.ContainsKey(cosine))
				{
					docsRanks[cosine].Add(qd.docName);
				}
				else
				{
					List<string> toAdd = new List<string>();
					toAdd.Add(qd.docName);
					docsRanks.Add(cosine, toAdd);
				}
				//docRanks[qd.docName] = cosine;

				sigmaWijWiq = 0;
				sigmaWijSqr = 0;
				sigmaWiqSqr = 0;
			}

			//now i have docsranks, need to extract top 50
			int resultsCount = 0;
			KeyValuePair<double, List<string>> docList;
			for (int i = docsRanks.Count - 1; i >= 0; i-- )
			{
				if (resultsCount > 50)
					break;

				docList = docsRanks.ElementAt(i);
				foreach (string doc in docList.Value)
				{
					ans.Add(doc);
					resultsCount++;
					if (resultsCount > 50)
						break;
				}
			}
			
			return ans;

		}
    }
}
