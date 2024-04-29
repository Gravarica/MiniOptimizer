using MiniOptimizer.LogicPlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Test
{
    
    public static class TestMethods
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
            foreach (var projectionNode in projectionNodes) {
                Console.WriteLine(projectionNode.Key);
                Console.WriteLine(plan.GetNodeDescription(projectionNode.Value));
            }

        }
    }
}
