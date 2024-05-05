using MiniOptimizer.LogicPlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Utils
{
    public static class JoinOptimizerUtils
    {
        public static List<LogicalNode> GetRelationsToOrder(LogicalPlan plan)
        {
            Func<LogicalNode, bool> isJoinNode = node =>
                                    node is LogicalJoinNode;

            List<LogicalNode> joinNodes = plan.FindAll(isJoinNode);
            List<LogicalNode> tableNodes = new List<LogicalNode>();
            foreach (var joinNode in joinNodes)
            {
                foreach (var child in joinNode.Children)
                {
                    if (!(child is LogicalJoinNode))
                    {
                        tableNodes.Add(child);
                    }
                }
            }

            return tableNodes;
        }

        public static (int, long[,], LogicalNode[,]) InitializeDP(LogicalPlan plan)
        {
            var tableNodes = GetRelationsToOrder(plan);

            int n = tableNodes.Count;
            long[,] dp = new long[n, n];
            int[,] split = new int[n, n];
            LogicalNode[,] solution = new LogicalNode[n, n];

            for (int i = 0; i < n; i++)
            {
                dp[i, i] = 0;
                solution[i, i] = tableNodes[i];
            }

            return (n, dp, solution);
        }
    }
}
