using MiniOptimizer.Exceptions;
using MiniOptimizer.LogicPlan;
using MiniOptimizer.Metadata;
using MiniOptimizer.PhysicPlan;
using MiniOptimizer.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Optimizer
{
    public class CostBasedOptimizer
    {
        private Catalog _catalog;
        private CostModel _costModel; 

        public CostBasedOptimizer(Catalog catalog, CostModel costModel)
        {
            _catalog=catalog;
            _costModel=costModel;
        }

        public PhysicalPlan Optimize(LogicalPlan logicalPlan)
        {
            PhysicalPlan plan = new PhysicalPlan();
            plan.Root = ConvertToPhysicalNode(logicalPlan.RootNode);
            return plan;

        }

        public PhysicalNode ConvertToPhysicalNode(LogicalNode node)
        {
            PhysicalNode physicalNode = null;

            switch (node)
            {
                case LogicalProjectionNode lproj:
                    physicalNode = SelectPhysicalProjection(lproj);
                    break;
                case LogicalJoinNode ljn:
                    physicalNode = SelectJoinMethod(ljn);
                    break;
                case LogicalProductNode lprod:
                    physicalNode = SelectJoinMethod(lprod);
                    break;
                case LogicalRelationNode lrel:
                    physicalNode = SelectOptimalAccessMethod(lrel.ProjectionNode);
                    break;
                case LogicalSelectionNode:
                    break;
                case LogicalScanNode:
                    break;
                default:
                    throw new BaseException("Invalid node type while converting to Physical Node");
            }

            foreach (LogicalNode child in node.Children)
            {
                if (child is LogicalSelectionNode || child is LogicalScanNode) break;
                PhysicalNode childPhysicalNode = ConvertToPhysicalNode(child);
                physicalNode.AddChild(childPhysicalNode);
            }

            return physicalNode;
        }

        public PhysicalPlan SelectAccessMethods(LogicalPlan logicalPlan)
        {
            // PSEUDOKOD 
            // Pokupi sve JOIN node-ove 
            // Pokupi njihovu decu
            // Analiziraj decu da vidis koje sve mozes pristupne puteve da napravis 
            // Evaluiraj vrednost svakog pristupnog puta
            // Izaberi optimalan
            PhysicalPlan physicalPlan = new PhysicalPlan();

            Func<LogicalNode, bool> isJoinNode = node =>
                                    node is LogicalJoinNode;

            List<LogicalNode> joinNodes = logicalPlan.FindAll(isJoinNode);

            List<LogicalNode> accessNodes = new List<LogicalNode>();

            if (joinNodes.Count == 0)
            {
                PhysicalNode physicalNode = SelectOptimalAccessMethod(logicalPlan.RootNode.Children.First());
                physicalPlan.ScanNodes.Add(physicalNode);
                return physicalPlan;
            }

            foreach (LogicalNode node in joinNodes)
            {
                // Znam da je levo duboko stablo
                accessNodes.Add(node.Children.Last());

                if (node.Children.First() is LogicalJoinNode) continue;

                accessNodes.Add(node.Children.First());
            }

            foreach (var node in accessNodes) 
            {
                PhysicalNode physicalNode = SelectOptimalAccessMethod(node);
                physicalPlan.ScanNodes.Add(physicalNode);
            }

            return physicalPlan;
        }

        private PhysicalNode SelectOptimalAccessMethod(LogicalNode node)
        {
            switch(node)
            {
                case LogicalScanNode scanNode:
                    return new SequentialScan(scanNode.TableName);
                case LogicalSelectionNode selectionNode:
                    return SelectAccessMethodFromSelection(selectionNode);
                case LogicalProjectionNode projectionNode:
                    return SelectAccessMethodFromProjection(projectionNode);
                case LogicalRelationNode relationNode:
                    return SelectOptimalAccessMethod(relationNode.ProjectionNode);
                default:
                    throw new BaseException("Unknown node type.");
            }
        }

        private PhysicalNode SelectAccessMethodFromSelection(LogicalSelectionNode node)
        {
            var qualifiedName = ParseHelper.ParseQualifiedName(node.LeftOperand);
            SequentialScan scanNode;

            // Ako nema index -> seq_scan + filter
            if (!_catalog.HasIndexOnColumn(qualifiedName.Item1, qualifiedName.Item2))
            {
                scanNode = new SequentialScan(qualifiedName.Item1);
                Filter filterNode = new Filter(node.Op, node.LeftOperand, node.RightOperand);
                filterNode.AddChild(scanNode);
                return filterNode;
            }

            // Ako ima index -> Proveriti prvo da li je klasterizovan ili nije
            Metadata.Index index = _catalog.GetIndex(qualifiedName.Item1, qualifiedName.Item2);

            // Ako je index klasterizovan, forsiraj index scan
            if (index.IsClustered)
                return new IndexScan(qualifiedName.Item1, index.Name, node.Op);

            // Ako nije, moramo uraditi estimaciju cene, jer moze da se desi da index scan bude skuplji od seq_scan
            // Mala selektivnost + neklasterizovan index = veliki broj nasumicnih pristupa disku
            scanNode = new SequentialScan(qualifiedName.Item1);
            IndexScan indexNode = new IndexScan(qualifiedName.Item1, index.Name, node.Op);

            var scanCost = _costModel.EstimateCost(scanNode);
            var indexScanCost = _costModel.EstimateCost(indexNode);

            if (indexScanCost > scanCost)
            {
                Filter filterNode = new Filter(node.Op, node.LeftOperand, node.RightOperand);
                filterNode.AddChild(scanNode);
                return filterNode;
            }

            return indexNode;

        }

        private PhysicalNode SelectAccessMethodFromProjection(LogicalProjectionNode projNode)
        {
            var table = projNode.GetTableName();
            // Ako ima compound index nad projektovanim poljima, nema potrebe za pristupom tabeli 
            // Citaj samo iz indexa -> IndexSeek
            if (_catalog.HasCompoundIndex(table, projNode.Attributes))
            {
                Metadata.Index index = _catalog.GetCompoundIndex(table, projNode.Attributes);

                if (!(projNode.Children.First() is LogicalSelectionNode selNode))
                    return new IndexSeek(table, index.Name);

                if (projNode.Attributes.Contains(selNode.GetColumnName(0))) 
                    return new IndexSeek(table, index.Name);
            }

            PhysicalProjection projectionNode;
            // Ako je u logickom planu projekcija pa scan -> seq_scan pa fizicka projekcija
            if (projNode.Children.First() is LogicalScanNode)
            {
                SequentialScan scanNode = new SequentialScan(table);
                projectionNode = new PhysicalProjection(projNode.Attributes);
                projectionNode.AddChild(scanNode);
                return projectionNode;
            }

            // Ako je u logickom planu posle projekcije selekcija (Proj -> Sel -> Scan)
            // Vodimo se istom logikom kao za selekciju pa samo dodamo fizicku projekciju
            PhysicalNode accessNode = SelectAccessMethodFromSelection(projNode.Children.First() as LogicalSelectionNode);
            projectionNode = new PhysicalProjection(projNode.Attributes);
            projectionNode.AddChild(accessNode);

            return projectionNode;
        }

        private PhysicalNode SelectJoinMethod(LogicalNode node)
        {
            if (node is LogicalProductNode product)
            {
                return new CrossProduct(product.GetTableName(0), product.GetTableName(1));
            }

            // Za pocetak forsiraj samo Nested Loop Join

            if (!(node is LogicalJoinNode joinNode)) throw new BaseException("Not a join node");

            return new NestedLoopJoin(joinNode);
        }

        private PhysicalNode SelectPhysicalProjection(LogicalNode node)
        {
            if (!(node is LogicalProjectionNode lpn)) throw new BaseException("Root note should be projection");

            if (lpn.Children.First() is LogicalScanNode || lpn.Children.First() is LogicalSelectionNode)
                return SelectOptimalAccessMethod(node);

            return new PhysicalProjection(lpn);
        }
    }
}
