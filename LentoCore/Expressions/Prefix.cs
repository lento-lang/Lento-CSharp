using LentoCore.Atoms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Parser;
using LentoCore.Util;

namespace LentoCore.Expressions
{
    class Prefix : Expression
    {
        private readonly PrefixOperator _operator;
        private readonly Expression _rhs;

        public Prefix(PrefixOperator @operator, Expression rhs, LineColumnSpan span) : base(span)
        {
            _operator = @operator;
            _rhs = rhs;
        }

        public override Atomic Evaluate()
        {
            throw new NotImplementedException();
        }

        public override string ToString() => $"{_operator}({_rhs.ToString()})";
    }
}
