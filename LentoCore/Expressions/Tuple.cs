using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;
using LentoCore.Util;

namespace LentoCore.Expressions
{
    public class Tuple : Expression
    {
        public Expressions.Expression[] Elements;
        public int Size => Elements.Length;

        public Tuple(LineColumnSpan span, Expressions.Expression[] elements) : base(span)
        {
            Elements = elements;
        }

        public override Atomic Evaluate()
        {
            Atomic[] evaluatedElements = Elements.Select(e => e.Evaluate()).ToArray();
            return new Atoms.Tuple(this, evaluatedElements);
        }

        public override string ToString() => $"Tuple: (\n    {string.Join(",\n    ", Elements.Select(e => e.ToString()))}\n)";
    }
}
