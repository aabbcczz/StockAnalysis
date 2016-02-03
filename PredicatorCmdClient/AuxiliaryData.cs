using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredicatorCmdClient
{
    public sealed class AuxiliaryData
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public double OpenPrice { get; set; }
        public double HighestPrice { get; set; }
        public double LowestPrice { get; set; }
        public double ClosePrice { get; set; }
    }
}
