using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Metadata
{

    public enum ColumnType { INT, STRING };

    public class Column
    {
        public string Name { get; set; }
        public ColumnType DataType { get; set; }
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public ForeignKeyConstraint? ForeignKeyConstraint { get; set; }
        public List<Index> Indexes { get; set; } = new List<Index>();

        public Column(string name, ColumnType dataType, bool isNullable = false, bool isPrimaryKey = false)
        {
            Name = name;
            DataType = dataType;
            IsNullable = isNullable;
            IsPrimaryKey = isPrimaryKey;
        }

        public Column(string name, bool primary) { Name = name; IsPrimaryKey = primary; DataType = ColumnType.INT; }
    }
}
