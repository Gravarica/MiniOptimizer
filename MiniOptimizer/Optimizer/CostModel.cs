using MiniOptimizer.Exceptions;
using MiniOptimizer.LogicPlan;
using MiniOptimizer.Metadata;
using MiniOptimizer.PhysicPlan;
using MiniOptimizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Optimizer
{
    public class CostModel
    {
        private Catalog _catalog;

        public CostModel(Catalog catalog)
        {
            _catalog = catalog;
        }

        public long EstimateCardinality(LogicalNode node)
        {
            long cardinality;
            switch (node.Type) 
            {
                case LogicalNodeType.SCAN:
                    cardinality = EstimateScanCardinality(node as LogicalScanNode);
                    break;
                case LogicalNodeType.SELECTION:
                    cardinality = EstimateSelectionCardinality(node as LogicalSelectionNode); 
                    break;
                case LogicalNodeType.PROJECTION:
                    cardinality = EstimateProjectionCardinality(node as LogicalProjectionNode);
                    break;
                case LogicalNodeType.JOIN:
                    cardinality = EstimateJoinCardinality(node as LogicalJoinNode);
                    break;
                case LogicalNodeType.PRODUCT:
                    cardinality = EstimateProductCardinality(node as LogicalProductNode);
                    break;
                default:
                    throw new BaseException("Invalid node type.");
            }

            node.Cardinality = cardinality;
            return cardinality;
        }

        public long EstimateCost(PhysicalNode physicalNode) {
            long cost = 0; 
            switch(physicalNode)
            {
                case PhysicalScanNode scanNode:
                    cost = EstimatePhysicalScanCost(scanNode);
                    break;
                case Filter filterNode:
                    cost = EstimateFilterCost(filterNode);
                    break;
                case PhysicalJoin joinNode:
                    cost = EstimatePhysicalJoinCost(joinNode);
                    break;
                case PhysicalProjection physProjNode:
                    cost = EstimatePhysicalProjectionCost(physProjNode);
                    break;
                default:
                    throw new BaseException("Invalid node type.");
            }

            return cost;
        }

        private long EstimatePhysicalJoinCost(PhysicalJoin node)
        {
            return 0;
        }

        private long EstimatePhysicalProjectionCost(PhysicalProjection physicalProjection)
        {
            TableStats statistics = _catalog.GetTableStats(physicalProjection.GetTableName());
            return physicalProjection.EstimateExecutionCost(statistics);
        }

        private long EstimateFilterCost(Filter filter)
        {
            TableStats statistics = _catalog.GetTableStats(filter.GetTableName());
            return filter.EstimateExecutionCost(statistics);
        }

        private long EstimatePhysicalScanCost(PhysicalScanNode scanNode)
        {
            TableStats statistics = _catalog.GetTableStats(scanNode.GetTableName());
            return scanNode.EstimateExecutionCost(statistics);
        }

        public long EstimateScanCardinality(LogicalScanNode node)
        {
            var tableStats = _catalog.GetTableStats(node.TableName);
            node.DistinctValues = CostModelUtil.TakeScanDistinctValues(tableStats);

            return tableStats.RowCount;
        }

        public long EstimateProjectionCardinality(LogicalProjectionNode node)
        {
            if (node.Children.Count == 0)
                return 0;

            node.DistinctValues = CostModelUtil.TakeDistinctValuesForProjection(node.Children.First().DistinctValues, node.Attributes);
            return node.Children.First().Cardinality;
        }

        public long EstimateSelectionCardinality(LogicalSelectionNode node)
        {
            if (node.Children.Count == 0) 
                return 0;

            var childCardinality = node.Children.First().Cardinality;
            long distinctValues = 1;

            var qualifiedName = ParseHelper.ParseQualifiedName(node.LeftOperand);

            long cardinality;
            if (node.Children.First() is LogicalScanNode scanNode)
            {
                var tableStats = _catalog.GetTableStats(scanNode.TableName);
                var columnStats = tableStats.ColumnStats[qualifiedName.Item2];

                object value = ParseHelper.ParseRightOperand(node.RightOperand);
                // Ako je jednakost onda gledaj histogram
                if (Op.Predicate == Predicate.EQ)
                {
                    cardinality = EstimateSelectionCardinalityFromHistogram(columnStats.Histogram, value, childCardinality);
                }
                else
                {
                    distinctValues = columnStats.DistinctValues / 3;
                    cardinality = childCardinality / 3;
                }
            }
            else
            {
                if (Op.Predicate != Predicate.EQ) return 0; // Necemo se baviti ovim slucajem
                cardinality = childCardinality / node.Children.First().DistinctValues[qualifiedName.Item2];
            }

            node.DistinctValues = CostModelUtil.TakeDistinctValues(node.Children.First().DistinctValues, qualifiedName.Item2, distinctValues);
            return cardinality;
        }

        //public long EstimatePotentialJoinCardinality(LogicalNode node1, LogicalNode node2)
        //{

        //}

        private long EstimateJoinCardinality(LogicalJoinNode node)
        {
            // Pretpostavka je da ce oba deteta biti relacije - Ovo je zbog DP algoritma
            if (!(node.Children.First() is LogicalScanNode leftScanNode) ||
                !(node.Children.Last() is LogicalScanNode rightScanNode))
                return EstimateJoinCardinalityForIntermediateRelations(node);

            var leftTableStats = _catalog.GetTableStats(leftScanNode.TableName);
            var rightTableStats = _catalog.GetTableStats(rightScanNode.TableName);

            var ltColumnStats = leftTableStats.ColumnStats[node.LeftColumn];
            var rtColumnStats = rightTableStats.ColumnStats[node.RightColumn];

            node.DistinctValues = CostModelUtil.GetDistinctValuesForJoin(node.Children.First().DistinctValues,
                                                                         node.Children.Last().DistinctValues,
                                                                         node.LeftColumn,
                                                                         Math.Min(ltColumnStats.DistinctValues, rtColumnStats.DistinctValues));

            return EstimateCardinalityForEquiJoin(
                ltColumnStats.Histogram,
                rtColumnStats.Histogram,
                leftTableStats.RowCount,
                rightTableStats.RowCount
            );
        }

        private long EstimateJoinCardinalityForIntermediateRelations(LogicalJoinNode node)
        {
            // Ne moram da proveravam, znam da je desni tabela a levi join 
            var leftCardinality = node.Children.First().Cardinality;
            var leftDistinctValues = node.Children.First().DistinctValues[node.LeftColumn];
            
            if (node.Children.Last() is LogicalScanNode scanNode)
            {
                var rightTableStats = _catalog.GetTableStats(scanNode.TableName);
                var rtColumnStats = rightTableStats.ColumnStats[node.RightColumn];

                node.DistinctValues = CostModelUtil.GetDistinctValuesForJoin(node.Children.First().DistinctValues,
                                                                         node.Children.Last().DistinctValues,
                                                                         node.LeftColumn,
                                                                         Math.Min(rtColumnStats.DistinctValues, leftDistinctValues));

                return rightTableStats.RowCount * leftCardinality / Math.Max(leftDistinctValues, rtColumnStats.DistinctValues);
            }

            var rightCardinality = node.Children.Last().Cardinality;
            var rightDistinctValues = node.Children.Last().DistinctValues[node.RightColumn];

            node.DistinctValues = CostModelUtil.GetDistinctValuesForJoin(node.Children.First().DistinctValues,
                                                                         node.Children.Last().DistinctValues,
                                                                         node.LeftColumn,
                                                                         Math.Min(rightDistinctValues, leftDistinctValues));

            return leftCardinality * rightCardinality / Math.Max(rightDistinctValues, leftDistinctValues);
        }

        private long EstimateProductCardinality(LogicalProductNode node)
        {
            return node.Children.Last().Cardinality * node.Children.First().Cardinality;
        }

        private long EstimateSelectionCardinalityFromHistogram(Histogram histogram, object value, double baseCardinality)
        {
            double selectivity = 0;

            foreach (var bucket in histogram.Buckets)
            {
                if (bucket.LowerBound <= (int)value && bucket.UpperBound >= (int)value)
                {
                    selectivity += bucket.RowCount / baseCardinality;
                }
            }

            return (long) Math.Ceiling(baseCardinality * selectivity);
        }

        private long EstimateCardinalityForEquiJoin(Histogram leftHistogram, Histogram rightHistogram, double leftCardinality, double rightCardinality)
        {
            double totalCardinality = 0;

            foreach (var leftBucket in leftHistogram.Buckets)
            {
                foreach (var rightBucket in rightHistogram.Buckets)
                {
                    if (leftBucket.LowerBound.Equals(rightBucket.LowerBound) && leftBucket.UpperBound.Equals(rightBucket.UpperBound))
                    {
                        double leftSelectivity = (double)leftBucket.RowCount / leftCardinality;
                        double rightSelectivity = (double)rightBucket.RowCount / rightCardinality;
                        totalCardinality += leftCardinality * rightSelectivity * leftSelectivity;
                    }
                }
            }

            return (long) Math.Ceiling(totalCardinality);
        }
    }
}
