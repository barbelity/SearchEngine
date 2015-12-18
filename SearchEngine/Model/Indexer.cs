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
        //		private string lastInPosting;//keep the last term in file for quick writing
        //		private bool writeToEnd;//this boolean determines wether to write to end of file
        StreamReader postingReader;
        StreamWriter tempWriter;

        public Indexer()
        {
            //mainIndex = new SortedDictionary<string, int>();
            mainIndexList = new SortedList<string, int>(); //- OPTIONAL to use SortedList instead of SortedDictionary for access by index, int = df
            //			lastInPosting = null;
        }

        public void saveTerms(SortedDictionary<string, Term> terms, Dictionary<string, Doc> docs)
        {
            int lineCount = 0;//on what line in posting i stand
            int writeLineNumber;//the line number i need to write
            int insertedTerms = 0;//we must keep trace on how many new terms added
            string currTerm;
            string currLine = "";

            //statistics
            string currDoc = "";
            int currDf;
            //lastInPosting = null;
            //			writeToEnd = false;
            postingReader = new StreamReader("postingFile.txt");
            if (!File.Exists("tempFile.txt"))
                tempWriter = File.CreateText("tempFile.txt");
            else
                tempWriter = new StreamWriter("tempFile.txt");

            foreach (KeyValuePair<string, Term> tuple in terms)
            {
                currTerm = tuple.Key;
                //currDoc = tuple.Value.d_locations
                //docs[]
                currDf = tuple.Value.d_locations.Count;


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
                    //update number of files df
                    mainIndexList[currTerm] += currDf;
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
                    //update number of files df
                    mainIndexList[currTerm] = currDf;
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
        }


        public void startIndexing()
        {
            throw new NotImplementedException();
        }


        public void saveTerms(SortedDictionary<string, Term> d_DateTerms, SortedDictionary<string, Term> d_WordTerms, SortedDictionary<string, Term> d_PercentallTerms, SortedDictionary<string, Term> d_PriceTerms, SortedDictionary<string, Term> d_NumberTerms, Dictionary<string, Doc> docs)
        {
            throw new NotImplementedException();
        }
    }
}
