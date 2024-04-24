using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer
{

    public enum Predicate { LT, GT, LE, GE, EQ, NE, LIKE };

    public class Op
    {
        public static Predicate Predicate { get; set; }

        public string ToString()
        {
            switch (Predicate)
            {
                case Predicate.LT:
                    return "<";
                case Predicate.GT:
                    return ">";
                case Predicate.LE:
                    return "<=";
                case Predicate.GE:
                    return ">=";
                case Predicate.EQ:
                    return "==";
                case Predicate.NE:
                    return "!=";
                case Predicate.LIKE:
                    return "LIKE";
                default:
                    throw new ArgumentException("No such operand");
            }
        }

        private Predicate GetOp(string op)
        {
            switch(op)
            {
                case "=":
                    return Predicate.EQ;
                case ">":
                    return Predicate.GT;
                case "<":
                    return Predicate.LT;
                case "!=":
                    return Predicate.NE;
                case ">=":
                    return Predicate.GE;
                case "<=":
                    return Predicate.LE;
                case "LIKE":
                    return Predicate.LIKE;
                default: throw new ArgumentException();
            }
        }

        public static void PrintOp()
        {
            Console.WriteLine("Operation is: ", Predicate.ToString());
        }

        public Op (string predicate)
        {
            Predicate = GetOp(predicate);
        }

        public Op (Predicate predicate) 
        {
            Predicate = predicate;
        }
    }
}
