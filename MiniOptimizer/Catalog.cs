using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer
{
    public class Catalog
    {
        Dictionary<String, TableStats> Stats { get ; set; }
        public Catalog() { }
    }
}
