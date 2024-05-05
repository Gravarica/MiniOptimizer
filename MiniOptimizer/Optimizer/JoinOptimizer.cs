using MiniOptimizer.LogicPlan;
using MiniOptimizer.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Optimizer
{
    public class JoinOptimizer
    {
        private CostModel _costModel;

        public JoinOptimizer(CostModel costModel)
        {
            _costModel = costModel;
        }

        public void ComputeOptimalJoinOrder(LogicalPlan logicalPlan)
        {

        }
    }
}
