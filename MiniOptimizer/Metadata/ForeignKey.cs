using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Metadata
{
    public class ForeignKey
    {
        public Table ReferencedTable { get; set; }
        public Column ReferencedColumn { get; set; }
    }
}
