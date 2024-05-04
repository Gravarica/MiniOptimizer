using MiniOptimizer.Metadata;
using MiniOptimizer.Test;
using MiniOptimizer.Compiler;
using System;
using System.Runtime.InteropServices;
using MiniOptimizer.Optimizer;
using MiniOptimizer.LogicPlan;

namespace MiniOptimizer
{
    class Program
    {
        static void Main(string[] args)
        {
            TestMethods.TestRuleBasedOptimizer();
            //TestMethods.TestCardinalityEstimation();
        }
    }
}
