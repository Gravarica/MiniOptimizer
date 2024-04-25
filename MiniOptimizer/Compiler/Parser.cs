using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using MiniOptimizer;
using MiniOptimizer.Compiler;
using MiniOptimizer.Exceptions;
using MiniOptimizer.LogicPlan;
using MiniOptimizer.Metadata;
using System.Collections.Generic;

public class Parser
{
    private Catalog _catalog;
    public LogicalPlan _logicalPlan;
    public bool SemanticCheck { get; set; }

    public Parser(Catalog catalog, bool semanticCheck = true)
    {
        _catalog=catalog;
        SemanticCheck = semanticCheck;
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

        LogicalProjectionNode projectionNode = (LogicalProjectionNode) _logicalPlan.CreateProjectionNode(projectedAttributes);
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
            string[] parts = attribute.Split('.');

            if (SemanticCheck && parts.Length != 2) 
                throw new BaseException("Column identifiers must be specified as <table>.<column>");
            if (!_catalog.CheckIfColumnExists(parts[0], parts[1]) && SemanticCheck)
                throw new ColumnNotFoundException(parts[1]);

            attributes.Add(attribute);
        }
        return attributes;
    }

    private void VisitRelationList(MiniQLParser.RelationListContext context)
    {
        
        foreach (MiniQLParser.RelationContext relationContext in context.relation())
        {
            string tableName = relationContext.GetText();

            if (!_catalog.CheckIfTableExists(tableName) && SemanticCheck)
                throw new TableNotFoundException(tableName);

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
            string leftOperand = context.attribute(0).GetText();
            string rightOperand = context.attribute(1).GetText();
            Op op = new Op(Predicate.EQ);

            LogicalSelectionNode selectionNode = new LogicalSelectionNode(LogicalPlan.GetNextNodeId(), op, leftOperand, rightOperand);
            _logicalPlan.SelectionNodes.Add(selectionNode);
        }
    }
}