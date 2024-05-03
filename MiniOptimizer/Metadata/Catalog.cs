using MiniOptimizer.Exceptions;
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

        public MiniQLDataType GetMiniQLDataType(string tableName, string columnName)
        {
            return Tables[tableName].GetMiniQLDataType(columnName);
        }

        public List<string> GetTablesByColumn(string column)
        {
            return (from table in Tables
                    where table.Value.CheckIfColumnExists(column)
                    select table.Key).ToList();
        }

        public TableStats GetTableStats(string tableName)
        {
            if (Tables.ContainsKey(tableName))
            {
                return Tables[tableName].Statistics;
            }

            throw new BaseException("Table statistics for table " + " do not exist.");
        }

        public bool HasIndexOnColumn(string tableName, string columnName) 
        {
            Table table = Tables[tableName];
            return table.Indexes.Any(index => index.IndexedColumns.Contains(columnName));
        }
    }
}
