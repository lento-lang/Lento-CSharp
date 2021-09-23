using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;
using LentoCore.Atoms.Types;
using LentoCore.Evaluator;
using LentoCore.Util;

namespace LentoCore.Expressions
{
    public abstract class Expression
    {
        public readonly LineColumnSpan Span;

        protected Expression(LineColumnSpan span)
        {
            Span = span;
        }

        public abstract Atomic Evaluate(Scope scope);
        public abstract AtomicType GetReturnType(TypeTable types);
        public override string ToString() => ToString("");
        public abstract string ToString(string indent);
    }
}
