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
					//wiq = (double)qt.queryOccurence / (double)2;

					//term frequency in doc normalized by maxTf in doc
					tfij = (double)(qt.term.d_docTf[qd.docName]) / (double)maxTf;

					wij = idf * tfij;

					sigmaWijWiq += (double)wiq * wij;
					//sigmaWijSqr += Math.Pow(wij, 2);
					sigmaWiqSqr += Math.Pow(wiq, 2);
				}

				//calculate cosine
				double docsSigmaWijSqr = dDocs[qd.docName].sigmaWijSqr; //double docsSigmaWijSqr = CalculateDocsWij(qd.docName, maxTf, numOfDocsInEngine);
				double cosineDenominator = docsSigmaWijSqr * sigmaWiqSqr;
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
				//sigmaWijSqr = 0;
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

		/*
		private double CalculateDocsWij(string docName, int maxTf, int numOfDocsInEngine)
		{
			IFormatter formatter = new BinaryFormatter();
			Dictionary<string, int> d_document = (Dictionary<string, int>)formatter.Deserialize(new FileStream(postingPath + @"\docs\" + docName + @".bin", FileMode.Open, FileAccess.Read, FileShare.Read));

			double idf, tfij, wij;
			double sigmaWijSqr = 0;
			int td = 1;

			//calculate sigmaWij for ALL terms in doc
			foreach (KeyValuePair<string, int> tuple in d_document)
			{
				string termString = tuple.Key;
				//extract term's frequency in docs
				if (mainIndexList1.ContainsKey(termString))
					td = mainIndexList1[termString];
				else if (mainIndexList2.ContainsKey(termString))
					td = mainIndexList2[termString];
				else if (mainIndexList3.ContainsKey(termString))
					td = mainIndexList3[termString];
				else if (mainIndexList4.ContainsKey(termString))
					td = mainIndexList4[termString];
				else if (mainIndexList5.ContainsKey(termString))
					td = mainIndexList5[termString];

				//get idf value
				if (!(termsData.ContainsKey(tuple.Key)))
				{
					idf = Math.Log((numOfDocsInEngine / td), 2);
					termsData[tuple.Key] = idf;
				}
				else
					idf = termsData[tuple.Key];

				//calculate wij
				tfij = (double)tuple.Value / (double)maxTf;
				wij = tfij * idf;

				//sum sigmaWiSqr
				sigmaWijSqr += Math.Pow(wij, 2);
			}

			return sigmaWijSqr;
		}
		*/
    }
}
