using MiniOptimizer.LogicPlan;
using MiniOptimizer.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Optimizer
{
    public class JoinOptimizer
    {
        private CostModel _costModel;

        public JoinOptimizer(CostModel costModel)
        {
            _costModel = costModel;
        }

        public (long, LogicalNode) ComputeOptimalJoinOrder(LogicalPlan logicalPlan)
        {
            Func<LogicalNode, bool> isJoinNode = node =>
                                    node is LogicalJoinNode;

            List<LogicalNode> joinNodes = logicalPlan.FindAll(isJoinNode);
            List<LogicalNode> tableNodes = new List<LogicalNode>();
            foreach(var joinNode in joinNodes)
            {
                foreach (var child in joinNode.Children)
                {
                    if (!(child is LogicalJoinNode))
                    {
                        tableNodes.Add(child);
                    }
                }
            }

            int n = tableNodes.Count;
            long[,] dp = new long[n, n];
            int[,] split = new int[n, n];
            LogicalNode[,] solution = new LogicalNode[n, n];

            for (int i = 0; i < n; i++)
            {
                dp[i, i] = 0;
                solution[i, i] = tableNodes[i];

            }

            for (int len = 2; len <= n; len++)
            {
                for (int i = 0; i <= n - len; i++)
                {
                    int j = i + len - 1;
                    dp[i, j] = long.MaxValue;

                    for (int k = i; k < j; k++)
                    {
                        var result = CalculateJoinCardinality(solution[i, k], solution[k + 1, j]);
                        long cost = dp[i, k] + dp[k + 1, j] + result.Item2;
                        if (cost < dp[i, j])
                        {
                            dp[i, j] = cost;
                            split[i, j] = k;
                            // Combine nodes into a new join node
                            LogicalNode newJoin;
                            if (result.Item1 == null)
                            {
                                newJoin = new LogicalProductNode(LogicalPlan.GetNextNodeId(), solution[i, k], solution[k + 1, j]);
                            } else
                            {
                                newJoin = new LogicalJoinNode(LogicalPlan.GetNextNodeId(), solution[i, k], solution[k + 1, j], result.Item1);
                            }
                            _costModel.EstimateCardinality(newJoin);
                            solution[i, j] = newJoin;
                        }
                    }
                }
            }

            PrintCostTable(dp, solution, n);
            // Return the cost and the optimal join order
            return (dp[0, n - 1], solution[0, n - 1]);
        }

        private (string, long) CalculateJoinCardinality(LogicalNode node1, LogicalNode node2)
        {
            var column = CanBeJoined(node1, node2);
            if (column == null) return (null, node1.Cardinality * node2.Cardinality);

            return (column, _costModel.EstimatePotentialJoinCardinality(node1, node2, column));
        }

        private string CanBeJoined(LogicalNode node1, LogicalNode node2)
        {
            if (node1 is LogicalProjectionNode lpn1)
            {
                if (node2 is LogicalProjectionNode lpn2)
                {

                    foreach (var attribute in lpn1.Attributes)
                    {
                        foreach (var attribute2 in lpn2.Attributes)
                        {
                            if (attribute == attribute2) return attribute;
                        }
                    }

                    return null;
                }

                if (node2 is LogicalJoinNode || node2 is LogicalProductNode)
                {
                    string res = CanBeJoined(node1, node2.Children.First());
                    if (res == null)
                    {
                        return CanBeJoined(node1, node2.Children.Last());
                    }
                    return res;
                }
            }

            if (node1 is LogicalJoinNode ljn1)
            {
                if (node2 is LogicalJoinNode ljn2)
                {
                    return ljn1.CanBeJoined(ljn2);
                }

                if (node2 is LogicalProductNode lproduct)
                {
                    string res = CanBeJoined(node1, node2.Children.First());
                    if (res == null)
                    {
                        return CanBeJoined(node1, node2.Children.Last());
                    }
                    return res;
                }
            }

            return null;
        }

        private void PrintCostTable(long[,] dp, LogicalNode[,] solution, int n)
        {
            Console.WriteLine("Cost Table with Join Orders:");
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i <= j) // Only printing the relevant part of the matrix
                    {
                        string tableName = solution[i, j]?.GetTableName(2) ?? "N/A";
                        Console.Write($"{dp[i, j]} ({tableName})\t");
                    }
                    else
                    {
                        Console.Write("\t\t"); // Aligning for the unused part of the matrix
                    }
                }
                Console.WriteLine();
            }
        }

    }
}
