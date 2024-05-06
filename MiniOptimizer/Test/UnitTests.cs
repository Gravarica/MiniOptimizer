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
    public static class UnitTests
    {
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
            foreach (var projectionNode in projectionNodes)
            {
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

        public static void TestSubsets(List<LogicalRelationNode> leaves)
        {
            var subsetSize = 3;
            List<HashSet<LogicalRelationNode>> subsets = JoinOptimizerUtils.GetSubsets(leaves, subsetSize);

            foreach (var subset in subsets)
            {
                foreach (var node in subset)
                {
                    Console.Write(node.GetTableName(0) + "   ");
                }
                Console.WriteLine("-------------------------");
            }
        }
    }
}
