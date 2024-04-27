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
            //LogicalProductNode root = CreateLeftDeepTree();
            LogicalProductNode root = CreateTotalProductNode();

            ldtParent.Children.Add(root);
            root.Parent = ldtParent;

        }

        private LogicalProductNode CreateTotalProductNode()
        {
            LogicalProductNode initial = new LogicalProductNode(GetNextNodeId());

            foreach (LogicalScanNode scanNode in ScanNodes) 
            {
                initial.Children.Add(scanNode);
            }

            return initial;
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

        public LogicalNode FindFirst(Func<LogicalNode, bool> predicate)
        {
            return FindFirstTraverseRecursive(RootNode, predicate);
        }

        // Finds the first node of given type
        public LogicalNode FindFirstTraverseRecursive(LogicalNode node, Func<LogicalNode, bool> predicate)
        {
            if (node == null) return null;

            if (predicate(node)) return node;

            foreach(LogicalNode child in node.Children)
            {
                LogicalNode returnNode = FindFirstTraverseRecursive(child, predicate);
                if (returnNode != null) return returnNode;
            }

            return null;
        }

        public List<LogicalNode> FindAll(Func<LogicalNode, bool> predicate)
        {
            return FindAllTraverseRecursive(RootNode, predicate);
        }

        public List<LogicalNode> FindAllTraverseRecursive(LogicalNode node, Func<LogicalNode, bool> predicate)
        {
            List<LogicalNode> matchingNodes = new List<LogicalNode>();

            if (node == null)
                return matchingNodes;

            if (predicate(node))
                matchingNodes.Add(node);

            foreach (LogicalNode child in node.Children)
            {
                matchingNodes.AddRange(FindAllTraverseRecursive(child, predicate));
            }

            return matchingNodes;
        }

        public void PrintLogicalPlan()
        {
            if (RootNode != null)
            {
                Console.WriteLine("Logical Plan:");
                PrintNode(RootNode, "", true);
            }
            else
            {
                Console.WriteLine("Logical plan is empty.");
            }
        }

        private void PrintNode(LogicalNode node, string prefix, bool isLast)
        {
            Console.WriteLine(prefix + (isLast ? "└─ " : "├─ ") + GetNodeDescription(node));

            var children = node.Children.ToList();
            for (int i = 0; i < children.Count; i++)
            {
                PrintNode(children[i], prefix + (isLast ? "   " : "│  "), i == children.Count - 1);
            }
        }

        private string GetNodeDescription(LogicalNode node)
        {
            switch (node)
            {
                case LogicalProjectionNode projectionNode:
                    return $"Projection: {string.Join(", ", projectionNode.Attributes)}";
                case LogicalSelectionNode selectionNode:
                    return $"Selection: {selectionNode.LeftOperand} {selectionNode.Op.ToString()} {selectionNode.RightOperand}";
                case LogicalJoinNode joinNode:
                    return $"Join: {joinNode.LeftTable}.{joinNode.LeftColumn} = {joinNode.RightTable}.{joinNode.RightColumn}";
                case LogicalProductNode productNode:
                    return "Product";
                case LogicalScanNode scanNode:
                    return $"Scan: {scanNode.TableName}";
                default:
                    return "Unknown node type";
            }
        }
    }
}
