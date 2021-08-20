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
    class Binary : Expression
    {
        private readonly BinaryOperator _operator;
        private readonly Expression lhs;
        private readonly Expression rhs;

        public Binary(BinaryOperator @operator, Expression lhs, Expression rhs, LineColumnSpan span) : base(span)
        {
            _operator = @operator;
            this.lhs = lhs;
            this.rhs = rhs;
        }

        public override Atomic Evaluate()
        {
            throw new NotImplementedException();
        }

        public override string ToString() => $"{_operator}({lhs.ToString()}, {rhs.ToString()})";
    }
}
