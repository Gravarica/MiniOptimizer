using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Metadata
{
    public enum IndexType
    {
        BTree,
        Hash
    }

    public class Index
    {
        public string Name { get; set; }
        public Table Table { get; set; }
        public List<Column> IndexedColumns { get; set; } = new List<Column>();
        public IndexType IndexType { get; set; }
        public bool IsUnique { get; set; }
        public bool IsClustered { get; set; }
    }
}
