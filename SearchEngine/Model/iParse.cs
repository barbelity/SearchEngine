using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine.Model
{
    interface iParse
    {
        Dictionary<string, Positions> parseDoc(string doc);
        bool startParseing(string path);

    }
}
