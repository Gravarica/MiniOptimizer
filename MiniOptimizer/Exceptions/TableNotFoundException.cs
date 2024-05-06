using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Exceptions
{
    public class TableNotFoundException : BaseException
    {
        public TableNotFoundException(string tableName) : base("Error 800. Table " + tableName + " is not defined.")
        {
            ErrorCode = 800;
        }

    }
}
