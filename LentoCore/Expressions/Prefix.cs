using LentoCore.Atoms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Evaluator;
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

        private Atoms.Tuple EvaluateTupleElements(Atoms.Tuple tuple, PrefixOperator op, Scope scope)
        {
            if (tuple.Elements.Length == 0) return tuple;
            Prefix[] negativeElementsExpressions = tuple.BaseExpression.Elements.Select(e => new Prefix(op, e, e.Span)).ToArray();
            Atomic[] negativeAtoms = negativeElementsExpressions.Select(n => n.Evaluate(scope)).ToArray();
            tuple.Elements = negativeAtoms;
            return tuple;
        }
        
        private RuntimeErrorException OperationTypeError(Atomic val, PrefixOperator op, params System.Type[] expected)
        {
            return new RuntimeErrorException(ErrorHandler.EvaluateErrorTypeMismatch(_rhs.Span.Start, op.FastToString(), val, expected));
        }
        public override Atomic Evaluate(Scope scope)
        {
            Atomic value = _rhs.Evaluate(scope);
            switch (_operator)
            {
                case PrefixOperator.Negative:
                {
                    if (value is Integer @integer) return new Atoms.Integer(@integer.Value * -1);
                    if (value is Float @float) return new Atoms.Float(@float.Value * -1);
                    if (value is Atoms.Tuple @tuple) return EvaluateTupleElements(@tuple, _operator, scope);
                    throw OperationTypeError(value, _operator, typeof(Integer), typeof(Float));
                }
                case PrefixOperator.Not:
                {
                    if (value is Atoms.Boolean @bool) return new Atoms.Boolean(!@bool.Value);
                    if (value is Atoms.Tuple @tuple) return EvaluateTupleElements(@tuple, _operator, scope);
                    throw OperationTypeError(value, _operator, typeof(Atoms.Boolean));
                }
                case PrefixOperator.Referenced:
                {
                    // Find reference in scope when evaluating function call
                    if (_rhs is Expressions.AtomicValue<Atoms.Identifier> identifier) return new Reference(identifier.GetAtomicValue());
                    if (_rhs is Expressions.AtomicValue<Atoms.IdentifierDotList> identifierDotList) return new Reference(identifierDotList.GetAtomicValue());
                    throw OperationTypeError(value, _operator, typeof(Atoms.Identifier), typeof(Atoms.IdentifierDotList));
                }
                default: throw new RuntimeErrorException(ErrorHandler.EvaluateError(Span.Start, $"Could not evaluate {_operator.FastToString()}. Invalid prefix operator!"));
            }
        }

        public override AtomicType GetReturnType(TypeTable table)
        {
            Atomic value = _rhs.GetReturnType(table);
            switch (_operator)
            {
                case PrefixOperator.Negative:
                    {
                        if (value.Equals(Integer.BaseType)) return Integer.BaseType;
                        if (value.Equals(Float.BaseType)) return Float.BaseType;
                        if (value.Equals(Atoms.Tuple.BaseType)) return Atoms.Tuple.BaseType;
                        throw OperationTypeError(value, _operator, typeof(Integer), typeof(Float));
                    }
                case PrefixOperator.Not:
                    {
                        if (value.Equals(Atoms.Boolean.BaseType)) return Atoms.Boolean.BaseType;
                        if (value.Equals(Atoms.Tuple.BaseType)) return Atoms.Tuple.BaseType;
                        throw OperationTypeError(value, _operator, typeof(Atoms.Boolean));
                    }
                case PrefixOperator.Referenced:
                    {
                        if (value.Equals(Identifier.BaseType)) return Reference.BaseType;
                        if (value.Equals(IdentifierDotList.BaseType)) return Reference.BaseType;
                        throw OperationTypeError(value, _operator, typeof(Atoms.Identifier), typeof(Atoms.IdentifierDotList));
                    }
                default: throw new RuntimeErrorException(ErrorHandler.EvaluateError(Span.Start, $"Could not evaluate {_operator.FastToString()}. Invalid prefix operator!"));
            }
        }
        public override string ToString(string indent) => $"{_operator.FastToString()}({_rhs.ToString(indent)})";
    }
}
