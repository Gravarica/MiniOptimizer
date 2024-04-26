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

        public ColumnType GetColumnType(string tableName, string columnName)
        {
            return Tables[tableName].GetColumnType(columnName);
        }

        public List<string> GetTablesByColumn(string column)
        {
            return (from table in Tables
                    where table.Value.CheckIfColumnExists(column)
                    select table.Key).ToList();
        }
    }
}
