using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Parser
{
    public enum BinaryOperator
    {
        Add, Subtract, Multiply, Divide, Modulus,

        Equals, NotEquals, LessThan, GreaterThan, LessThanEquals, GreaterThanEquals,

        And, Or, Exclude
    }
    public class InfixBindingPower
    {
        public int Left;
        public int Right;

        public InfixBindingPower(int left, int right)
        {
            Left = left;
            Right = right;
        }
    }
}
