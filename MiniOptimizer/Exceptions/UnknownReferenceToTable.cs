using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Exceptions
{
    public class UnknownReferenceToTable : BaseException
    {
        public UnknownReferenceToTable(string tableName) : base("Error 803. Unknown reference to table: " + tableName + ".", 803)
        { 
        
        }
    }
}
