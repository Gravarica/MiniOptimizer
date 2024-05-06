using MiniOptimizer.LogicPlan;
using MiniOptimizer.Optimizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
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

        public static List<LogicalRelationNode> GetRelationNodes(LogicalPlan plan)
        {
            Func<LogicalNode, bool> isJoinNode = node =>
                                    node is LogicalJoinNode;

            Func<LogicalNode, bool> isProductNode = node => 
                                    node is LogicalProductNode;

            List<LogicalNode> joinNodes = plan.FindAll(isJoinNode);
            List<LogicalNode> prodNodes = plan.FindAll(isProductNode);
            List<LogicalRelationNode> tableNodes = new List<LogicalRelationNode>();

            foreach (var joinNode in joinNodes)
            {
                foreach (var child in joinNode.Children)
                {
                    if (!(child is LogicalJoinNode) && !(child is LogicalProductNode))
                    {
                        LogicalRelationNode relationNode = new LogicalRelationNode(child);
                        tableNodes.Add(relationNode);
                    }
                }
            }

            foreach (var prodNode in prodNodes)
            {
                foreach (var child in prodNode.Children)
                {
                    if (!(child is LogicalJoinNode) && !(child is LogicalProductNode))
                    {
                        LogicalRelationNode relationNode = new LogicalRelationNode(child);
                        tableNodes.Add(relationNode);
                    }
                }
            }

            return tableNodes;
        }

        public static List<HashSet<T>> GetSubsets<T>(List<T> original, int subsetSize)
        {
            var allSubsets = new List<HashSet<T>>();
            RecurseSubsets(new HashSet<T>(), original, subsetSize, 0, allSubsets);
            return allSubsets;
        }

        private static void RecurseSubsets<T>(HashSet<T> current, List<T> original, int subsetSize, int startIndex, List<HashSet<T>> allSubsets)
        {
            if (current.Count == subsetSize)
            {
                allSubsets.Add(new HashSet<T>(current));
                return;
            }

            for (int i = startIndex; i < original.Count; i++)
            {
                current.Add(original[i]);
                RecurseSubsets(current, original, subsetSize, i + 1, allSubsets);
                current.Remove(original[i]);
            }
        }

        public static string CanBeJoined(LogicalNode node1, LogicalNode node2)
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

                if (node2 is LogicalRelationNode lrel)
                {
                    return CanBeJoined(node1, lrel.ProjectionNode);
                }

                if (node2 is LogicalProjectionNode lprojection)
                {
                    string res = CanBeJoined(node1.Children.First(), lprojection);
                    if (res == null)
                    {
                        return CanBeJoined(node1.Children.Last(), lprojection);
                    }
                    return res;
                }

            }

            if (node1 is LogicalProductNode prod)
            {
                string col = CanBeJoined(node1.Children.First(), node2);
                if (col != null) return col;
                return CanBeJoined(node1.Children.Last(), node2);
            }

            if (node1 is LogicalRelationNode r1)
            {
                return CanBeJoined(r1.ProjectionNode, node2);
            }

            if (node2 is LogicalRelationNode r2)
            {
                return CanBeJoined(node1, r2.ProjectionNode);
            }

            return null;
        }
    }
}
