using Antlr4.Runtime;
using System;

namespace MiniOptimizer
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser parser = new Parser();
            var logicalPlan = parser.Parse("SELECT mbr, ime, prz, god, dat FROM radnik, radproj WHERE mbr = 5");
            logicalPlan.PrintLogicalPlan();
        }
    }
}
