using MiniOptimizer.LogicPlan;
using MiniOptimizer.Metadata;
using MiniOptimizer.PhysicPlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Optimizer
{
    public class QueryOptimizer
    {
        private Catalog _catalog;
        private CostModel _costModel;
        private RuleBasedOptimizer rbo;
        private CostBasedOptimizer cbo;
        private JoinOptimizer jo;

        public QueryOptimizer(Catalog catalog) 
        {
            _catalog = catalog;
            _costModel = new CostModel(catalog);
            rbo = new RuleBasedOptimizer(catalog);
            jo = new JoinOptimizer(_costModel);
            cbo = new CostBasedOptimizer(catalog, _costModel);
        }

        public PhysicalPlan Optimize(LogicalPlan logicalPlan)
        {
            rbo.Optimize(logicalPlan);
            LogicalNode optimizedTree = jo.OptimizeJoin(logicalPlan);
            logicalPlan.ChangeSubtree(logicalPlan.RootNode.Children.First(), optimizedTree);
            return cbo.Optimize(logicalPlan);
        }
    }
}
