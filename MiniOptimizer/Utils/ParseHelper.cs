using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniOptimizer.Compiler;
using MiniOptimizer.Metadata;

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

        public static string GetTableNameFromPredicate(string input)
        {
            Tuple<string, string> qualifiedName = ParseQualifiedName(input);

            return qualifiedName?.Item1;
        }

        public static MiniQLDataType ConvertToMiniQLDataType(int tokenType)
        {
            if (tokenType == MiniQLParser.NUMERIC_LITERAL)
            {
                return MiniQLDataType.INT;
            }
            else if (tokenType == MiniQLParser.DECIMAL_VALUE)
            {
                return MiniQLDataType.DECIMAL;
            }

            return MiniQLDataType.STRING;
        }
    }

    
}
