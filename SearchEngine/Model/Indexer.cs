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
    class Indexer : iIndexer
    {
		private SortedList<string, int> mainIndexList1;
		private SortedList<string, int> mainIndexList2;
		private SortedList<string, int> mainIndexList3;
		private SortedList<string, int> mainIndexList4;
		private SortedList<string, int> mainIndexList5;

        StreamReader postingReader;
        StreamWriter tempWriter;
        string postingPath;
        private bool v;

        public Indexer(string postingPath)
        {
            this.postingPath = postingPath;
			mainIndexList1 = new SortedList<string, int>();
			mainIndexList2 = new SortedList<string, int>();
			mainIndexList3 = new SortedList<string, int>();
			mainIndexList4 = new SortedList<string, int>();
			mainIndexList5 = new SortedList<string, int>();
        }

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

            }
            catch (Exception e)
            {

                throw;
            }

        }

        public void saveTerms(SortedDictionary<string, Term> terms, string fileName, SortedList<string, int> mainIndexList)
        {
            int lineCount = 0;//on what line in posting i stand
            int writeLineNumber;//the line number i need to write
            int insertedTerms = 0;//we must keep trace on how many new terms added
            string currTerm;
            string currLine = "";
            fileName = postingPath + "\\" + fileName;
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

        public void saveLists()
        {
            File.Delete(postingPath + @"\list1.bin");
            File.Delete(postingPath + @"\list2.bin");
            File.Delete(postingPath + @"\list3.bin");
            File.Delete(postingPath + @"\list4.bin");
            File.Delete(postingPath + @"\list5.bin");
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(new FileStream(postingPath + @"\list1.bin", FileMode.Create, FileAccess.Write, FileShare.None), mainIndexList1);
            formatter.Serialize(new FileStream(postingPath + @"\list2.bin", FileMode.Create, FileAccess.Write, FileShare.None), mainIndexList2);
            formatter.Serialize(new FileStream(postingPath + @"\list3.bin", FileMode.Create, FileAccess.Write, FileShare.None), mainIndexList3);
            formatter.Serialize(new FileStream(postingPath + @"\list4.bin", FileMode.Create, FileAccess.Write, FileShare.None), mainIndexList4);
            formatter.Serialize(new FileStream(postingPath + @"\list5.bin", FileMode.Create, FileAccess.Write, FileShare.None), mainIndexList5);
        }
    }
}
