using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SearchEngine.Model
{
    class ReadFile
    {
        static public Queue<string> ReadFiles(string path)
        {
            Queue<string> data = new Queue<string> { };
            try
            {
                string[] filesPaths = Directory.GetFiles(@path.ToString(), "*.", SearchOption.TopDirectoryOnly);

                foreach (string filePath in filesPaths)
                {
                    try
                    {
                        string file = new System.IO.StreamReader(filePath).ReadToEnd();
                        string[] docs = file.Split(new string[] { "<DOC>" }, StringSplitOptions.None);
                        foreach (string doc in docs)
                        {
                            if (doc.Length > 0)
                            {

                                data.Enqueue(doc.Split(new string[] { "</DOC>" }, StringSplitOptions.None)[0]);

                            }
                        }
                    }
                    catch (Exception e)
                    {

                        throw e;
                    }


                }

            }
            catch (Exception e)
            {

                throw e;
            }


            return data;
        }

        internal static string[] getFilesPaths(string filesPath)
        {
            try
            {
                return Directory.GetFiles(@filesPath.ToString(), "*.", SearchOption.TopDirectoryOnly);
            }
            catch (Exception e)
            {

                throw e;
            }
        }



        internal static string[] fileToDocString(string filePath)
        {
            /*
            XmlDocument file = new XmlDocument();
            XmlNodeList docs = file.GetElementsByTagName("DOC");
            foreach (XmlNode doc in docs)
            {
                RetrievalEngineProject.MainWindow.docCounter++;
                Doc newDoc = new Doc(elem);

            }
            doc.Load(filePath);
            */
            string file = new System.IO.StreamReader(filePath).ReadToEnd();
            return file.Split(new string[] { "<DOC>" }, StringSplitOptions.None);
        }

        /// <summary>
        ///  reads stop wards
        /// </summary>
        /// <param name="path">path to stop words text file</param>
        internal static Dictionary<string, bool> readStopWords(string path)
        {
            Dictionary<string, bool> StopWords = new Dictionary<string, bool>();

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
			
            return StopWords;

        }

    }
}
