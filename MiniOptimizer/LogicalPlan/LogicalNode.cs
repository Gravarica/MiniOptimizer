using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer
{

    public enum LogicalNodeType
    {
        SELECTION, PROJECTION, JOIN, PRODUCT, SCAN
    }

    public class LogicalNode
    {

        public int Id {  get; set; }
        public List<LogicalNode> Children { get; private set; }
        public LogicalNode? Parent { get; set; }

        public LogicalNodeType Type { get ; set; }

        public LogicalNode(int id)
        {
            Id = id;
            Children = new List<LogicalNode>();
        }

        public LogicalNode() 
        {
            Children = new List<LogicalNode>();
        }

        public void AddChild(LogicalNode child)
        {
            Children.Add(child);
            child.Parent = this;
        }
    }

    public class LogicalProjectionNode : LogicalNode
    {
        public List<string> Attributes { get; set; }

        public LogicalProjectionNode(int id) : base(id)
        {
            Attributes = new List<string>();
            Type = LogicalNodeType.PROJECTION;
        }

        public LogicalProjectionNode(string Attribute)
        {
            Attributes = new List<string>();
            Attributes.Add(Attribute);
            Type = LogicalNodeType.PROJECTION;
        }

        public void AddAttribute(string attribute)
        {
            Attributes.Add(attribute);
        }

        public LogicalProjectionNode SplitProjection(string Attribute)
        {
            Attributes.Remove(Attribute);
            return new LogicalProjectionNode(Attribute);
        }
    }

    public class LogicalScanNode : LogicalNode 
    {
        public string? TableName { get; set; }

        public int TableId { get; set; }

        public LogicalScanNode(int id, string alias, int tableId) : base(id) 
        {
            TableName = alias; 
            TableId = tableId;
            Type = LogicalNodeType.SCAN;
        }

        public LogicalScanNode(int id , int tableId) : base(id)
        {
            TableId=tableId;
            Type = LogicalNodeType.SCAN;
        }
    }

    public class LogicalSelectionNode : LogicalNode
    {
        public Op Op { get; set; }

        public string? LeftOperand { get; set; }

        public string? RightOperand { get; set; } 

        public LogicalSelectionNode(int id, Op op, string? column, string? value) : base(id)
        {
            Op = op;
            LeftOperand = column;
            RightOperand = value;
            Type = LogicalNodeType.SELECTION;
        }

        public LogicalSelectionNode(int id) : base(id)
        {
            Type = LogicalNodeType.SELECTION;
        }
    }

    public class LogicalJoinNode : LogicalNode
    {
        public Op JoinOp { get; set; }
        public string? LeftColumn { get; set; }
        public string? RightColumn { get; set; }

        public LogicalJoinNode(int id, Op op, string? leftColumn, string? rightColumn) : base(id)
        {
            JoinOp = op;
            LeftColumn = leftColumn;
            RightColumn = rightColumn;
            Type = LogicalNodeType.JOIN;
        }
    }

    public class LogicalProductNode: LogicalNode
    {
        public LogicalProductNode(int id) : base(id) {
            Type = LogicalNodeType.PRODUCT; 
        }
    }
}
