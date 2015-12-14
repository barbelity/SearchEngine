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
        public Dictionary<string, string> d_locations { get; set; }

        public Term(string type, string termString)
        {
            d_locations = new Dictionary<string, string>();
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
                d_locations[docNme] = p + ",";
            }
            else
            {
                d_locations[docNme] += p + ",";
            }

        }
        public override string ToString()
        {
            string ans = "";
            //ans = termString + "=|";
            foreach (var pair in d_locations)
            {
                ans += pair.Key + ";" + pair.Value.Count(x => x == ',') + ":" + pair.Value + "|";
            }
            return ans;
        }

        public int returnTF(string docName)
        {
            return d_locations[docName].Count(x => x == ',');
        }
    }
}
