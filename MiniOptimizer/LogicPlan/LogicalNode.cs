using MiniOptimizer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MiniOptimizer.LogicPlan
{

    public enum LogicalNodeType
    {
        SELECTION, PROJECTION, JOIN, PRODUCT, SCAN
    }

    public enum PredicateType
    {
        JOIN, FILTER
    }

    public class LogicalNode
    {
        public int Id {  get; set; }
        public HashSet<LogicalNode> Children { get; private set; }
        public LogicalNode? Parent { get; set; }

        public long Cardinality { get; set; }

        public Dictionary<string, long> DistinctValues { get; set; }

        public LogicalNodeType Type { get ; set; }

        public LogicalNode(int id)
        {
            Id = id;
            Children = new HashSet<LogicalNode>();
        }

        public LogicalNode() 
        {
            Children = new HashSet<LogicalNode>();
        }

        public void AddChild(LogicalNode child)
        {
            Children.Add(child);
            child.Parent = this;
        }

        public void RemoveChild(LogicalNode child)
        {
            Children.Remove(child);
            child.Parent = null;
        }

        public virtual string GetTableName(int position = 0)
        {
            return "";
        }
    }

    public class LogicalProjectionNode : LogicalNode
    {
        public HashSet<string> Attributes { get; set; }

        public LogicalProjectionNode(int id) : base(id)
        {
            Attributes = new HashSet<string>();
            Type = LogicalNodeType.PROJECTION;
        }

        public LogicalProjectionNode(string Attribute)
        {
            Attributes = new HashSet<string>();
            Attributes.Add(Attribute);
            Type = LogicalNodeType.PROJECTION;
        }

        public LogicalProjectionNode(HashSet<string> attributes)
        {
            Attributes = attributes;
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

        public override string GetTableName(int position = 0)
        {
            return Children.First().GetTableName(position);
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

        public override string GetTableName(int position = 0)
        {
            return TableName;
        }
    }

    public class LogicalSelectionNode : LogicalNode
    {
        public Op Op { get; set; }

        public string? LeftOperand { get; set; }

        public string? RightOperand { get; set; } 

        public PredicateType PredicateType { get; set; }

        public LogicalSelectionNode(int id, PredicateType type, Op op, string? column, string? value) : base(id)
        {
            Op = op;
            LeftOperand = column;
            RightOperand = value;
            Type = LogicalNodeType.SELECTION;
            PredicateType = type;
        }

        public LogicalSelectionNode(int id) : base(id)
        {
            Type = LogicalNodeType.SELECTION;
        }

        public override string GetTableName(int position = 0)
        {
            var qualifiedName = ParseHelper.ParseQualifiedName(LeftOperand);
            return qualifiedName.Item1;
        }

        public string GetColumnName(int position)
        {
            var name = position == 0 ? LeftOperand : RightOperand;
            var qualifiedName = ParseHelper.ParseQualifiedName(name);
            return qualifiedName.Item2;
        }
    }

    public class LogicalJoinNode : LogicalNode
    {
        public Op JoinOp { get; set; }
        public string? LeftColumn { get; set; }
        public string? RightColumn { get; set; }
        public string? LeftTable { get; set; }

        public string? RightTable { get; set; }    

        public LogicalJoinNode(int id, Op op, string? leftTable, string? rightTable) : base(id)
        {
            JoinOp = op;
            LeftTable = leftTable;
            RightTable = rightTable;
            Type = LogicalNodeType.JOIN;
        }

        public LogicalJoinNode(int id, string? leftColumn, string? rightColumn, string? leftTable, string? rightTable) : base(id)
        {
            LeftTable = leftTable ?? string.Empty;
            RightTable = rightTable ?? string.Empty;
            LeftColumn = leftColumn ?? string.Empty;
            RightColumn = rightColumn ?? string.Empty;
            Type = LogicalNodeType.JOIN;
            JoinOp = new Op(Predicate.EQ);
        }

        public override string GetTableName(int position)
        {
            return position == 0 ? LeftTable : RightTable;
        }
    }

    public class LogicalProductNode: LogicalNode
    {
        public LogicalProductNode(int id) : base(id) {
            Type = LogicalNodeType.PRODUCT; 
        }
    }
}
