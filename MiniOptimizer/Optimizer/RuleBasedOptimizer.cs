using MiniOptimizer.LogicPlan;
using MiniOptimizer.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Optimizer
{
    public class RuleBasedOptimizer
    {
        public LogicalPlan LogicalPlan { get; set; }

        private Catalog _catalog;

        public RuleBasedOptimizer(LogicalPlan logicalPlan, Catalog catalog)
        {
            LogicalPlan = logicalPlan;
            _catalog = catalog;
        }

        public LogicalPlan PushDownSelections()
        {
            return LogicalPlan;
        }
        
    }
}
