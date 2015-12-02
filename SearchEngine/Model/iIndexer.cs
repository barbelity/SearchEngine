﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Model
{
    interface iIndexer
    {
        void saveTerms(Dictionary<string, Positions> terms);
        void startIndexing();
    }
}
