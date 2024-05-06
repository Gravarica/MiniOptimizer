using MiniOptimizer.LogicPlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Optimizer
{
    public class DPTableEntry
    {
        public long Cost { get; set; }
        public long Size { get; set; }
        public LogicalNode JoinTree { get; set; }

        public DPTableEntry() { }

        public DPTableEntry(long cost, long size)
        {
            Cost = cost;
            Size = size;
        }

        public DPTableEntry(long cost, long size, LogicalNode joinTree) : this(cost, size)
        {
            JoinTree=joinTree;
        }
    }
}
