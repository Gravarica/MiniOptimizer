using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.LogicPlan
{
    public class LogicalPlan
    {
        public LogicalNode RootNode { get; private set; }
        private static int _nodeId = 0;

        public List<LogicalProjectionNode> ProjectionNodes { get; set; } = new List<LogicalProjectionNode>();
        public List<LogicalSelectionNode> SelectionNodes { get; set; } = new List<LogicalSelectionNode>();
        public List<LogicalScanNode> ScanNodes { get; set; } = new List<LogicalScanNode>();
        public List<LogicalProductNode> ProductNodes {get; set; } = new List<LogicalProductNode>();

        private Queue<LogicalScanNode> _nodeQueue;

        public LogicalPlan()
        {
        }

        public LogicalNode CreateProjectionNode(List<string> projectedAttributes)
        {
            LogicalProjectionNode projectionNode = new LogicalProjectionNode(GetNextNodeId());
            foreach (var attribute in projectedAttributes)
            {
                projectionNode.AddAttribute(attribute);
            }
            return projectionNode;
        }

        public LogicalNode CreateSelectionNode(Op op, PredicateType type, string leftOperand, string rightOperand)
        {
            return new LogicalSelectionNode(GetNextNodeId(), type, op, leftOperand, rightOperand);
        }

        public LogicalNode CreateJoinNode(Op joinCondition, string leftJoinAttribute, string rightJoinAttribute)
        {
            return new LogicalJoinNode(GetNextNodeId(), joinCondition, leftJoinAttribute, rightJoinAttribute);
        }

        public LogicalNode CreateProductNode()
        {
            return new LogicalProductNode(GetNextNodeId());
        }

        public LogicalNode CreateScanNode(string tableName, int tableId)
        {
            return new LogicalScanNode(GetNextNodeId(), tableName, tableId);
        }

        public void SetRootNode(LogicalNode rootNode)
        {
            RootNode = rootNode;
        }

        public static int GetNextNodeId()
        {
            return _nodeId++;
        }

        public void CreateInitialPlan()
        {
            SetRootNode(ProjectionNodes.First());

            if (SelectionNodes.Count > 0)
            {
                RootNode.Children.Add(SelectionNodes.First());
                SelectionNodes.First().Parent = RootNode;
            }
                

            for (int i = 1; i < SelectionNodes.Count; i++)
            {
                SelectionNodes[i - 1].Children.Add(SelectionNodes[i]);
                SelectionNodes[i].Parent = SelectionNodes[i - 1];
            }

            LogicalNode ldtParent = SelectionNodes.Count > 0 ? SelectionNodes.Last() : RootNode;

            if (ScanNodes.Count == 1)
            {
                ldtParent.Children.Add(ScanNodes[0]);
                return;
            }
            
            _nodeQueue = new Queue<LogicalScanNode>(ScanNodes);
            LogicalProductNode root = CreateLeftDeepTree();

            ldtParent.Children.Add(root);
            root.Parent = ldtParent;

        }

        private LogicalProductNode CreateLeftDeepTree()
        {

            LogicalProductNode initial = new LogicalProductNode(GetNextNodeId());

            initial.Children.Add(_nodeQueue.Dequeue());
            initial.Children.Add(_nodeQueue.Dequeue());

            LogicalProductNode current = initial;

            while (_nodeQueue.Count > 0)
            {
                LogicalProductNode newNode = new LogicalProductNode(GetNextNodeId());
                newNode.Children.Add(current);
                newNode.Children.Add(_nodeQueue.Dequeue());

                current = newNode;
            }

            return current;
        }

        public void PrintLogicalPlan()
        {
            if (RootNode != null)
            {
                Console.WriteLine("Logical Plan:");
                PrintNode(RootNode, 0);
            }
            else
            {
                Console.WriteLine("Logical plan is empty.");
            }
        }

        private void PrintNode(LogicalNode node, int indent)
        {
            StringBuilder indentBuilder = new StringBuilder();
            for (int i = 0; i < indent; i++)
            {
                indentBuilder.Append("  ");
            }

            string indentString = indentBuilder.ToString();

            switch (node)
            {
                case LogicalProjectionNode projectionNode:
                    Console.WriteLine($"{indentString}Projection: {string.Join(", ", projectionNode.Attributes)}");
                    foreach (LogicalNode child in node.Children)
                    {
                        PrintNode(child, indent + 1);
                    }
                    break;
                case LogicalSelectionNode selectionNode:
                    Console.WriteLine($"{indentString}Selection: {selectionNode.LeftOperand} {selectionNode.Op.ToString()} {selectionNode.RightOperand}");
                    foreach (LogicalNode child in node.Children)
                    {
                        PrintNode(child, indent + 1);
                    }
                    break;
                case LogicalJoinNode joinNode:
                    Console.WriteLine($"{indentString}Join: {joinNode.LeftColumn} = {joinNode.RightColumn}");
                    foreach (LogicalNode child in node.Children)
                    {
                        PrintNode(child, indent + 1);
                    }
                    break;
                case LogicalProductNode productNode:
                    Console.WriteLine($"{indentString}Product");
                    foreach (LogicalNode child in node.Children)
                    {
                        PrintNode(child, indent + 1);
                    }
                    break;
                case LogicalScanNode scanNode:
                    Console.WriteLine($"{indentString}Scan: {scanNode.TableName}");
                    break;
                default:
                    Console.WriteLine($"{indentString}Unknown node type");
                    foreach (LogicalNode child in node.Children)
                    {
                        PrintNode(child, indent + 1);
                    }
                    break;
            }
        }
    }
}
