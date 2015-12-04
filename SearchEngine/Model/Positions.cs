using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Model
{
    class Positions
    {
        private string docName;
        List<int> l_position;
        string termType;


        public Positions(string docName, string termType)
        {
            // TODO: Complete member initialization
            this.docName = docName;
            this.termType = termType;
            l_position = new List<int>();
        }
        internal void addPosition(int p)
        {
            l_position.Add(p);
        }

    }
}
