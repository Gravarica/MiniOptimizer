using MiniOptimizer.Compiler;
using MiniOptimizer.LogicPlan;
using MiniOptimizer.Metadata;
using MiniOptimizer.Optimizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Test
{
    
    public static class TestMethods
    {
        public static void TestRuleBasedOptimizer()
        {
            Catalog catalog = TestData.TestDataFromFile(true);
            RuleBasedOptimizer rbo = new RuleBasedOptimizer(catalog);
            CostModel costModel = new CostModel(catalog);
            CostBasedOptimizer cbo = new CostBasedOptimizer(catalog, costModel); 
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
                    rbo.CreateJoinNodes(logicalPlan);
                    Console.WriteLine("================= Creating joins ================== ");
                    logicalPlan.PrintLogicalPlan();
                    rbo.PushDownSelections(logicalPlan);
                    Console.WriteLine("================= Pushing down selections ================== ");
                    logicalPlan.PrintLogicalPlan();
                    rbo.ReplicateProjections(logicalPlan);
                    Console.WriteLine("================= Replicating projections ================== ");
                    logicalPlan.PrintLogicalPlan();
                    Console.WriteLine("================= Selecting access methods ================== ");
                    var physicalPlan = cbo.SelectAccessMethods(logicalPlan);
                    physicalPlan.Print();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static void TestProjectionSplitting()
        {
            LogicalPlan plan = new LogicalPlan();
            LogicalProjectionNode node = new LogicalProjectionNode(LogicalPlan.GetNextNodeId());
            node.AddAttribute("radnik.mbr");
            node.AddAttribute("radnik.ime");
            node.AddAttribute("radnik.prz");
            //node.AddAttribute("projekat.spr");
            plan.SetRootNode(node);
            LogicalSelectionNode selectionNode = new LogicalSelectionNode(LogicalPlan.GetNextNodeId(), 
                                                                          PredicateType.JOIN,
                                                                          new Op(Predicate.EQ),
                                                                          "radnik.mbr",
                                                                          "projekat.ruk");
            plan.AppendNode(selectionNode, node);
            plan.SelectionNodes.Add(selectionNode);
            var projectionNodes = plan.ReplicateProjectionByTable(node);
            foreach (var projectionNode in projectionNodes) {
                Console.WriteLine(projectionNode.Key);
                Console.WriteLine(plan.GetNodeDescription(projectionNode.Value));
            }
        }

        public static void TestCardinalityEstimation()
        {
            Catalog catalog = TestData.TestDataFromFile(true);
            LogicalPlan plan = new LogicalPlan();
            LogicalProjectionNode node = new LogicalProjectionNode(LogicalPlan.GetNextNodeId());
            node.AddAttribute("radnik.mbr");
            node.AddAttribute("radnik.ime");
            node.AddAttribute("radnik.prz");
            LogicalScanNode scanNode = new LogicalScanNode(LogicalPlan.GetNextNodeId(), "radnik", 1);
            LogicalScanNode scanNode2 = new LogicalScanNode(LogicalPlan.GetNextNodeId(), "radproj", 1);
            LogicalScanNode scanNode3 = new LogicalScanNode(LogicalPlan.GetNextNodeId(), "projekat", 1);
            LogicalJoinNode joinNode = new LogicalJoinNode(LogicalPlan.GetNextNodeId(),
                                                           "mbr", "mbr", "radnik", "radproj");
            LogicalJoinNode joinNode2 = new LogicalJoinNode(LogicalPlan.GetNextNodeId(),
                                                           "spr", "spr", "projekat", "radproj");
            LogicalSelectionNode selNode = new LogicalSelectionNode(LogicalPlan.GetNextNodeId(), PredicateType.FILTER, new Op(Predicate.EQ),
                                                                    "radnik.mbr", "10");
            plan.SetRootNode(node);
            plan.AppendNode(joinNode, node);
            plan.AppendNode(scanNode2, joinNode2);
            plan.AppendNode(scanNode3, joinNode2);
            plan.AppendNode(joinNode2, joinNode);
            plan.AppendNode(selNode, joinNode);
            plan.AppendNode(scanNode, selNode);
            
            CostModel model = new CostModel(catalog);
            
            var scanEstimation = model.EstimateCardinality(scanNode);
            var scanEstimation2 = model.EstimateCardinality(scanNode2);
            var scanEstimation3 = model.EstimateCardinality(scanNode3);
            var selEstimation = model.EstimateCardinality(selNode);
            var estimation2 = model.EstimateCardinality(joinNode2);
            var estimation = model.EstimateCardinality(joinNode);
            var projEstimation = model.EstimateCardinality(node);

            Console.WriteLine(scanEstimation);
            Console.WriteLine(selEstimation);
            Console.WriteLine(scanEstimation2);
            Console.WriteLine(scanEstimation3);
            Console.WriteLine(estimation2);
            Console.WriteLine(estimation);
            Console.WriteLine(projEstimation);
        }
    }
}
