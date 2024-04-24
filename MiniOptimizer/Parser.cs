using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using MiniOptimizer;
using System.Collections.Generic;

public class Parser
{
    public LogicalPlan Parse(string input)
    {
        AntlrInputStream inputStream = new AntlrInputStream(input);
        MiniQLLexer lexer = new MiniQLLexer(inputStream);
        CommonTokenStream tokens = new CommonTokenStream(lexer);
        MiniQLParser parser = new MiniQLParser(tokens);

        var queryContext = parser.query();

        LogicalPlan logicalPlan = new LogicalPlan();

        logicalPlan.SetRootNode(VisitQuery(queryContext));

        return logicalPlan;
    }

    private LogicalNode VisitQuery(MiniQLParser.QueryContext context)
    {
        List<string> projectedAttributes = VisitAttributeList(context.attributeList());
        List<LogicalNode> scanNodes = VisitRelationList(context.relationList());

        LogicalNode productNode = CreateProductNode(scanNodes);

        if (context.condition() != null)
        {
            Console.WriteLine("Condition context nije null");
            LogicalNode selectionNode = VisitCondition(context.condition(), productNode);
            LogicalNode projectionNode = CreateProjectionNode(projectedAttributes);
            projectionNode.AddChild(selectionNode);
            return projectionNode;
        }
        else
        {
            LogicalNode projectionNode = CreateProjectionNode(projectedAttributes);
            projectionNode.AddChild(productNode);
            return projectionNode;
        }
    }

    private List<string> VisitAttributeList(MiniQLParser.AttributeListContext context)
    {
        List<string> attributes = new List<string>();
        foreach (MiniQLParser.AttributeContext attributeContext in context.attribute())
        {
            attributes.Add(attributeContext.GetText());
        }
        return attributes;
    }

    private List<LogicalNode> VisitRelationList(MiniQLParser.RelationListContext context)
    {
        List<LogicalNode> scanNodes = new List<LogicalNode>();
        foreach (MiniQLParser.RelationContext relationContext in context.relation())
        {
            string tableName = relationContext.GetText();
            int tableId = 1;
            scanNodes.Add(new LogicalScanNode(LogicalPlan.GetNextNodeId(), tableName, tableId));
        }
        return scanNodes;
    }

    private LogicalNode VisitCondition(MiniQLParser.ConditionContext context, LogicalNode inputNode)
    {
        string leftOperand = context.attribute(0).GetText();
        string rightOperand = context.attribute(1).GetText();
        Op op = new Op(Predicate.EQ); // Assuming equality condition

        LogicalNode selectionNode = new LogicalSelectionNode(LogicalPlan.GetNextNodeId(), op, leftOperand, rightOperand);
        selectionNode.AddChild(inputNode);
        return selectionNode;
    }

    private LogicalNode CreateProductNode(List<LogicalNode> scanNodes)
    {
        LogicalNode productNode = new LogicalProductNode(LogicalPlan.GetNextNodeId());
        foreach (LogicalNode scanNode in scanNodes)
        {
            productNode.AddChild(scanNode);
        }
        return productNode;
    }

    private LogicalNode CreateProjectionNode(List<string> projectedAttributes)
    {
        LogicalProjectionNode projectionNode = new LogicalProjectionNode(LogicalPlan.GetNextNodeId());
        foreach (string attribute in projectedAttributes)
        {
            projectionNode.AddAttribute(attribute);
        }
        return projectionNode;
    }
}