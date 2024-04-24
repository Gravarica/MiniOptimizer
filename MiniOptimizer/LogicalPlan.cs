using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer
{
    public class LogicalPlan
    {
        public LogicalNode RootNode { get; private set; }
        private int _nodeId = 0;

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

        public LogicalNode CreateSelectionNode(Op op, string leftOperand, string rightOperand)
        {
            return new LogicalSelectionNode(GetNextNodeId(), op, leftOperand, rightOperand);
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

        public int GetNextNodeId()
        {
            return _nodeId++;
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
                    Console.WriteLine($"{indentString}Projection: {string.Join(", ", projectionNode.ProjectedAttributes)}");
                    foreach (LogicalNode child in node.Children)
                    {
                        PrintNode(child, indent + 1);
                    }
                    break;
                case LogicalSelectionNode selectionNode:
                    Console.WriteLine($"{indentString}Selection: {selectionNode.LeftOperand} {selectionNode.Operator} {selectionNode.RightOperand}");
                    foreach (LogicalNode child in node.Children)
                    {
                        PrintNode(child, indent + 1);
                    }
                    break;
                case LogicalJoinNode joinNode:
                    Console.WriteLine($"{indentString}Join: {joinNode.JoinType}, {joinNode.LeftJoinAttribute} {joinNode.JoinCondition} {joinNode.RightJoinAttribute}");
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
