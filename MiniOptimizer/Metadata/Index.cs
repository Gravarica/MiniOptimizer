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
        public List<string> IndexedColumns { get; set; } = new List<string>();
        public IndexType IndexType { get; set; }
        public bool IsUnique { get; set; }
        public bool IsClustered { get; set; }
        
        public Index(string indexName, string columnName, Table table) {
            IsClustered = true;
            IndexedColumns.Add(columnName); 
            Name = indexName;
            table.Indexes.Add(this);
        }

        public Index(string indexName, List<string> columns, Table table) 
        {
            IsClustered = columns.Count == 1;
            IndexedColumns = columns;
            Name = indexName;
            table.Indexes.Add(this);
        }

        public bool HasColumn(string columnName)
        {
            return IndexedColumns.Contains(columnName);
        }

        public bool HasColumns(HashSet<string> columns)
        {
            bool found = true; 

            foreach (var column in IndexedColumns)
            {
                if (!columns.Contains(column)) found = false;
            }

            return found;
        }
    }
}
