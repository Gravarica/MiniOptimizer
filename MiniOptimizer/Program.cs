using Antlr4.Runtime;
using MiniOptimizer.Metadata;
using MiniOptimizer.Test;
using System;
using System.Runtime.InteropServices;

namespace MiniOptimizer
{
    class Program
    {
        static void Main(string[] args)
        {
            Catalog catalog = TestData.TestDataFromFile(false);

            Parser parser = new Parser(catalog);
            while (true)
            {
                Console.WriteLine("Unesite upit: ");
                string query = Console.ReadLine();
                if (query == "X") break;
                try
                {
                    var logicalPlan = parser.Parse(query);
                    logicalPlan.PrintLogicalPlan();
                } catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            
        }
    }
}
