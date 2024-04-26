using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using MiniOptimizer;
using MiniOptimizer.Compiler;
using MiniOptimizer.Exceptions;
using MiniOptimizer.LogicPlan;
using MiniOptimizer.Metadata;
using System.Collections.Generic;

namespace MiniOptimizer.Compiler
{
    public class Parser
    {
        private Catalog _catalog;
        public LogicalPlan _logicalPlan;
        public SemanticAnalyzer SemanticAnalyzer;

        public Parser(Catalog catalog)
        {
            _catalog=catalog;
            SemanticAnalyzer = new SemanticAnalyzer(catalog);
        }

        public void TurnOffSemanticAnalysis()
        {
            SemanticAnalyzer.TurnOff();
        }

        public void TurnOnSemanticAnalysis()
        {
            SemanticAnalyzer.TurnOn();
        }

        public LogicalPlan Parse(string input)
        {
            AntlrInputStream inputStream = new AntlrInputStream(input);
            MiniQLLexer lexer = new MiniQLLexer(inputStream);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            MiniQLParser parser = new MiniQLParser(tokens);

            var queryContext = parser.query();

            _logicalPlan = new LogicalPlan();

            VisitQuery(queryContext);

            return _logicalPlan;
        }

        private void VisitQuery(MiniQLParser.QueryContext context)
        {
            List<string> projectedAttributes = VisitAttributeList(context.attributeList());
            VisitRelationList(context.relationList());

            LogicalProjectionNode projectionNode = (LogicalProjectionNode)_logicalPlan.CreateProjectionNode(projectedAttributes);
            _logicalPlan.ProjectionNodes.Add(projectionNode);

            if (context.condition() != null)
            {
                VisitCondition(context.condition());
            }
        }

        private List<string> VisitAttributeList(MiniQLParser.AttributeListContext context)
        {
            List<string> attributes = new List<string>();
            foreach (MiniQLParser.AttributeContext attributeContext in context.attribute())
            {
                string attribute = attributeContext.GetText();
             
                SemanticAnalyzer.CheckAttribute(attribute);

                attributes.Add(attribute);
            }
            return attributes;
        }

        private void VisitRelationList(MiniQLParser.RelationListContext context)
        {

            foreach (MiniQLParser.RelationContext relationContext in context.relation())
            {
                string tableName = relationContext.GetText();

                SemanticAnalyzer.CheckIfTableExists(tableName); 

                int tableId = 1;
                LogicalScanNode node = new LogicalScanNode(LogicalPlan.GetNextNodeId(), tableName, tableId);
                _logicalPlan.ScanNodes.Add(node);
            }
        }

        private void VisitCondition(MiniQLParser.ConditionContext context)
        {

            if (context.AND() != null)
            {
                VisitCondition(context.condition(0));
                VisitCondition(context.condition(1));
            }
            else
            {
                SemanticAnalyzer.CheckPredicateOperands(context.attribute(0), context.attribute(1));

                string leftOperand = context.attribute(0).GetText();
                string rightOperand = context.attribute(1).GetText();
                Op op = new Op(Predicate.EQ);

                PredicateType type = context.attribute(1).constant() != null ? PredicateType.FILTER : PredicateType.JOIN;

                LogicalSelectionNode selectionNode = new LogicalSelectionNode(LogicalPlan.GetNextNodeId(), type, op, leftOperand, rightOperand);
                _logicalPlan.SelectionNodes.Add(selectionNode);
            }
        }
    }
}
