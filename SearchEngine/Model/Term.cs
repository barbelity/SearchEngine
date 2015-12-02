using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.Model
{
    class Term
    {
        string type;
        string value;

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}
