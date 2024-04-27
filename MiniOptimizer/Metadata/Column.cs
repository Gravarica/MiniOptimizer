using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Metadata
{

    public enum MiniQLDataType { INT, STRING, DECIMAL };

    public class Column
    {
        public string Name { get; set; }
        public MiniQLDataType MiniQLDataType { get; set; }
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public ForeignKeyConstraint? ForeignKeyConstraint { get; set; }
        public List<Index> Indexes { get; set; } = new List<Index>();

        public Column(string name, MiniQLDataType MiniQLDataType, bool isNullable = false, bool isPrimaryKey = false)
        {
            Name = name;
            MiniQLDataType = MiniQLDataType;
            IsNullable = isNullable;
            IsPrimaryKey = isPrimaryKey;
        }

        public Column(string name, bool primary) { Name = name; IsPrimaryKey = primary; MiniQLDataType = MiniQLDataType.INT; }
    }
}
