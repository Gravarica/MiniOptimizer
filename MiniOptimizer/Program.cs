using Antlr4.Runtime;
using System;

namespace MiniOptimizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = new AntlrInputStream("SELECT name FROM name WHERE name = name");
            var lexer = new MiniQLLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new MiniQLParser(tokens);

            var context = parser.query();
            Console.WriteLine("Parsing complete");
        }
    }
}
