using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using MiniOptimizer;
using MiniOptimizer.Compiler;
using MiniOptimizer.Exceptions;
using MiniOptimizer.LogicPlan;
using MiniOptimizer.Metadata;
using System.Collections.Generic;

namespace MiniOptimizer.Compiler
{
    public class SQLParser : IAntlrErrorListener<IToken>
    {
        private Catalog _catalog;
        public LogicalPlan _logicalPlan;
        public SemanticAnalyzer? SemanticAnalyzer;
        private bool analyze = true;

        public SQLParser(Catalog catalog)
        {
            _catalog=catalog;
        }

        public void TurnOffSemanticAnalysis()
        {
            analyze = false;
        }

        public void TurnOnSemanticAnalysis()
        {
            analyze = true;
        }

        public LogicalPlan Parse(string input)
        {
            AntlrInputStream inputStream = new AntlrInputStream(input);
            MiniQLLexer lexer = new MiniQLLexer(inputStream);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            MiniQLParser parser = new MiniQLParser(tokens);
            parser.AddErrorListener(this);

            parser.AddErrorListener(new BaseErrorListener());

            var queryContext = parser.query();

            _logicalPlan = new LogicalPlan();

            VisitQuery(queryContext);

            return _logicalPlan;
        }

        private void VisitQuery(MiniQLParser.QueryContext context)
        {
            // Svaki put kada se novi upit parsira, kreira se novi semanticki analyzer
            SemanticAnalyzer = new SemanticAnalyzer(_catalog, analyze);
            List<string> projectedAttributes = VisitAttributeList(context.attributeList());
            VisitRelationList(context.relationList());

            LogicalProjectionNode projectionNode = (LogicalProjectionNode)_logicalPlan.CreateProjectionNode(projectedAttributes);
            _logicalPlan.ProjectionNodes.Add(projectionNode);

            if (context.condition() != null)
            {
                VisitCondition(context.condition());
            }

            SemanticAnalyzer = null;
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

        public void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            throw new BaseException($"Error in parser at line {line}: {msg}");
        }
    }
}
