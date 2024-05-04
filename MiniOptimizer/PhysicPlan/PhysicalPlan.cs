using MiniOptimizer.LogicPlan;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.PhysicPlan
{
    public class PhysicalPlan
    {
        public PhysicalNode Root { get; set; }

        public List<PhysicalNode> ScanNodes { get; set; } = new List<PhysicalNode>();

        public PhysicalPlan() { }

        public PhysicalPlan(PhysicalNode root)
        {
            Root = root;
        }

        public void SelectAccessMethods(LogicalPlan logicalPlan)
        {
            // PSEUDOKOD 
            // Pokupi sve JOIN node-ove 
            // Pokupi njihovu decu
            // Analiziraj decu da vidis koje sve mozes pristupne puteve da napravis 
            // Evaluiraj vrednost svakog pristupnog puta
            // Izaberi optimalan
        }

        public void Print()
        {
            foreach (PhysicalNode scanNode in ScanNodes)
            {
                scanNode.Print();
            }
        }
    }
}
