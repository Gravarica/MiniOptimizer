using MiniOptimizer.LogicPlan;
using MiniOptimizer.Metadata;
using MiniOptimizer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Antlr4.Runtime.Atn.SemanticContext;

namespace MiniOptimizer.Optimizer
{
    public class JoinOptimizer
    {
        private CostModel _costModel;
        private Dictionary<HashSet<LogicalRelationNode>, DPTableEntry> _dpTable;

        public JoinOptimizer(CostModel costModel)
        {
            _costModel = costModel;
        }

        public LogicalNode OptimizeJoin(LogicalPlan logicalPlan)
        {
            if (logicalPlan.ScanNodes.Count == 1) { return null; }

            _dpTable = new Dictionary<HashSet<LogicalRelationNode>, DPTableEntry>(HashSet<LogicalRelationNode>.CreateSetComparer());

            List<LogicalRelationNode> leafNodes = JoinOptimizerUtils.GetRelationNodes(logicalPlan);

            InitializeDPTable(leafNodes);

            DPTableEntry optimalEntry = ComputeOptimalJoinOrder(leafNodes);

            LogicalNode optimalJoinTree = ConstructOptimalJoinTree(optimalEntry);

            return optimalJoinTree;
        }

        private void InitializeDPTable(List<LogicalRelationNode> leafNodes)
        {
            foreach (LogicalRelationNode leafNode in leafNodes)
            {
                HashSet<LogicalRelationNode> singleSet = new HashSet<LogicalRelationNode> { leafNode };
                _dpTable[singleSet] = new DPTableEntry
                {
                    Cost = 0,
                    Size = leafNode.Cardinality,
                    JoinTree = leafNode
                };
            }

            for (int i = 0; i < leafNodes.Count - 1; i++)
            {
                var node1 = leafNodes[i];
                for (int j = i + 1; j < leafNodes.Count; j++)
                {
                    var node2 = leafNodes[j];
                    HashSet<LogicalRelationNode> pairSet = new HashSet<LogicalRelationNode> { node1, node2 };
                    var result = CalculateJoinCardinality(node1, node2);
                    LogicalNode joinTree;
                    if (result.Item1 == null)
                    {
                        joinTree = new LogicalProductNode(LogicalPlan.GetNextNodeId(), node1, node2);
                    }
                    else
                    {
                        joinTree = new LogicalJoinNode(LogicalPlan.GetNextNodeId(), node1, node2, result.Item1);
                    }
                    _costModel.EstimateCardinality(joinTree);
                    _dpTable[pairSet] = new DPTableEntry(0, result.Item2, joinTree);
                }
            }
        }

        private DPTableEntry ComputeOptimalJoinOrder(List<LogicalRelationNode> leafNodes)
        {
            for (int subsetSize = 3; subsetSize <= leafNodes.Count; subsetSize++)
            {
                foreach (HashSet<LogicalRelationNode> subset in JoinOptimizerUtils.GetSubsets(leafNodes, subsetSize))
                {
                    DPTableEntry minEntry = null;
                    foreach (LogicalRelationNode node in subset)
                    {
                        HashSet<LogicalRelationNode> remaining = new HashSet<LogicalRelationNode>(subset);
                        remaining.Remove(node);

                        DPTableEntry remainingEntry = _dpTable[remaining];
                        var result = CalculateJoinCardinality(remainingEntry.JoinTree, node);
                        long joinCost = remainingEntry.Size + result.Item2;

                        LogicalNode joinTree;
                        if (result.Item1 == null)
                        {
                            joinTree = new LogicalProductNode(LogicalPlan.GetNextNodeId(), remainingEntry.JoinTree, node);
                        }
                        else
                        {
                            joinTree = new LogicalJoinNode(LogicalPlan.GetNextNodeId(), remainingEntry.JoinTree, node, result.Item1);
                        }

                        _costModel.EstimateCardinality(joinTree);
                        DPTableEntry currentEntry = new DPTableEntry(remainingEntry.Cost + joinCost, joinCost, joinTree);

                        if (minEntry == null || currentEntry.Cost < minEntry.Cost)
                        {
                            minEntry = currentEntry;
                        }
                    }

                    _dpTable[subset] = minEntry;
                }
            }

            return _dpTable[new HashSet<LogicalRelationNode>(leafNodes)];
        }

        private LogicalNode ConstructOptimalJoinTree(DPTableEntry optimalEntry)
        {
            return optimalEntry.JoinTree;
        }

        private (string, long) CalculateJoinCardinality(LogicalNode node1, LogicalNode node2)
        {
            var column = JoinOptimizerUtils.CanBeJoined(node1, node2);
            if (column == null) return (null, node1.Cardinality * node2.Cardinality);

            return (column, _costModel.EstimatePotentialJoinCardinality(node1, node2, column));
        }

    }
}
