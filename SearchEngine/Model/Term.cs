using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.Model
{
    class Term
    {
        public string type { get; set; }
        public string termString { get; set; }
        // <docName,positions> example positions="4,7,8,"
        public Dictionary<string, StringBuilder> d_locations { get; set; }

        public Term(string type, string termString)
        {
            d_locations = new Dictionary<string, StringBuilder>();
            this.type = type;
            this.termString = termString;
        }

        public override int GetHashCode()
        {
            return termString.GetHashCode();
        }

        internal void addPosition(string docNme, int p)
        {
            if (!d_locations.ContainsKey(docNme))
            {
                d_locations[docNme] = new StringBuilder(p + ",");

            }
            else
            {
                d_locations[docNme].Append(p + ",");
            }

        }
        public override string ToString()
        {
            StringBuilder ans = new StringBuilder();
            //ans = termString + "=|";
            foreach (var pair in d_locations)
            {
                ans.Append(pair.Key + ";");
                ans.Append(pair.Value);
                ans.Append('|');
            }
            return ans.ToString();
        }


    }
}
