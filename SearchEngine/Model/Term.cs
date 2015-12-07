﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchEngine.Model
{
    class Term
    {
        string type;
        string termString;


        public Term(string type, string termString)
        {
            // TODO: Complete member initialization
            this.type = type;
            this.termString = termString;
        }

        public override int GetHashCode()
        {
            return termString.GetHashCode();
        }

        public override string ToString()
        {
            return termString;
        }
    }
}