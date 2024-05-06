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
        public Dictionary<string, Table> Tables{ get ; set; }
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

        public bool HasCompoundIndex(string tableName, HashSet<string> columnNames)
        {
            Table table = Tables[tableName];
            bool found = true;


            foreach (var index in table.Indexes)
            {
                if (index.IndexedColumns.Count != columnNames.Count)
                {
                    found = false;
                    continue;
                }

                foreach (var indexedColumn in index.IndexedColumns)
                {

                    if (!columnNames.Contains(indexedColumn))
                    {
                        found = false;
                        continue;
                    }
                }
            }
            return found;
        }

        public Index GetIndex(string tableName, string column)
        {
            return Tables[tableName].GetIndex(column);
        }

        public Index GetCompoundIndex(string tableName, HashSet<string> columns)
        {
            return Tables[tableName].GetCompoundIndex(columns);
        }
    }
}
