using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SearchEngine.Model
{
    class Parse : iParse
    {

        private Dictionary<string, bool> StopWords;
        Mutex mStopwords = new Mutex();

        public Parse(string path)
        {
            readStopWords(path + @"\StopWords.txt");
        }


        public void ParserDocs(List<string> docs)
        {

            foreach (var docString in docs)
            {
                Doc doc = new Doc(docString);

            }
            //WaitHandle.WaitAll(doneEvents);
            Console.WriteLine("All calculations are complete.");
        }


        internal static void startParsing(string filesPath)
        {
            string[] paths = ReadFile.getFilesPaths(filesPath);
            foreach (string filePath in paths)
            {
                string[] docs = ReadFile.fileToDocString(filePath);
                foreach (var doc in docs)
                {
                    //   ThreadPool.QueueUserWorkItem(parseDoc, doc);
                }
            }
        }

        public void parseDoc(Object rawDoc)
        {
            //name

            string[] splitDoc = ((string)rawDoc).Split(new string[] { "<DOCNO>" }, StringSplitOptions.None);
            splitDoc = splitDoc[1].Split(new string[] { "</DOCNO>" }, StringSplitOptions.None);
            string name = splitDoc[0];
            //text
            splitDoc = splitDoc[1].Split(new string[] { "<TEXT>" }, StringSplitOptions.None);
            splitDoc = splitDoc[1].Split(new string[] { "</TEXT>" }, StringSplitOptions.None);
            string text = splitDoc[0];
            string[] terms = text.Split(' ', '\n', '(', ')', '[', ']', '{', '}', '*', ':', ';', '?', '!', '+', '&', '^', '$');
            string term = "";
            int index = 1;
            for (int i = 0; i < terms.Length; i++)
            {

                mStopwords.WaitOne();
                if (StopWords.ContainsKey(term.ToLower()))
                {
                    mStopwords.ReleaseMutex();
                    index++;
                    continue;
                }
                mStopwords.ReleaseMutex();

                // if (isNumber(term))
                {

                }
            }

        }




        /// <summary>
        ///  reads stop wards
        /// </summary>
        /// <param name="path">path to stop words text file</param>
        private void readStopWords(string path)
        {
            StopWords = new Dictionary<string, bool>();

            try
            {
                string text = System.IO.File.ReadAllText(path);
                string[] words = text.Split('\n');
                foreach (string word in words)
                {
                    StopWords[word.Substring(0, word.Length - 1)] = true;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public Dictionary<string, Positions> parseDoc(string doc)
        {
            throw new NotImplementedException();
        }

        public bool startParseing(string path)
        {
            throw new NotImplementedException();
        }
    }
}
