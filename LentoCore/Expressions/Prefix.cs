using LentoCore.Atoms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Exception;
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
            Atomic value = _rhs.Evaluate();
            switch (_operator)
            {
                case PrefixOperator.Negative:
                {
                    if (value is Integer @integer) return new Atoms.Integer(@integer.Value * -1);
                    if (value is Float @float) return new Atoms.Float(@float.Value * -1);
                    throw new EvaluateErrorException(ErrorHandler.EvaluateErrorTypeMismatch(_rhs.Span.Start, value, typeof(Integer), typeof(Float)));
                }
                case PrefixOperator.Not:
                {
                    if (value is Atoms.Boolean @bool) return new Atoms.Boolean(!@bool.Value);
                    throw new EvaluateErrorException(ErrorHandler.EvaluateErrorTypeMismatch(_rhs.Span.Start, value, typeof(Atoms.Boolean)));
                }
                default: throw new EvaluateErrorException(ErrorHandler.EvaluateError(Span.Start, $"Could not evaluate {_operator}. Invalid prefix operator!"));
            }
        }

        public override string ToString() => $"{_operator}({_rhs.ToString()})";
    }
}
