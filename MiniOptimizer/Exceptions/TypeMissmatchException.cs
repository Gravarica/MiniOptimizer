using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Exceptions
{
    public class TypeMissmatchException : BaseException
    {
        public TypeMissmatchException(string column) : base("Error 902. Column " + column + " and value in predicate are of different types.", 902) { }
    }
}
