using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Model
{
    class Indexer : iIndexer
    {

		//private SortedDictionary<string, int> mainIndex;
		private SortedList<string, int> mainIndexList;
		private string lastInPosting;
		private bool writeToEnd;//this boolean determines wether to write to end of file
		StreamReader postingReader;
		StreamWriter tempWriter;

		public Indexer()
		{
			//mainIndex = new SortedDictionary<string, int>();
			mainIndexList = new SortedList<string, int>(); //- OPTIONAL to use SortedList instead of SortedDictionary for access by index
			lastInPosting = null;
		}

        public void saveTerms(SortedDictionary<string, Term> terms)
        {
			int lineCount = 0;//on what line in posting i stand
			int writeLineNumber;//the line number i need to write
			int insertedTerms = 0;//we must keep trace on how many new terms added
			string currTerm;
			string currLine = "";
			//lastInPosting = null;
			writeToEnd = false;
			//CREATE here 1 writer to append posting, 1 reader for posting, 1 writer for new TEMP file
			postingReader = new StreamReader("postingFile.txt");
			if (!File.Exists("tempFile.txt"))
				tempWriter = File.CreateText("tempFile.txt");
			else
				tempWriter = new StreamWriter("tempFile.txt");

			foreach (KeyValuePair<string, Term> tuple in terms)
			{
				currTerm = tuple.Key;
/*
				if (writeToEnd)
				{
					tempWriter.WriteLine(currTerm + " " + tuple.Value.ToString());
					continue;
				}
*/
				//if (mainIndex.ContainsKey(currTerm))
				if (mainIndexList.ContainsKey(currTerm))
				{
					//get term's index
					//writeLineNumber = mainIndex[currTerm];
					writeLineNumber = mainIndexList[currTerm];
					//move all the lines before to the new file
					for (; lineCount < writeLineNumber; lineCount++)
					{
						tempWriter.WriteLine(postingReader.ReadLine());
					}
					//get the required line
					currLine = postingReader.ReadLine();
					//edit it
					currLine += tuple.Value.ToString();
					//write it to new file
					tempWriter.WriteLine(currLine);
					//INCREASE LINE?
					lineCount++;
					//go to next term
				}
				else
				{
					//checks if the term is after the last term in file
/*
					if (currTerm.CompareTo(lastInPosting) > 0)
					{
						//write to end of file
						tempWriter.WriteLine(currTerm + " " + tuple.Value.ToString());
						writeToEnd = true;

					}
					else
					{
*/
						//have to add this term to index without value
						/*
						mainIndex[currTerm] = -1;
						//get the position of the term in index
						writeLineNumber = 0;
						//foreach (KeyValuePair<string, int> indexItem in mainIndex)
						foreach (KeyValuePair<string, int> indexItem in mainIndexList)
						{
							if (indexItem.Value == -1)
								break;	
							writeLineNumber++;
						}
						//mainIndex[currTerm] = writeLineNumber;
						*/
						mainIndexList[currTerm] = -1;
						writeLineNumber = mainIndexList.IndexOfKey(currTerm);
						//move all the lines before to the new file
						for (; lineCount < writeLineNumber - insertedTerms; lineCount++)
						{
							tempWriter.WriteLine(postingReader.ReadLine());
						}
						//write the new term line
						tempWriter.WriteLine(currTerm + " " + tuple.Value.ToString());
						//INCREASE LINE?
						insertedTerms++;
						//go to next term
//					}
					
				}
			}
			//DONT FORGET to complete writing rest of lines from posting to temp
			while (!postingReader.EndOfStream)
			{
				tempWriter.WriteLine(postingReader.ReadLine());
			}
			//close streams
			postingReader.Close();
			tempWriter.Close();
			//switch between file names
			File.Move("postingFile.txt", "postingFile.old");
			File.Move("tempFile.txt", "postingFile.txt");
			File.Delete("postingFile.old");
			//go over mainIndex and set new line numbers
			
/*			
 			int indexingHelper = 0;
			SortedDictionary<string, int> tempIndex = new SortedDictionary<string, int>();
			foreach (KeyValuePair<string, int> indexTerm in mainIndex)
			{
				tempIndex[indexTerm.Key] = indexingHelper;
				indexingHelper++;
			}
			mainIndex = tempIndex;
*/
        }


        public void startIndexing()
        {
            throw new NotImplementedException();
        }

    }
}
