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

        private Atoms.Tuple EvaluateTupleElements(Atoms.Tuple tuple, PrefixOperator op)
        {
            if (tuple.Elements.Length == 0) return tuple;
            Prefix[] negativeElementsExpressions = tuple.BaseExpression.Elements.Select(e => new Prefix(op, e, e.Span)).ToArray();
            Atomic[] negativeAtoms = negativeElementsExpressions.Select(n => n.Evaluate()).ToArray();
            tuple.Elements = negativeAtoms;
            return tuple;
        }
        
        private EvaluateErrorException OperationTypeError(Atomic val, PrefixOperator op, params System.Type[] expected)
        {
            return new EvaluateErrorException(ErrorHandler.EvaluateErrorTypeMismatch(_rhs.Span.Start, op.ToString(), val, expected));
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
                    if (value is Atoms.Tuple @tuple) return EvaluateTupleElements(@tuple, _operator);
                    throw OperationTypeError(value, _operator, typeof(Integer), typeof(Float));
                }
                case PrefixOperator.Not:
                {
                    if (value is Atoms.Boolean @bool) return new Atoms.Boolean(!@bool.Value);
                    if (value is Atoms.Tuple @tuple) return EvaluateTupleElements(@tuple, _operator);
                    throw OperationTypeError(value, _operator, typeof(Atoms.Boolean));
                }
                case PrefixOperator.Referenced:
                {
                    // Find reference in scope
                    if (value is Atoms.Identifier @ident) return new Reference(@ident);
                    if (value is Atoms.IdentifierDotList @identDotList) return new Reference(@identDotList);
                    throw OperationTypeError(value, _operator, typeof(Atoms.Identifier));
                }
                default: throw new EvaluateErrorException(ErrorHandler.EvaluateError(Span.Start, $"Could not evaluate {_operator}. Invalid prefix operator!"));
            }
        }

        public override string ToString(string indent) => $"{_operator}({_rhs.ToString(indent)})";
    }
}
