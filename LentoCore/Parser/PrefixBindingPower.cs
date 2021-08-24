using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Parser
{
    public enum PrefixOperator
    {
        Not, Negative, Referenced
    }
    public class PrefixBindingPower
    {
        public int Right;

        public PrefixBindingPower(int right)
        {
            Right = right;
        }
    }
}
