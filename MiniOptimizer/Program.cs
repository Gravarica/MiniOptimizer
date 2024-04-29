using MiniOptimizer.Metadata;
using MiniOptimizer.Test;
using MiniOptimizer.Compiler;
using System;
using System.Runtime.InteropServices;
using MiniOptimizer.Optimizer;
using MiniOptimizer.LogicPlan;

namespace MiniOptimizer
{
    class Program
    {
        static void Main(string[] args)
        {
            Catalog catalog = TestData.TestDataFromFile(false);
            RuleBasedOptimizer optimizer = new RuleBasedOptimizer(catalog);
            SQLParser parser = new SQLParser(catalog);
            parser.TurnOffSemanticAnalysis();
            while (true)
            {
                Console.WriteLine("Unesite upit: ");
                string query = Console.ReadLine();
                if (query == "X") break;
                try
                {
                    var logicalPlan = parser.Parse(query);
                    logicalPlan.CreateInitialPlan();
                    Console.WriteLine("================= Initial Plan ================== ");
                    logicalPlan.PrintLogicalPlan();
                    optimizer.CreateJoinNodes(logicalPlan);
                    Console.WriteLine("================= Creating joins ================== ");
                    logicalPlan.PrintLogicalPlan();
                    optimizer.PushDownSelections(logicalPlan);
                    Console.WriteLine("================= Pushing down selections ================== ");
                    logicalPlan.PrintLogicalPlan();
                    optimizer.ReplicateProjections(logicalPlan);
                    Console.WriteLine("================= Replicating projections ================== ");
                    logicalPlan.PrintLogicalPlan();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

        }
    }
}
