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
        public List<string> IndexedColumns { get; set; } = new List<string>();
        public IndexType IndexType { get; set; }
        public bool IsUnique { get; set; }
        public bool IsClustered { get; set; }
        
        public Index(string indexName, string columnName, Table table) {
            IsClustered = true;
            IndexedColumns.Add(columnName); 
            Table = table;
            Name = indexName;
            Table.Columns[columnName].Indexes.Add(this);
        }
    }
}
