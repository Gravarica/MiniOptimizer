using MiniOptimizer.LogicPlan;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.PhysicPlan
{
    public class PhysicalPlan
    {
        public PhysicalNode Root { get; set; }

        public List<PhysicalNode> ScanNodes { get; set; } = new List<PhysicalNode>();

        public PhysicalPlan() { }

        public PhysicalPlan(PhysicalNode root)
        {
            Root = root;
        }

        public void Print()
        {
            foreach (PhysicalNode scanNode in ScanNodes)
            {
                scanNode.Print();
            }
        }

        public void PrintPhysicalPlan()
        {
            if (Root != null)
            {
                Console.WriteLine("Physical Plan:");
                PrintPhysicalNode(Root, "", true);
            }
            else
            {
                Console.WriteLine("Physical plan is empty.");
            }
        }

        private void PrintPhysicalNode(PhysicalNode node, string prefix, bool isLast)
        {
            Console.WriteLine(prefix + (isLast ? "└─ " : "├─ ") + GetPhysicalNodeDescription(node));

            var children = node.Children.ToList();
            for (int i = 0; i < children.Count; i++)
            {
                PrintPhysicalNode(children[i], prefix + (isLast ? "   " : "│  "), i == children.Count - 1);
            }
        }

        private string GetPhysicalNodeDescription(PhysicalNode node)
        {
            switch (node)
            {
                case SequentialScan sequentialScan:
                    return $"Sequential Scan - Table: {sequentialScan.TableName}";
                case IndexSeek indexSeek:
                    return $"Index Seek - Table: {indexSeek.TableName}, Index: {indexSeek.IndexName}";
                case IndexScan indexScan:
                    return $"Index Scan - Table: {indexScan.TableName}, Index: {indexScan.IndexName}, Op: {indexScan.Op.ToString()}";
                case Filter filter:
                    return $"Filter - {filter.LeftOperand} {filter.Op.ToString()} {filter.RightOperand} on table: {filter.GetTableName()}";
                case PhysicalProjection projection:
                    return $"Projection - {string.Join(", ", projection.Attributes)}";
                case CrossProduct crossProduct:
                    return $"Cross Product - {crossProduct.LeftTable} |x| {crossProduct.RightTable}";
                case NestedLoopJoin nestedLoopJoin:
                    return $"Nested Loop Join - {nestedLoopJoin.LeftTable} |x| {nestedLoopJoin.RightTable} ON {nestedLoopJoin.LeftColumn} = {nestedLoopJoin.RightColumn}";
                case SortMergeJoin sortMergeJoin:
                    return $"Sort Merge Join - {sortMergeJoin.LeftTable} |x| {sortMergeJoin.RightTable} ON {sortMergeJoin.LeftColumn} = {sortMergeJoin.RightColumn}";
                case IndexNestedLoopJoin indexNestedLoopJoin:
                    return $"Index Nested Loop Join - {indexNestedLoopJoin.LeftTable} |x| {indexNestedLoopJoin.RightTable} ON {indexNestedLoopJoin.LeftColumn} = {indexNestedLoopJoin.RightColumn}";
                case HashJoin hashJoin:
                    return $"Hash Join - {hashJoin.LeftTable} |x| {hashJoin.RightTable} ON {hashJoin.LeftColumn} = {hashJoin.RightColumn}";
                default:
                    return "Unknown node type";
            }
        }
    }
}
