using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Model
{
    class Ranker
    {
		private Dictionary<string, double> termsData;
		public string postingPath;

		public SortedList<string, int> mainIndexList1;
		public SortedList<string, int> mainIndexList2;
		public SortedList<string, int> mainIndexList3;
		public SortedList<string, int> mainIndexList4;
		public SortedList<string, int> mainIndexList5;
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
		internal List<string> StartRanking(Dictionary<string, QueryDoc> queryDocs, Dictionary<string, Doc> dDocs)
		{
			List<string> ans = new List<string>();
			SortedList<double, List<string>> docsRanks = new SortedList<double, List<string>>();

			//calculates number of terms in query
			int termsInQuery = Parse.d_abNumTerms.Count + Parse.d_cfTerms.Count + Parse.d_gmTerms.Count + Parse.d_nrTerms.Count + Parse.d_szTerms.Count;
			//Dictionary<string, double> docRanks = new Dictionary<string, double>();
			////this dictionary will be initialized for each document, holds term
			//Dictionary<string, int> docTerms = new Dictionary<string, int>();

			int maxTf;
			double wij, idf, tfij, wiq;
			double sigmaWijWiq = 0;
			//double sigmaWijSqr = 0;
			double sigmaWiqSqr = 0;
			int numOfDocsInEngine = dDocs.Count;

			foreach (QueryDoc qd in queryDocs.Values)
			{
				maxTf = dDocs[qd.docName].maxtfCount;

				foreach (QueryTerm qt in qd.queryTerm)
				{

					//reuse idf values to avoid recalculation
					if (!(termsData.ContainsKey(qt.term.termString)))
					{
						idf = Math.Log((numOfDocsInEngine / qt.term.d_docTf.Count), 2);
						termsData[qt.term.termString] = idf;
					}
					else
						idf = termsData[qt.term.termString];

					wiq = 1;

					//term frequency in doc normalized by maxTf in doc
					tfij = (double)(qt.term.d_docTf[qd.docName]) / (double)maxTf;

					//mult tfij by 1.2 if term appears in doc's header
					if (qt.term.d_docHeader[qd.docName] == true)
						tfij *= 1.2;

					wij = idf * tfij;

					sigmaWijWiq += (double)wiq * wij;
					sigmaWiqSqr += Math.Pow(wiq, 2);
				}


				//calculate cosine
				double docsSigmaWijSqr = dDocs[qd.docName].sigmaWijSqr;
				double cosineDenominator = docsSigmaWijSqr * sigmaWiqSqr;
				cosineDenominator = Math.Sqrt(cosineDenominator);
				if (termsInQuery == qd.queryTerm.Count)
					sigmaWijWiq *= 1.2;
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

				sigmaWijWiq = 0;
				sigmaWiqSqr = 0;
			}

			//now i have docs ranks, need to extract top 50
			int resultsCount = 0;
			KeyValuePair<double, List<string>> docList;
			for (int i = docsRanks.Count - 1; i >= 0; i-- )
			{
				if (resultsCount == 50)
					break;

				docList = docsRanks.ElementAt(i);
				foreach (string doc in docList.Value)
				{
					ans.Add(doc);
					resultsCount++;
					if (resultsCount == 50)
						break;
				}
			}
			
			return ans;

		}

    }
}
