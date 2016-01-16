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
    class Indexer
    {
		public SortedList<string, int> mainIndexList1;
		public SortedList<string, int> mainIndexList2;
		public SortedList<string, int> mainIndexList3;
		public SortedList<string, int> mainIndexList4;
		public SortedList<string, int> mainIndexList5;

        StreamReader postingReader;
        StreamWriter tempWriter;
        internal string postingPath;
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="postingPath">path to save the posting to</param>
        public Indexer(string postingPath)
        {
            this.postingPath = postingPath;
			mainIndexList1 = new SortedList<string, int>();
			mainIndexList2 = new SortedList<string, int>();
			mainIndexList3 = new SortedList<string, int>();
			mainIndexList4 = new SortedList<string, int>();
			mainIndexList5 = new SortedList<string, int>();
        }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="postingPath">path to save the posting to</param>
		/// <param name="v">load posting from files</param>
        public Indexer(string postingPath, bool v)
        {
            this.postingPath = postingPath;
            IFormatter formatter = new BinaryFormatter();

            try
            {
                mainIndexList1 = (SortedList<string, int>)formatter.Deserialize(new FileStream(postingPath + @"\list1.bin", FileMode.Open, FileAccess.Read, FileShare.Read));
                mainIndexList2 = (SortedList<string, int>)formatter.Deserialize(new FileStream(postingPath + @"\list2.bin", FileMode.Open, FileAccess.Read, FileShare.Read));
                mainIndexList3 = (SortedList<string, int>)formatter.Deserialize(new FileStream(postingPath + @"\list3.bin", FileMode.Open, FileAccess.Read, FileShare.Read));
                mainIndexList4 = (SortedList<string, int>)formatter.Deserialize(new FileStream(postingPath + @"\list4.bin", FileMode.Open, FileAccess.Read, FileShare.Read));
                mainIndexList5 = (SortedList<string, int>)formatter.Deserialize(new FileStream(postingPath + @"\list5.bin", FileMode.Open, FileAccess.Read, FileShare.Read));
                Parse.d_docs = (Dictionary<string, Doc>)formatter.Deserialize(new FileStream(postingPath + @"\Doc.bin", FileMode.Open, FileAccess.Read, FileShare.Read));
                Parse.StopWords = ReadFile.readStopWords(postingPath + @"\stop_words.txt");
            }
            catch (Exception e)
            {

                throw;
            }

        }

		/// <summary>
		/// save terms to posting file
		/// </summary>
		/// <param name="terms">dictionary of terms</param>
		/// <param name="fileName">name of posting file</param>
		/// <param name="mainIndexList">the index in memory which saves the posting's data</param>
        public void saveTerms(SortedDictionary<string, Term> terms, string fileName, SortedList<string, int> mainIndexList)
        {
            if (Parse.stop) return;
            int lineCount = 0;
            int writeLineNumber;

            int insertedTerms = 0;
            string currTerm;
            string currLine = "";
            fileName = postingPath + "\\" + fileName;

            int currDf;
            postingReader = new StreamReader(fileName);
            if (!File.Exists("tempFile.txt"))
                tempWriter = File.CreateText("tempFile.txt");
            else
                tempWriter = new StreamWriter("tempFile.txt");

            foreach (KeyValuePair<string, Term> tuple in terms)
            {
                currTerm = tuple.Key;
                currDf = tuple.Value.d_locations.Count;
                if (mainIndexList.ContainsKey(currTerm))
                {
                    mainIndexList[currTerm] += currDf;
					writeLineNumber = mainIndexList.IndexOfKey(currTerm) - insertedTerms;

                    for (; lineCount < writeLineNumber; lineCount++)
                    {
                        tempWriter.WriteLine(postingReader.ReadLine());
                    }

                    currLine = postingReader.ReadLine();
                    currLine += tuple.Value.ToString();
                    tempWriter.WriteLine(currLine);
                    lineCount++;
                }
                else
                {
                    mainIndexList[currTerm] = currDf;
                    writeLineNumber = mainIndexList.IndexOfKey(currTerm);

                    for (; lineCount < writeLineNumber - insertedTerms; lineCount++)
                    {
                        tempWriter.WriteLine(postingReader.ReadLine());
                    }

                    tempWriter.WriteLine(currTerm + "@" + tuple.Value.ToString());
                    insertedTerms++;
                }
            }

            while (!postingReader.EndOfStream)
            {
                tempWriter.WriteLine(postingReader.ReadLine());
            }

            postingReader.Close();
            tempWriter.Close();

            File.Move(fileName, fileName + ".old");
			File.Move("tempFile.txt", fileName);
			File.Delete(fileName + ".old");
        }

		/// <summary>
		/// manages the update of posting files
		/// </summary>
		/// <param name="d_terms">pointer to array of dictionaries of terms</param>
        public void startIndexing(ref SortedDictionary<string, Term>[] d_terms)
        {
			saveTerms(d_terms[0], "abNumsPosting.txt", mainIndexList1);
            d_terms[0] = null;
            saveTerms(d_terms[1], "cfPosting.txt", mainIndexList2);
            d_terms[1] = null;
            saveTerms(d_terms[2], "gmPosting.txt", mainIndexList3);
            d_terms[2] = null;
            saveTerms(d_terms[3], "nrPosting.txt", mainIndexList4);
            d_terms[3] = null;
            saveTerms(d_terms[4], "szPosting.txt", mainIndexList5);
            d_terms[4] = null;
        }

		/// <summary>
		/// serializes the indexes to files
		/// </summary>
        public void saveLists()
        {
            File.Delete(postingPath + @"\list1.bin");
            File.Delete(postingPath + @"\list2.bin");
            File.Delete(postingPath + @"\list3.bin");
            File.Delete(postingPath + @"\list4.bin");
            File.Delete(postingPath + @"\list5.bin");
            File.Delete(postingPath + @"\Doc.bin");
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(new FileStream(postingPath + @"\list1.bin", FileMode.Create, FileAccess.Write, FileShare.None), mainIndexList1);
            formatter.Serialize(new FileStream(postingPath + @"\list2.bin", FileMode.Create, FileAccess.Write, FileShare.None), mainIndexList2);
            formatter.Serialize(new FileStream(postingPath + @"\list3.bin", FileMode.Create, FileAccess.Write, FileShare.None), mainIndexList3);
            formatter.Serialize(new FileStream(postingPath + @"\list4.bin", FileMode.Create, FileAccess.Write, FileShare.None), mainIndexList4);
            formatter.Serialize(new FileStream(postingPath + @"\list5.bin", FileMode.Create, FileAccess.Write, FileShare.None), mainIndexList5);
            formatter.Serialize(new FileStream(postingPath + @"\Doc.bin", FileMode.Create, FileAccess.Write, FileShare.None), Parse.d_docs);
            Parse.StopWords = ReadFile.readStopWords(postingPath + @"\stop_words.txt");
        }

		/// <summary>
		/// builds a string of posting results
		/// </summary>
		/// <returns>result string</returns>
        public string getPostingString()
        {

            StringBuilder ans = new StringBuilder();

            ans.Append(ListToString(mainIndexList1));
            ans.Append(ListToString(mainIndexList2));
            ans.Append(ListToString(mainIndexList3));
            ans.Append(ListToString(mainIndexList4));
            ans.Append(ListToString(mainIndexList5));

            return ans.ToString();

        }

		/// <summary>
		/// converts an index-list to string
		/// </summary>
		/// <param name="mainIndexList">index-list to write</param>
		/// <returns>result string</returns>
        private StringBuilder ListToString(SortedList<string, int> mainIndexList)
        {
            StringBuilder ans = new StringBuilder();
            foreach (var item in mainIndexList)
            {
                ans.Append(item.Key + ": " + item.Value.ToString()+"\n");
            }
            return ans;
        }

        internal int getNumOfTerms()
        {
            int ans = mainIndexList1.Count;
            ans += mainIndexList2.Count;
            ans += mainIndexList3.Count;
            ans += mainIndexList4.Count;
            ans += mainIndexList5.Count;
            return ans;
        }


		public Term convertPostingStringToTerm(string line)
		{
			Term result = new Term();
			string termString;
			//string type;
			Dictionary<string, StringBuilder> dLocations = new Dictionary<string, StringBuilder>();
			Dictionary<string, int> dDocTf = new Dictionary<string, int>();

			//extract term string
			int termEndIndex = line.IndexOf('@');
			termString = line.Substring(0, termEndIndex);
			//cut the term string
			line = line.Substring(termEndIndex + 1);

			//extract documents and positions
			string[] docsDivisionArray = line.Split('|');

			foreach (string docString in docsDivisionArray)
			{
				if (docString.Length < 1)
					continue;
				int currLoc, docTf;//, tfCount = 0;
				string docName;
				StringBuilder sb = new StringBuilder();

				//extracting the docName
				currLoc = docString.IndexOf(';');
				docName = docString.Substring(0, currLoc);
				//removing docName
				string docLocationsString = docString.Substring(currLoc + 1);

				//extracting docTf
				currLoc = docLocationsString.IndexOf(':');
				docTf = Int32.Parse(docLocationsString.Substring(0, currLoc));
				//removing docTf
				docLocationsString = docLocationsString.Substring(currLoc + 1);

				string[] locationsDivisionArray = docLocationsString.Split(',');

				//iterating over locations
				foreach (string locString in locationsDivisionArray)
				{
					if (locString.Length < 1)
						continue;
					sb.Append(locString + ",");
					//tfCount++;
				}

				//updating dLocations and dDocTf with data
				dLocations[docName] = sb;
				dDocTf[docName] = docTf;

			}

			result.termString = termString;
			result.d_locations = dLocations;
			result.d_docTf = dDocTf;

			return result;
		}
    }
}
