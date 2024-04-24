using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Metadata
{
    public class Column
    {
        public string Name { get; set; }
        public Type DataType { get; set; }
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public ForeignKeyConstraint? ForeignKeyConstraint { get; set; }
        public List<Index> Indexes { get; set; } = new List<Index>();

        public Column(string name, Type dataType, bool isNullable = false, bool isPrimaryKey = false)
        {
            Name = name;
            DataType = dataType;
            IsNullable = isNullable;
            IsPrimaryKey = isPrimaryKey;
        }

        public Column(string name, bool primary) { Name = name; IsPrimaryKey = primary; }
    }
}
