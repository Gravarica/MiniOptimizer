using MiniOptimizer.LogicPlan;
using MiniOptimizer.Metadata;
using MiniOptimizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Optimizer
{
    public class RuleBasedOptimizer
    {
        //public LogicalPlan LogicalPlan { get; set; }

        private Catalog _catalog;

        public RuleBasedOptimizer(Catalog catalog)
        {
            //LogicalPlan = logicalPlan;
            _catalog = catalog;
        }

        public LogicalPlan CreateJoinNodes(LogicalPlan logicalPlan)
        {
            Func<LogicalNode, bool> isJoinPredicateSelection = node =>
                                        node is LogicalSelectionNode selectionNode && 
                                        selectionNode.PredicateType == PredicateType.JOIN;
            Func<LogicalNode, bool> isProductNode = node =>
                                    node is LogicalProductNode;

            List<LogicalNode> selectionJPNodes = logicalPlan.FindAll(isJoinPredicateSelection);
            
            LogicalNode productNode = logicalPlan.FindFirst(isProductNode);

            List<LogicalJoinNode> joinNodes = new List<LogicalJoinNode>();
            LogicalNode topJoinNode = null;

            foreach(LogicalNode selectionNode in selectionJPNodes)
            {
                Tuple<string, string> leftQN = ParseHelper.ParseQualifiedName((selectionNode as LogicalSelectionNode).LeftOperand);
                Tuple<string, string> rightQN = ParseHelper.ParseQualifiedName((selectionNode as LogicalSelectionNode).RightOperand);

                
                // Ne koristim
                Func<LogicalNode, bool> scanHasTable = node => 
                                            node is LogicalScanNode scanNode && 
                                            (scanNode.TableName == leftQN.Item1 || scanNode.TableName == rightQN.Item1);


                List<LogicalNode> scanNodes = new List<LogicalNode>();
                bool foundLeftScan = false; 
                bool foundRightScan = false;

                foreach (var scanNode in productNode.Children)
                {
                    if ((scanNode as LogicalScanNode).TableName == leftQN.Item1)
                    {
                        scanNodes.Add(scanNode);
                        productNode.Children.Remove(scanNode);
                        scanNode.Parent = null;
                        foundLeftScan = true;
                    } else if ((scanNode as LogicalScanNode).TableName == rightQN.Item1)
                    {
                        scanNodes.Add(scanNode);
                        productNode.Children.Remove(scanNode);
                        scanNode.Parent = null;
                        foundRightScan = true;
                    }
                }


                if (scanNodes.Count == 2)
                {
                    LogicalJoinNode joinNode = new LogicalJoinNode(LogicalPlan.GetNextNodeId(), leftQN.Item2, rightQN.Item2, leftQN.Item1, rightQN.Item1);
                    var leftNode = scanNodes.First();
                    leftNode.Parent = joinNode;
                    var rightNode = scanNodes.Last();
                    rightNode.Parent = joinNode;    
                    joinNode.Children.Add(leftNode);
                    joinNode.Children.Add(rightNode);
                    topJoinNode = joinNode;
                    joinNodes.Add(joinNode);
                } else if (scanNodes.Count == 1)
                {
                    string tableName = foundLeftScan ? rightQN.Item1 : leftQN.Item1;
                    LogicalJoinNode joinNode = new LogicalJoinNode(LogicalPlan.GetNextNodeId(), leftQN.Item2, rightQN.Item2, tableName, rightQN.Item1);
                    LogicalJoinNode existing = joinNodes.Find(n => n.LeftTable == tableName || n.RightTable == tableName);
                    joinNode.Children.Add(existing);
                    var node = scanNodes.First();
                    node.Parent = joinNode;
                    joinNode.Children.Add(node);
                    existing.Parent = joinNode;
                    topJoinNode = joinNode;
                }

                selectionNode.Parent.Children.Remove(selectionNode);
                selectionNode.Parent.Children.Add(selectionNode.Children.First());
                selectionNode.Children.First().Parent = selectionNode.Parent;
            }

            if (productNode.Children.Count > 0 && topJoinNode != null)
            {
                foreach (var child in productNode.Children) 
                {
                    LogicalProductNode newProductNode = new LogicalProductNode(LogicalPlan.GetNextNodeId());
                    newProductNode.Children.Add(topJoinNode);
                    newProductNode.Children.Add(child);
                    topJoinNode = newProductNode;
                }
            }

            topJoinNode.Parent = productNode.Parent;
            productNode.Parent.Children.Remove(productNode);
            topJoinNode.Parent.Children.Add(topJoinNode);

            return logicalPlan;
        }

        public LogicalPlan PushDownSelections(LogicalPlan logicalPlan)
        {
            Func<LogicalNode, bool> isFilterSelection = node =>
                                        node is LogicalSelectionNode selectionNode &&
                                        selectionNode.PredicateType == PredicateType.FILTER;

            List<LogicalNode> selectionFilterNodes = logicalPlan.FindAll(isFilterSelection);

            foreach (var selectionNode in selectionFilterNodes) 
            {
                Tuple<string, string> qualifiedName = ParseHelper.ParseQualifiedName((selectionNode as LogicalSelectionNode).LeftOperand);

                Func<LogicalNode, bool> scanHasTable = node =>
                                            node is LogicalScanNode scanNode &&
                                            (scanNode.TableName == qualifiedName.Item1);

                LogicalNode scanNode = logicalPlan.FindFirst(scanHasTable);

                selectionNode.Parent.Children.Remove(selectionNode);
                selectionNode.Parent.Children.Add(selectionNode.Children.First());
                selectionNode.Children.First().Parent = selectionNode.Parent;
                selectionNode.Children.Clear();
                scanNode.Parent.Children.Remove(scanNode);
                scanNode.Parent.Children.Add(selectionNode);
                selectionNode.Parent = scanNode.Parent;
                selectionNode.Children.Add(scanNode);
                scanNode.Parent = selectionNode;
            }

            return logicalPlan;
        }

    }
}
