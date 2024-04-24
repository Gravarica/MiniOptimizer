using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Exceptions
{
    public class ColumnNotFoundException : BaseException 
    {
        public ColumnNotFoundException(string name) : base("Error 801. Specified column " + name + " doesn't exist.") 
        {
            ErrorCode = 801;
        }
    }
}
