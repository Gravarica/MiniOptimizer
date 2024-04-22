using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace MiniOptimizer
{
    public class Parser
    {
        MiniQLParser parser_;
        

        public Parser() { }
    }

    class MiniQLVisitor : MiniQLBaseVisitor<object>
    {
        private int _nodeId = 0;
        private Dictionary<string, int> _tableAliasMap = new Dictionary<string, int>();

        public override LogicalNode VisitQuery([NotNull] MiniQLParser.QueryContext context)
        {
            var projectionNode = VisitAttributeList(context.attributeList());
            var scanNodes = VisitRelationList(context.relationList());

            if (context.condition() != null)
            {
                var selectionNode = VisitCondition(context.condition());
                selectionNode.AddChild(scanNodes);
                projectionNode.AddChild(selectionNode);
            }
            else
            {
                projectionNode.AddChild(scanNodes);
            }

            return projectionNode;
        }

        public override LogicalNode VisitAttributeList([NotNull] MiniQLParser.AttributeListContext context)
        {
            var projectionNode = new LogicalProjectionNode(_nodeId++);

            foreach (var attributeContext in context.attribute())
            {
                projectionNode.AddAttribute(attributeContext.GetText());
            }

            return projectionNode;
        }

        public override LogicalNode VisitRelationList([NotNull] MiniQLParser.RelationListContext context)
        {
            var scanNodes = new List<LogicalScanNode>();

            foreach (var relationContext in context.relation())
            {
                var tableName = relationContext.GetText();
                var tableId = _tableAliasMap.GetValueOrDefault(tableName, _tableAliasMap.Count);
                _tableAliasMap[tableName] = tableId;

                var scanNode = new LogicalScanNode(_nodeId++, tableName, tableId);
                scanNodes.Add(scanNode);
            }

            return scanNodes.Count == 1 ? scanNodes[0] : new LogicalNode(_nodeId++) { Children = scanNodes };
        }

        public override LogicalNode VisitCondition([NotNull] MiniQLParser.ConditionContext context)
        {
            if (context.ChildCount == 3)
            {
                var leftCondition = VisitCondition(context.condition(0));
                var rightCondition = VisitCondition(context.condition(1));

                var andNode = new LogicalNode(_nodeId++);
                andNode.AddChild(leftCondition);
                andNode.AddChild(rightCondition);

                return andNode;
            }
            else
            {
                var leftAttribute = VisitAttribute(context.attribute(0));
                var rightAttribute = VisitAttribute(context.attribute(1));

                LogicalScanNode? leftScanNode = leftAttribute as LogicalScanNode;
                LogicalScanNode? rightScanNode = rightAttribute as LogicalScanNode;

                if (leftScanNode != null && rightScanNode != null)
                {
                    // Both left and right attributes are qualified names (join condition)
                    var leftColumn = leftAttribute.GetText().Split('.')[1];
                    var rightColumn = rightAttribute.GetText().Split('.')[1];

                    var selectionNode = new LogicalSelectionNode(_nodeId++, Op.EQ, null, leftColumn, rightColumn);
                    selectionNode.AddChild(leftScanNode);
                    selectionNode.AddChild(rightScanNode);
                    return selectionNode;
                }
                else
                {
                    // One attribute is a column name, and the other is a constant
                    LogicalNode? scanNode = leftScanNode ?? rightScanNode;
                    string? column = leftScanNode != null ? rightAttribute.GetText() : leftAttribute.GetText();
                    string? value = leftScanNode == null ? leftAttribute.GetText() : rightAttribute.GetText();

                    var selectionNode = new LogicalSelectionNode(_nodeId++, Op.EQ, scanNode?.Alias, column, value);
                    if (scanNode != null)
                    {
                        selectionNode.AddChild(scanNode);
                    }
                    return selectionNode;
                }
            }
        }

        public override LogicalNode VisitAttribute([NotNull] MiniQLParser.AttributeContext context)
        {
            if (context.constant() != null)
            {
                return VisitConstant(context.constant());
            }
            else
            {
                var identifier = context.identifier().GetText();
                var parts = identifier.Split('.');

                if (parts.Length == 2)
                {
                    var tableAlias = parts[0];
                    var tableId = _tableAliasMap[tableAlias];
                    return new LogicalScanNode(_nodeId++, tableAlias, tableId);
                }
                else
                {
                    return new LogicalProjectionNode(identifier);
                }
            }
        }

        public override LogicalNode VisitConstant([NotNull] MiniQLParser.ConstantContext context)
        {
            return new LogicalProjectionNode(context.GetText());
        }
    }
}
