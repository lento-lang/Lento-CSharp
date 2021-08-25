using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;
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

        public abstract Atomic Evaluate(); // Add Scope as parameter
        public override string ToString() => ToString("");
        public abstract string ToString(string indent);
    }
}
