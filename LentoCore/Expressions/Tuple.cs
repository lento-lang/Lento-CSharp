using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;
using LentoCore.Evaluator;
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

        public override Atomic Evaluate(Scope scope)
        {
            Atomic[] evaluatedElements = Elements.Select(e => e.Evaluate(scope)).ToArray();
            return new Atoms.Tuple(this, evaluatedElements);
        }

        public override string ToString(string indent) => $"Tuple: (\n{indent + Formatting.Indentation}{string.Join($",\n{indent}{Formatting.Indentation}", Elements.Select(e => e.ToString(indent + Formatting.Indentation)))}\n{indent})";
    }
}
