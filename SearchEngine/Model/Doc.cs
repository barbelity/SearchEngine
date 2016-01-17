using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SearchEngine.Model
{
    [Serializable()]
    class Doc
    {

        public Dictionary<string, int> d_TermsCount;
        public string maxtfString { get; set; }
        public int maxtfCount { get; set; }
        public string docName { get; set; }
        public string date { get; set; }
		public int termsCount { get; set; }
        public string Header { get; internal set; }

        public Doc(string docName, string date)
        {
            this.docName = docName;
            this.date = date;
            maxtfString = "";
            maxtfCount = 0;
			termsCount = 0;
        }

        public Doc(string docName)
        {
            this.docName = docName;
            this.date = "NA";
            maxtfString = "";
            maxtfCount = 0;
			termsCount = 0;
        }

    }


}
