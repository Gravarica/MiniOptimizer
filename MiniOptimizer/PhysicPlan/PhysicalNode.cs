using MiniOptimizer.LogicPlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.PhysicPlan
{
    public abstract class PhysicalNode
    {
        public HashSet<PhysicalNode> Children { get; set; } = new HashSet<PhysicalNode>();

        public abstract long EstimateExecutionCost();

        public abstract void Execute(); // Razmisliti o ovome za dalje
    }

    public class SequentialScan : PhysicalNode
    {
        public string TableName { get; set; }

        public override long EstimateExecutionCost()
        {
            throw new NotImplementedException();
        }

        public override void Execute()
        {
            Console.WriteLine("Executing Sequential scan...");
        }
    }

    // Index seek sluzi ako se polja koja se projektuju nalaze u indeksnoj strukturi
    public class IndexSeek : PhysicalNode
    {
        public string IndexName { get; set; }

        public override long EstimateExecutionCost()
        {
            throw new NotImplementedException();
        }

        public override void Execute()
        {
            Console.WriteLine("Executing Index seek...");
        }
    }

    public class IndexAccessScan : PhysicalNode
    {
        public string TableName { get; set; }
        public string IndexName { get; set; }

        public override long EstimateExecutionCost()
        {
            throw new NotImplementedException();
        }

        public override void Execute()
        {
            Console.WriteLine("Executing Index access scan...");
        }
    }

    public class Filter : PhysicalNode
    {
        public Op Op { get; set; }

        public string? LeftOperand { get; set; }

        public string? RightOperand { get; set; }

        public Filter(Op op, string leftOperand, string rightOperand)
        {
            Op = op;
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
        }

        public Filter(string leftOperand, string rightOperand)
        {
            Op = new Op(Predicate.EQ);
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
        }

        public override long EstimateExecutionCost()
        {
            throw new NotImplementedException();
        }

        public override void Execute()
        {
            Console.WriteLine("Executing Filter based on condition...");
        }
    }

    public class PhysicalProjection : PhysicalNode
    {
        HashSet<string> Attributes { get; set; } = new HashSet<string>();

        public PhysicalProjection(List<string> attributes)
        {
            attributes.ForEach(a => Attributes.Add(a));
        }

        public override long EstimateExecutionCost()
        {
            throw new NotImplementedException();
        }

        public override void Execute()
        {
            Console.WriteLine("Executing Physical projection...");
        }
    }

    public enum PhysicalJoinType { NESTED_LOOP, INDEX_NESTED_LOOP, HASH, SORT_MERGE };

    public class PhysicalJoin : PhysicalNode
    {
        
        public Op Op { get; set; }

        public string LeftTable { get; set; }

        public string RightTable { get; set; }

        public string LeftColumn { get; set; }

        public string RightColumn { get; set; }

        public PhysicalJoinType Type { get; set; }

        public PhysicalJoin(PhysicalJoinType type, string leftTable, string rightTable, string leftColumn, string rightColumn)
        {
            LeftTable = leftTable;
            RightTable = rightTable;
            LeftColumn = leftColumn;
            RightColumn = rightColumn;
            Op = new Op(Predicate.EQ);
            Type = type;
        }

        public override long EstimateExecutionCost()
        {
            throw new NotImplementedException();
        }

        public override void Execute()
        {
            Console.WriteLine("Executing join between");
        }
    }

    public class NestedLoopJoin : PhysicalJoin
    {
        public NestedLoopJoin(PhysicalJoinType type, string leftTable, string rightTable, string leftColumn, string rightColumn) 
            : base(type, leftTable, rightTable, leftColumn, rightColumn)
        {
        }

        public override long EstimateExecutionCost()
        {
            return base.EstimateExecutionCost();
        }

        public override void Execute()
        {
            base.Execute();
        }
    }

    public class SortMergeJoin : PhysicalJoin
    {
        public SortMergeJoin(PhysicalJoinType type, string leftTable, string rightTable, string leftColumn, string rightColumn)
            : base(type, leftTable, rightTable, leftColumn, rightColumn)
        {
        }

        public override long EstimateExecutionCost()
        {
            return base.EstimateExecutionCost();
        }

        public override void Execute()
        {
            base.Execute();
        }
    }

    public class IndexNestedLoopJoin : PhysicalJoin
    {
        public IndexNestedLoopJoin(PhysicalJoinType type, string leftTable, string rightTable, string leftColumn, string rightColumn)
            : base(type, leftTable, rightTable, leftColumn, rightColumn)
        {
        }

        public override long EstimateExecutionCost()
        {
            return base.EstimateExecutionCost();
        }

        public override void Execute()
        {
            base.Execute();
        }
    }

    public class HashJoin : PhysicalJoin
    {
        public HashJoin(PhysicalJoinType type, string leftTable, string rightTable, string leftColumn, string rightColumn)
            : base(type, leftTable, rightTable, leftColumn, rightColumn)
        {
        }

        public override long EstimateExecutionCost()
        {
            return base.EstimateExecutionCost();
        }

        public override void Execute()
        {
            base.Execute();
        }
    }
}
