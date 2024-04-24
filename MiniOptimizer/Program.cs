using Antlr4.Runtime;
using System;

namespace MiniOptimizer
{
    class Program
    {
        static void Main(string[] args)
        {
            MiniQLParser parser = new MiniQLParser();
            var logicalPlan = parser.Parse("SELECT mbr, ime FROM radnik, radproj, projekat WHERE mbr = 10");
            logicalPlan.PrintLogicalPlan();
        }
    }
}
