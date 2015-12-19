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
		private SortedList<string, int> mainIndexList1;
		private SortedList<string, int> mainIndexList2;
		private SortedList<string, int> mainIndexList3;
		private SortedList<string, int> mainIndexList4;
		private SortedList<string, int> mainIndexList5;

        StreamReader postingReader;
        StreamWriter tempWriter;

        public Indexer()
        {
			mainIndexList1 = new SortedList<string, int>();
			mainIndexList2 = new SortedList<string, int>();
			mainIndexList3 = new SortedList<string, int>();
			mainIndexList4 = new SortedList<string, int>();
			mainIndexList5 = new SortedList<string, int>();
        }

        public void saveTerms(SortedDictionary<string, Term> terms, string fileName, SortedList<string, int> mainIndexList)
        {
            int lineCount = 0;//on what line in posting i stand
            int writeLineNumber;//the line number i need to write
            int insertedTerms = 0;//we must keep trace on how many new terms added
            string currTerm;
            string currLine = "";

            //statistics
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
                    //update number of files df
                    mainIndexList[currTerm] += currDf;
                    //get term's index
					writeLineNumber = mainIndexList.IndexOfKey(currTerm) - insertedTerms;
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
                }
                else
                {
                    //update number of files df
                    mainIndexList[currTerm] = currDf;
                    writeLineNumber = mainIndexList.IndexOfKey(currTerm);
                    //move all the lines before to the new file
                    for (; lineCount < writeLineNumber - insertedTerms; lineCount++)
                    {
                        tempWriter.WriteLine(postingReader.ReadLine());
                    }
                    //write the new term line
                    tempWriter.WriteLine(currTerm + "@" + tuple.Value.ToString());
                    insertedTerms++;
                }
            }
            //complete writing rest of lines from posting to temp
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


        public void startIndexing(SortedDictionary<string, Term> d_abNums, SortedDictionary<string, Term> d_cf, SortedDictionary<string, Term> d_gm, SortedDictionary<string, Term> d_nr, SortedDictionary<string, Term> d_sz)
        {
			System.Console.WriteLine("started indexing at:" + DateTime.Now);
			saveTerms(d_abNums, "abNumsPosting.txt", mainIndexList1);
			saveTerms(d_cf, "cfPosting.txt", mainIndexList2);
			saveTerms(d_gm, "gmPosting.txt", mainIndexList3);
			saveTerms(d_nr, "nrPosting.txt", mainIndexList4);
			saveTerms(d_sz, "szPosting.txt", mainIndexList5);
			System.Console.WriteLine("finished indexing at:" + DateTime.Now);
        }
    }
}
