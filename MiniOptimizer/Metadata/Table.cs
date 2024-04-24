
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Metadata
{
    public class Table
    {
        public string Name { get; set; }
        public Dictionary<string, Column> Columns { get; set; } = new Dictionary<string, Column>();
        public List<Column> PrimaryKeyColumns { get; set; } = new List<Column>();
        public List<ForeignKeyConstraint> ForeignKeyConstraints { get; set; } = new List<ForeignKeyConstraint>();
        public List<Index> Indexes { get; set; } = new List<Index>();
        public TableStats Statistics { get; set; }

        public Table(string name)
        {
            Name = name;
            Columns = [];
            PrimaryKeyColumns = new List<Column>();
            ForeignKeyConstraints = new List<ForeignKeyConstraint>();
            Indexes = new List<Index>();
            Statistics = new TableStats();
        }

        public void AddColumn(Column column)
        {
            Columns[column.Name] = column;
            if (column.IsPrimaryKey)
                PrimaryKeyColumns.Add(column);
        }

        public bool CheckIfColumnExists(string column)
        {
            return Columns.ContainsKey(column);
        }

        public void PrintTableStats()
        {
            Console.WriteLine("Table: " + Name);
            Console.WriteLine("Row count: " + Statistics.RowCount);
            for (int i = 0; i < Columns.Count; i++)
            {
                string columnName = Columns.Keys.ElementAt(i);
                Statistics.ColumnStats[columnName].PrintHistogram();
            }
        }
    }
}
