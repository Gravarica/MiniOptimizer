using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Metadata
{
    public class TableStats
    {

        public long RowCount { get; set; }
        public Dictionary<string, ColumnStats> ColumnStats { get; set; } = new Dictionary<string, ColumnStats>();
        public Dictionary<Index, IndexStats> IndexStats { get; set; } = new Dictionary<Index, IndexStats>();

        public TableStats() 
        {
            ColumnStats = [];
        }
    }
}
