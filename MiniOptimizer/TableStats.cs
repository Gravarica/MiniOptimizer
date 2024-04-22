using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer
{
    public class TableStats
    {

        public int IOCostPerPage { get; set; }

        public int Cardinality { get; set; }

        public TableStats() { }
    }
}
