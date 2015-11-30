using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SearchEngine.Model
{
    class Doc
    {
        private ManualResetEvent _doneEvent;
        private string rawData;
        private int docIndex;

        public Doc(string content)
        {
            rawData = content;
            _doneEvent = new ManualResetEvent(false);
        }

        public void ThreadPoolCallback(Object threadContext)
        {
            docIndex = (int)threadContext;
            Console.WriteLine("Parsing doc {0} started...", docIndex);
            parsDoc(rawData);
            Console.WriteLine("Parsing doc {0} ended...", docIndex);
            _doneEvent.Set();
        }

        private void parsDoc(string rawData)
        {

            string[] split = rawData.Split(new string[] { "<DOCNO>" }, StringSplitOptions.None);
            split = split[1].Split(new string[] { "</DOCNO>" }, StringSplitOptions.None);
            string nDoc = split[0];
            //Doc text
            split = split[1].Split(new string[] { "<TEXT>" }, StringSplitOptions.None);
            split = split[1].Split(new string[] { "</TEXT>" }, StringSplitOptions.None);
            string text = split[0];
        }
    }


}
