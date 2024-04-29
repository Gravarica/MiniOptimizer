using MiniOptimizer.LogicPlan;
using MiniOptimizer.Metadata;
using MiniOptimizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Security.AccessControl;
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

        public void Optimize(LogicalPlan logicalPlan)
        {
            CreateJoinNodes(logicalPlan);
            PushDownSelections(logicalPlan);
            ReplicateProjections(logicalPlan);
        }

        public void CreateJoinNodes(LogicalPlan logicalPlan)
        {
            Func<LogicalNode, bool> isJoinPredicateSelection = node =>
                                        node is LogicalSelectionNode selectionNode && 
                                        selectionNode.PredicateType == PredicateType.JOIN;
            Func<LogicalNode, bool> isProductNode = node =>
                                    node is LogicalProductNode;

            List<LogicalNode> selectionJPNodes = logicalPlan.FindAll(isJoinPredicateSelection);
            
            LogicalNode productNode = logicalPlan.FindFirst(isProductNode);

            if (productNode == null) return;

            if (selectionJPNodes.Count == 0)
            {
                var root = logicalPlan.CreateLeftDeepTree();
                productNode.Parent.AddChild(root);
                productNode.Parent.RemoveChild(productNode);

                return;
            };

            List<LogicalJoinNode> joinNodes = new List<LogicalJoinNode>();
            LogicalNode topJoinNode = null;

            foreach(LogicalNode selectionNode in selectionJPNodes)
            {
                Tuple<string, string> leftQN = ParseHelper.ParseQualifiedName((selectionNode as LogicalSelectionNode).LeftOperand);
                Tuple<string, string> rightQN = ParseHelper.ParseQualifiedName((selectionNode as LogicalSelectionNode).RightOperand);

                List<LogicalNode> scanNodes = new List<LogicalNode>();
                bool foundLeftScan = false; 
                bool foundRightScan = false;

                foreach (var scanNode in productNode.Children)
                {
                    if ((scanNode as LogicalScanNode).TableName == leftQN.Item1)
                    {
                        scanNodes.Add(scanNode);
                        productNode.RemoveChild(scanNode);
                        foundLeftScan = true;
                    } 
                    else if ((scanNode as LogicalScanNode).TableName == rightQN.Item1)
                    {
                        scanNodes.Add(scanNode);
                        productNode.RemoveChild(scanNode);
                        foundRightScan = true;
                    }
                }

                string tableName = foundLeftScan ? rightQN.Item1 : leftQN.Item1;
                LogicalJoinNode joinNode  = new LogicalJoinNode(LogicalPlan.GetNextNodeId(), leftQN.Item2, rightQN.Item2, tableName, rightQN.Item1);

                if (scanNodes.Count == 2)
                {
                    logicalPlan.AppendNode(scanNodes.First(), joinNode);
                    logicalPlan.AppendNode(scanNodes.Last(), joinNode);
                } 
                else if (scanNodes.Count == 1)
                {
                    LogicalJoinNode existing = joinNodes.Find(n => n.LeftTable == tableName || n.RightTable == tableName);
                    logicalPlan.AppendNode(existing, joinNode);
                    logicalPlan.AppendNode(scanNodes.First(), joinNode);
                }

                topJoinNode = joinNode;
                joinNodes.Add(joinNode);
                logicalPlan.RemoveNode(selectionNode);
            }

            if (productNode?.Children.Count > 0 && topJoinNode != null)
            {
                foreach (var child in productNode.Children) 
                {
                    LogicalProductNode newProductNode = new LogicalProductNode(LogicalPlan.GetNextNodeId());
                    newProductNode.Children.Add(topJoinNode);
                    newProductNode.Children.Add(child);
                    topJoinNode = newProductNode;
                }
            }

            topJoinNode.Parent = productNode?.Parent;
            productNode.Parent?.Children.Remove(productNode);
            topJoinNode.Parent?.Children.Add(topJoinNode);
        }

        public void PushDownSelections(LogicalPlan logicalPlan)
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

                logicalPlan.MoveNode(selectionNode, scanNode);
            }

        }

        public void ReplicateProjections(LogicalPlan logicalPlan)
        {
            // Nema potrebe raditi replikaciju projekcija ako ima samo jedna tabela 
            if (logicalPlan.ScanNodes.Count == 1) return;

            Dictionary<string, LogicalProjectionNode> projectionNodes = 
                    logicalPlan.ReplicateProjectionByTable(logicalPlan.RootNode as LogicalProjectionNode);

            foreach (var projectionNode in projectionNodes)
            {
                Func<LogicalNode, bool> scanHasTable = node =>
                node is LogicalScanNode scanNode &&
                                            (scanNode.TableName == projectionNode.Key);

           
                LogicalNode scanNode = logicalPlan.FindFirst(scanHasTable);

                if (scanNode.Parent is LogicalSelectionNode)
                {
                    logicalPlan.InsertNode(projectionNode.Value, scanNode.Parent);
                } 
                else
                {
                    logicalPlan.InsertNode(projectionNode.Value, scanNode);
                }
            }
        }

    }
}
