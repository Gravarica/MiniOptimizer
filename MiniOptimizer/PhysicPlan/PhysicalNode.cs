using MiniOptimizer.LogicPlan;
using MiniOptimizer.Metadata;
using MiniOptimizer.Optimizer;
using MiniOptimizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MiniOptimizer.PhysicPlan
{

    public abstract class PhysicalNode
    {
        public long Cost { get; set; }

        public PhysicalNode Parent { get; set; }

        public HashSet<PhysicalNode> Children { get; set; } = new HashSet<PhysicalNode>();

        public abstract long EstimateExecutionCost(TableStats? statistics = null);

        public abstract void Execute(); // Razmisliti o ovome za dalje

        public abstract string GetTableName(int position);

        public void AddChild(PhysicalNode child)
        {
            Children.Add(child);
            child.Parent = this;
        }

        public abstract void Print();
    }

    public enum AccessMethod { SEQ_SCAN, INDEX_SEEK, INDEX_SCAN}

    public class PhysicalScanNode : PhysicalNode
    {
        public string TableName { get; set; }

        public AccessMethod AccessMethod { get; set; }

        public PhysicalScanNode() { }

        public PhysicalScanNode(string tableName, AccessMethod accessMethod)
        {
            TableName=tableName;
            AccessMethod=accessMethod;
        }

        public override long EstimateExecutionCost(TableStats statistics)
        {
            throw new NotImplementedException();
        }

        public override void Execute()
        {
            Console.WriteLine("Access method for the table");
        }

        public override string GetTableName(int position = 0)
        {
            return TableName;
        }

        public override void Print()
        {
            Console.WriteLine("Physical Scan Node");
        }
    }

    public class SequentialScan : PhysicalScanNode
    {
        public SequentialScan() { }

        public SequentialScan(string tableName) : base(tableName, AccessMethod.SEQ_SCAN) { } 

        public override long EstimateExecutionCost(TableStats statistics)
        {
            return statistics.GetNumberOfBlocks();
        }

        public override void Execute()
        {
            Console.WriteLine("Executing Sequential scan...");
        }

        public override void Print()
        {
            Console.WriteLine("Sequential Scan - Table: " + TableName);
        }
    }

    // Index seek sluzi ako se polja koja se projektuju nalaze u indeksnoj strukturi
    // Ovo znaci -> Citaj samo iz indeksa, ne moras da pristupis tabeli
    public class IndexSeek : PhysicalScanNode
    {
        public string IndexName { get; set; }

        public IndexSeek() { 
            AccessMethod = AccessMethod.INDEX_SEEK;
        }

        public IndexSeek(string tableName, string indexName) : base(tableName, AccessMethod.INDEX_SEEK)
        {
            IndexName=indexName;
        }

        public override long EstimateExecutionCost(TableStats statistics)
        {
            return 0; // Force IndexSeek if it is possible -> IndexSeek means that it will read all data from the index only
        }

        public override void Execute()
        {
            Console.WriteLine("Executing Index seek...");
        }

        public override void Print()
        {
            Console.WriteLine("Index Seek - Table: " + TableName + " Index: " + IndexName);
        }
    }

    public class IndexScan : PhysicalScanNode
    {
        public string IndexName { get; set; }

        public Op Op { get; set; }

        public IndexScan() { 
            AccessMethod= AccessMethod.INDEX_SCAN;  
        }

        public IndexScan(string tableName, string indexName, Op op) : base(tableName, AccessMethod.INDEX_SCAN)
        {
            IndexName = indexName;
            Op = op;
        }

        public IndexScan(string tableName, string indexName) : base(tableName, AccessMethod.INDEX_SCAN)
        {
            IndexName = indexName;
            Op = new Op(Predicate.EQ);
        }

        public override long EstimateExecutionCost(TableStats statistics)
        {
            long distinctValues = statistics.GetDistinctValues(IndexName);
            if (Op.Predicate == Predicate.EQ) 
                return statistics.GetNumberOfBlocks() / distinctValues;

            return statistics.GetNumberOfBlocks() / 3;
        }

        public override void Execute()
        {
            Console.WriteLine("Executing Index access scan...");
        }

        public override void Print()
        {
            Console.WriteLine("Index Scan - Table: " + TableName + " Index: " + IndexName + " Op: " + Op.Predicate.ToString());
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

        public override long EstimateExecutionCost(TableStats? stats = null)
        {
            return Children.First().EstimateExecutionCost(stats);
        }

        public override void Execute()
        {
            Console.WriteLine("Executing Filter based on condition...");
        }

        public override string GetTableName(int position = 0)
        {
            var qualifiedName = ParseHelper.ParseQualifiedName(LeftOperand);
            return qualifiedName.Item1;
        }

        public override void Print()
        {
            Console.WriteLine("Filter - : " + LeftOperand + " " + Op.Predicate.ToString() + " " + RightOperand + " on table: " + GetTableName());
            Children.First().Print();
        }
    }

    public class PhysicalProjection : PhysicalNode
    {
        public HashSet<string> Attributes { get; set; } = new HashSet<string>();

        public PhysicalProjection(List<string> attributes)
        {
            attributes.ForEach(a => Attributes.Add(a));
        }

        public PhysicalProjection(HashSet<string> attributes)
        {
            Attributes = attributes;
        }

        public PhysicalProjection(LogicalProjectionNode node)
        {
            Attributes = node.Attributes;
        }

        public override long EstimateExecutionCost(TableStats? statistics = null)
        {
            return (Attributes.Count * 4 / statistics.TupleSize) * Children.First().EstimateExecutionCost(statistics);
        }

        public override void Execute()
        {
            Console.WriteLine("Executing Physical projection...");
            Children.First().Print();
        }

        public override string GetTableName(int position = 0)
        {
            return Children.First().GetTableName(position);
        }

        public override void Print()
        {
            Console.Write("Projection - : ");
            foreach (var item in Attributes)
            {
                Console.Write(item + " ");
            }
            Console.Write("\n");
            Children.First().Print();
        }
    }

    public enum PhysicalJoinType { NESTED_LOOP, INDEX_NESTED_LOOP, HASH, SORT_MERGE };

    public class CrossProduct : PhysicalNode
    {
        public string LeftTable { get; set; }

        public string RightTable { get; set; }

        public CrossProduct() { }

        public CrossProduct(string leftTable, string rightTable)
        {
            LeftTable=leftTable;
            RightTable=rightTable;
        }

        public override long EstimateExecutionCost(TableStats? statistics = null)
        {
            throw new NotImplementedException();
        }

        public override void Execute()
        {
            Console.WriteLine("Executing Cross Product");
        }

        public override string GetTableName(int position)
        {
            return position == 0 ? LeftTable : RightTable;
        }

        public override void Print()
        {
            Console.WriteLine("Cross Product: " + LeftTable + " |x| " + RightTable);
        }
    }

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

        public override long EstimateExecutionCost(TableStats? statistics = null)
        {
            throw new NotImplementedException();
        }

        public override void Execute()
        {
            Console.WriteLine("Executing join between");
        }

        public override string GetTableName(int position)
        {
            return position == 0 ? LeftTable : RightTable;
        }

        public override void Print()
        {
            Console.WriteLine("Join: " + LeftTable + " |x| " + RightTable + " ON " + LeftColumn + " = " + RightColumn);
        }
    }

    public class NestedLoopJoin : PhysicalJoin
    {
        public NestedLoopJoin(string leftTable, string rightTable, string leftColumn, string rightColumn) 
            : base(PhysicalJoinType.NESTED_LOOP, leftTable, rightTable, leftColumn, rightColumn)
        {
        }

        public NestedLoopJoin(LogicalJoinNode ljn) : base(PhysicalJoinType.NESTED_LOOP, ljn.LeftTable, ljn.RightTable, ljn.LeftColumn, ljn.RightColumn)
        {
            Op = ljn.JoinOp;
        }

        public override long EstimateExecutionCost(TableStats? statistics = null)
        {
            return base.EstimateExecutionCost();
        }

        public override void Execute()
        {
            base.Execute();
        }

        public override void Print()
        {
            Console.WriteLine("Nested Loop Join: " + LeftTable + " |x| " + RightTable + " ON " + LeftColumn + " = " + RightColumn);
        }
    }

    public class SortMergeJoin : PhysicalJoin
    {
        public SortMergeJoin(PhysicalJoinType type, string leftTable, string rightTable, string leftColumn, string rightColumn)
            : base(type, leftTable, rightTable, leftColumn, rightColumn)
        {
        }

        public override long EstimateExecutionCost(TableStats? statistics = null)
        {
            return base.EstimateExecutionCost();
        }

        public override void Execute()
        {
            base.Execute();
        }

        public override void Print()
        {
            Console.WriteLine("Sort Merge Join: " + LeftTable + " |x| " + RightTable + " ON " + LeftColumn + " = " + RightColumn);
        }
    }

    public class IndexNestedLoopJoin : PhysicalJoin
    {
        public IndexNestedLoopJoin(PhysicalJoinType type, string leftTable, string rightTable, string leftColumn, string rightColumn)
            : base(type, leftTable, rightTable, leftColumn, rightColumn)
        {
        }

        public override long EstimateExecutionCost(TableStats? statistics = null)
        {
            return base.EstimateExecutionCost();
        }

        public override void Execute()
        {
            base.Execute();
        }

        public override void Print()
        {
            Console.WriteLine("Index Nested Loop Join: " + LeftTable + " |x| " + RightTable + " ON " + LeftColumn + " = " + RightColumn);
        }
    }

    public class HashJoin : PhysicalJoin
    {
        public HashJoin(PhysicalJoinType type, string leftTable, string rightTable, string leftColumn, string rightColumn)
            : base(type, leftTable, rightTable, leftColumn, rightColumn)
        {
        }

        public override long EstimateExecutionCost(TableStats? statistics = null)
        {
            return base.EstimateExecutionCost();
        }

        public override void Execute()
        {
            base.Execute();
        }

        public override void Print()
        {
            Console.WriteLine("Hash Join: " + LeftTable + " |x| " + RightTable + " ON " + LeftColumn + " = " + RightColumn);
        }
    }
}
