using MiniOptimizer.Compiler;
using MiniOptimizer.LogicPlan;
using MiniOptimizer.Metadata;
using MiniOptimizer.Optimizer;
using MiniOptimizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Test
{
    
    public static class TestMethods
    {
        const string query1 = "SELECT radnik.mbr FROM radnik, projekat, radproj, angazovanje WHERE" +
            " radnik.plt = 3000 AND radnik.mbr = radproj.mbr AND radproj.spr = projekat.spr AND radnik.mbr = angazovanje.mbr";

        const string query3 = "SELECT radproj.brc FROM radnik, radproj WHERE radnik.mbr = 3000 AND radnik.mbr = radproj.mbr";

        const string query4 = "SELECT radproj.brc FROM radnik, radproj, projekat WHERE radnik.mbr = radproj.mbr";

        const string query5 = "SELECT radproj.brc FROM radnik, radproj, projekat WHERE radnik.plt = 3000 AND radnik.mbr = radproj.mbr AND radproj.spr = projekat.spr";

        const string query2 = "SELECT A.a1, B.b1, B.d1 FROM A, B, C, D WHERE A.b1 = B.b1 AND A.a1 = C.a1 AND C.f1 = D.f1";

        public static void TestOptimizer()
        {
            Catalog catalog = TestData.TestDataFromFile(false);
            RuleBasedOptimizer rbo = new RuleBasedOptimizer(catalog);
            CostModel costModel = new CostModel(catalog);
            JoinOptimizer jo = new JoinOptimizer(costModel);
            CostBasedOptimizer cbo = new CostBasedOptimizer(catalog, costModel); 
            SQLParser parser = new SQLParser(catalog);
            while (true)
            {
                Console.WriteLine("Enter your query: ");
                string query = Console.ReadLine();
                if (query == "X") break;
                try
                {
                    if (query == "1")
                        query = query1;
                    if (query == "2")
                        query = query2;
                    if (query == "3")
                        query = query3;
                    if (query == "4")
                        query = query4;
                    if (query == "5")
                        query = query5;

                    var logicalPlan = parser.Parse(query);
                    logicalPlan.CreateInitialPlan();
                    Console.WriteLine("================= Initial Plan ================== ");
                    logicalPlan.PrintLogicalPlan();
                    Console.WriteLine();

                    Console.WriteLine("================= Creating joins ================== ");
                    Console.ReadLine();
                    rbo.CreateJoinNodes(logicalPlan);
                    logicalPlan.PrintLogicalPlan();
                    Console.WriteLine();

                    Console.WriteLine("================= Pushing down selections ================== ");
                    Console.ReadLine();
                    rbo.PushDownSelections(logicalPlan);
                    logicalPlan.PrintLogicalPlan();
                    Console.WriteLine();

                    Console.WriteLine("================= Replicating projections ================== ");
                    Console.ReadLine();
                    rbo.ReplicateProjections(logicalPlan);
                    logicalPlan.PrintLogicalPlan();
                    Console.WriteLine();

                    Console.WriteLine("================= Estimating Plan Cardinality ===================");
                    Console.ReadLine();
                    costModel.EstimatePlanCardinality(logicalPlan);                    
                    logicalPlan.PrintLogicalPlan();
                    Console.WriteLine();

                    Console.WriteLine("================= Join Ordering ========================");
                    Console.ReadLine();
                    LogicalNode optimizedTree = jo.OptimizeJoin(logicalPlan);
                    if (optimizedTree != null) logicalPlan.ChangeSubtree(logicalPlan.RootNode.Children.First(), optimizedTree);
                    logicalPlan.PrintLogicalPlan();
                    Console.WriteLine();

                    Console.WriteLine("================= Converting to Physical plan ================== ");
                    Console.ReadLine();
                    var physicalPlan = cbo.Optimize(logicalPlan);
                    physicalPlan.PrintPhysicalPlan();
                    Console.WriteLine();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        
    }
}
