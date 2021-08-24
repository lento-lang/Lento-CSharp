using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class Tuple : Atomic
    {
        public Expressions.Tuple BaseExpression;
        public Atomic[] Elements;
        public int Size => Elements.Length;
        
        public Tuple(Atomic[] elements)
        {
            Elements = elements;
        }
        public Tuple(Expressions.Tuple baseExpression, Atomic[] elements)
        {
            BaseExpression = baseExpression;
            Elements = elements;
        }

        public override string ToString() => $"({string.Join(", ", Elements.Select(e => e.ToString()))})";
    }
}
