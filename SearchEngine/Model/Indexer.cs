using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Model
{
    class Indexer : iIndexer
    {
		private int docsToFile;
		public int DocsToFile 
		{
			get { return docsToFile;}
			set { docsToFile = value;}
		}

		private Dictionary<string, Positions> tempIndex;
		private int docsCounter = 0;

		public Indexer()
		{
			this.docsToFile = 10;
			tempIndex = new Dictionary<string, Positions>();
		}

        public void saveTerms(Dictionary<string, Term> terms)
        {
			//add the terms received to a temp dictionary
			//calculate doc's info
			//save doc info to docs dictionary on memory
			//after X time, write all data to posting file
			//keep dictionary on memory for indexing
			if (docsCounter == docsToFile)
			{
				try
				{
					writeTermsToFile();
					tempIndex.Clear();
				}
				catch(Exception e)
				{
					Console.WriteLine("Exception writing to file");
					throw e;
				}
			}
			else
			{
				foreach (KeyValuePair<string, Positions> t in terms)
				{

				}
			}
			


            throw new NotImplementedException();
            //add the terms received to a temp dictionary
            //calculate doc's info
            //save doc info to docs dictionary on memory
            //after X time, write all data to posting file
            //keep dictionary on memory for indexing
        }

		private void writeTermsToFile()
		{
			throw new NotImplementedException();
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
