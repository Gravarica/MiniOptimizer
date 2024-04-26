using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Utils
{
    public static class ParseHelper
    {
        public static Tuple<string, string>? ParseQualifiedName (string input)
        {
            string[] strings = input.Split(".");
            if (strings.Length != 2)
                return null;
            
            return new Tuple<string, string>(strings[0], strings[1]);
        }
    }
}
