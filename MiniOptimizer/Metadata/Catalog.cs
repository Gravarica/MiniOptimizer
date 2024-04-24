using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Metadata
{
    public class Catalog
    {
        Dictionary<string, Table> Tables{ get ; set; }
        public Catalog() 
        {
            Tables = [];
        }

        public Catalog(Dictionary<string, Table> tables)
        {
            Tables = tables;
        }

        public bool CheckIfTableExists(string tableName)
        {
            return Tables.ContainsKey(tableName);
        }

        public bool CheckIfColumnExists(string tableName, string columnName)
        {
            return Tables[tableName].CheckIfColumnExists(columnName);
        }
    }
}
