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
    public class List : Expression
    {
        public List<Expressions.Expression> Elements;
        public int Size => Elements.Count;

        public List(LineColumnSpan span, List<Expressions.Expression> elements) : base(span)
        {
            Elements = elements;
        }

        public override Atomic Evaluate(Scope scope)
        {
            List<Atomic> evaluatedElements = Elements.Select(e => e.Evaluate(scope)).ToList();
            return new Atoms.List(this, evaluatedElements);
        }

        public override AtomicType GetReturnType(TypeTable table) => Atoms.List.BaseType;
        public override string ToString(string indent) => $"List: [\n{indent + Formatting.Indentation}{string.Join($",\n{indent}{Formatting.Indentation}", Elements.Select(e => e.ToString(indent + Formatting.Indentation)))}\n{indent}]";
    }
}
