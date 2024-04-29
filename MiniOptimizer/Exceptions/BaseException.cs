using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Exceptions
{
    public class BaseException : Exception
    {
        public int ErrorCode { get; set;  }

        public BaseException()
        {
        }

        public BaseException(string message)
        : base(message)
        {
        }

        public BaseException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
