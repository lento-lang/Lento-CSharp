﻿using LentoCore.Atoms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Exception;
using LentoCore.Parser;
using LentoCore.Util;
using Boolean = LentoCore.Atoms.Boolean;
using String = LentoCore.Atoms.String;

namespace LentoCore.Expressions
{
    class Binary : Expression
    {
        private readonly BinaryOperator _operator;
        private readonly Expression _lhs;
        private readonly Expression _rhs;

        public Binary(BinaryOperator @operator, Expression lhs, Expression rhs, LineColumnSpan span) : base(span)
        {
            _operator = @operator;
            _lhs = lhs;
            _rhs = rhs;
        }
        private bool Operation<TAtomic, TPrimitive>(Atomic lhs, Atomic rhs, Func<TAtomic, TAtomic, TPrimitive> op,
            out TPrimitive result) => CrossOperation(lhs, rhs, op, out result);
        private bool CrossOperation<TAtomicLeft, TAtomicRight, TPrimitive>(Atomic lhs, Atomic rhs, Func<TAtomicLeft, TAtomicRight, TPrimitive> op, out TPrimitive result)
        {
            if (lhs is TAtomicLeft lhsAtom)
            {
                if (rhs is TAtomicRight rhsAtom)
                {
                    result = op(lhsAtom, rhsAtom);
                    return true;
                }
                // throw new EvaluateErrorException(ErrorHandler.EvaluateErrorTypeMismatch(_rhs.Span.Start, rhs, typeof(TAtomicRight)));
            }

            result = default;
            return false;
        }

        private bool VoidOperation<TAtomic>(Atomic lhs, Atomic rhs, Action<TAtomic, TAtomic> op) => CrossOperation<TAtomic, TAtomic, bool>(lhs, rhs, (l, r) => {
                op(l, r);
                return true;
            }, out bool _);
        private bool TupleCrossOperation<TAtomic>(Atomic lhs, Atomic rhs, BinaryOperator op,
            out Atoms.Tuple result)
        {
            if (lhs is TAtomic && rhs is Atoms.Tuple @tupleRhs)
            {
                result = @tupleRhs;
                result.Elements = @tupleRhs.BaseExpression.Elements.Select(e => new Binary(_operator, _lhs, e, e.Span).Evaluate()).ToArray();
                return true;
            }
            if (rhs is TAtomic && lhs is Atoms.Tuple @tupleLhs)
            {
                result = @tupleLhs;
                result.Elements = @tupleLhs.BaseExpression.Elements.Select(e => new Binary(_operator, _rhs, e, e.Span).Evaluate()).ToArray();
                return true;
            }

            result = default;
            return false;
        }

        private bool TupleOperation(Atomic lhs, Atomic rhs, BinaryOperator op,
            out Atoms.Tuple result)
        {
            if (lhs is Atoms.Tuple @tupleLhs && rhs is Atoms.Tuple @tupleRhs)
            {
                if (!@tupleLhs.Size.Equals(@tupleRhs.Size)) throw new EvaluateErrorException(ErrorHandler.EvaluateErrorTypeMismatch(_rhs.Span.Start, rhs, @tupleLhs.GetTypeName()));
                result = @tupleLhs;
                for (int i = 0; i < result.Size; i++)
                {
                    Expression left = @tupleLhs.BaseExpression.Elements[i];
                    Expression right = @tupleRhs.BaseExpression.Elements[i];
                    result.Elements[i] = new Binary(op, left, right, new LineColumnSpan(left.Span.Start, right.Span.End)).Evaluate();
                }
                return true;
            }

            result = default;
            return false;
        }

        private EvaluateErrorException OperationTypeError(Atomic lhs, BinaryOperator op, params Type[] expected)
        {
            return new EvaluateErrorException(ErrorHandler.EvaluateErrorTypeMismatch(_lhs.Span.Start, op.ToString(), lhs, expected));
        }

        public override Atomic Evaluate()
        {
            Atomic lhs = _lhs.Evaluate();
            Atomic rhs = _rhs.Evaluate();
            switch (_operator)
            {
                case BinaryOperator.Add:
                {
                    if (Operation<Atoms.Integer, int>(lhs, rhs, (l, r) => l.Value + r.Value, out int resultInt)) return new Atoms.Integer(resultInt);
                    if (Operation<Atoms.Float, float>(lhs, rhs, (l, r) => l.Value + r.Value, out float resultFloat)) return new Atoms.Float(resultFloat);
                    if (CrossOperation<Atoms.Integer, Atoms.Float, float>(lhs, rhs, (l, r) => l.Value + r.Value, out float resultIntFloat)) return new Atoms.Float(resultIntFloat);
                    if (CrossOperation<Atoms.Float, Atoms.Integer, float>(lhs, rhs, (l, r) => l.Value + r.Value, out float resultFloatInt)) return new Atoms.Float(resultFloatInt);
                    if (TupleOperation(lhs, rhs, _operator, out Atoms.Tuple resultTuple)) return resultTuple;
                    if (VoidOperation<Atoms.List>(lhs, rhs, (l, r) => l.Elements.AddRange(r.Elements))) return lhs;
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float));
                }
                case BinaryOperator.Subtract:
                {
                    if (Operation<Atoms.Integer, int>(lhs, rhs, (l, r) => l.Value - r.Value, out int resultInt)) return new Atoms.Integer(resultInt);
                    if (Operation<Atoms.Float, float>(lhs, rhs, (l, r) => l.Value - r.Value, out float resultFloat)) return new Atoms.Float(resultFloat);
                    if (CrossOperation<Atoms.Integer, Atoms.Float, float>(lhs, rhs, (l, r) => l.Value - r.Value, out float resultIntFloat)) return new Atoms.Float(resultIntFloat);
                    if (CrossOperation<Atoms.Float, Atoms.Integer, float>(lhs, rhs, (l, r) => l.Value - r.Value, out float resultFloatInt)) return new Atoms.Float(resultFloatInt);
                    if (TupleOperation(lhs, rhs, _operator, out Atoms.Tuple resultTuple)) return resultTuple;
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float));
                }
                case BinaryOperator.Multiply:
                {
                    if (Operation<Atoms.Integer, int>(lhs, rhs, (l, r) => l.Value * r.Value, out int resultInt)) return new Atoms.Integer(resultInt);
                    if (Operation<Atoms.Float, float>(lhs, rhs, (l, r) => l.Value * r.Value, out float resultFloat)) return new Atoms.Float(resultFloat);
                    if (CrossOperation<Atoms.Integer, Atoms.Float, float>(lhs, rhs, (l, r) => l.Value * r.Value, out float resultIntFloat)) return new Atoms.Float(resultIntFloat);
                    if (CrossOperation<Atoms.Float, Atoms.Integer, float>(lhs, rhs, (l, r) => l.Value * r.Value, out float resultFloatInt)) return new Atoms.Float(resultFloatInt);
                    if (TupleCrossOperation<Atoms.Integer>(lhs, rhs, _operator, out Atoms.Tuple resultTuple)) return resultTuple;
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float));
                }
                case BinaryOperator.Divide:
                {
                    if (Operation<Atoms.Integer, int>(lhs, rhs, (l, r) => l.Value / r.Value, out int resultInt)) return new Atoms.Integer(resultInt);
                    if (Operation<Atoms.Float, float>(lhs, rhs, (l, r) => l.Value / r.Value, out float resultFloat)) return new Atoms.Float(resultFloat);
                    if (CrossOperation<Atoms.Integer, Atoms.Float, float>(lhs, rhs, (l, r) => l.Value / r.Value, out float resultIntFloat)) return new Atoms.Float(resultIntFloat);
                    if (CrossOperation<Atoms.Float, Atoms.Integer, float>(lhs, rhs, (l, r) => l.Value / r.Value, out float resultFloatInt)) return new Atoms.Float(resultFloatInt);
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float));
                }
                case BinaryOperator.Modulus:
                {
                    if (Operation<Atoms.Integer, int>(lhs, rhs, (l, r) => l.Value % r.Value, out int resultInt)) return new Atoms.Integer(resultInt);
                    if (Operation<Atoms.Float, float>(lhs, rhs, (l, r) => l.Value % r.Value, out float resultFloat)) return new Atoms.Float(resultFloat);
                    if (CrossOperation<Atoms.Integer, Atoms.Float, float>(lhs, rhs, (l, r) => l.Value % r.Value, out float resultIntFloat)) return new Atoms.Float(resultIntFloat);
                    if (CrossOperation<Atoms.Float, Atoms.Integer, float>(lhs, rhs, (l, r) => l.Value % r.Value, out float resultFloatInt)) return new Atoms.Float(resultFloatInt);
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float));
                }
                case BinaryOperator.Equals:
                {
                    if (Operation<Atoms.Integer, bool>(lhs, rhs, (l, r) => l.Value == r.Value, out bool resultInt)) return new Atoms.Boolean(resultInt);
                    if (Operation<Atoms.Float, bool>(lhs, rhs, (l, r) => Math.Abs(l.Value - r.Value) < float.Epsilon, out bool resultFloat)) return new Atoms.Boolean(resultFloat);
                    if (CrossOperation<Atoms.Integer, Atoms.Float, bool>(lhs, rhs, (l, r) => Math.Abs(l.Value - r.Value) < float.Epsilon, out bool resultIntFloat)) return new Atoms.Boolean(resultIntFloat);
                    if (CrossOperation<Atoms.Float, Atoms.Integer, bool>(lhs, rhs, (l, r) => Math.Abs(l.Value - r.Value) < float.Epsilon, out bool resultFloatInt)) return new Atoms.Boolean(resultFloatInt);
                    if (Operation<Atoms.Boolean, bool>(lhs, rhs, (l, r) => l.Value == r.Value, out bool resultBool)) return new Atoms.Boolean(resultBool);
                    if (Operation<Atoms.Atom, bool>(lhs, rhs, (l, r) => l.Name == r.Name, out bool resultAtom)) return new Atoms.Boolean(resultAtom);
                    if (Operation<Atoms.Character, bool>(lhs, rhs, (l, r) => l.Value == r.Value, out bool resultChar)) return new Atoms.Boolean(resultChar);
                    if (Operation<Atoms.String, bool>(lhs, rhs, (l, r) => l.Value == r.Value, out bool resultString)) return new Atoms.Boolean(resultString);
                    if (Operation<Atoms.Unit, bool>(lhs, rhs, (l, r) => true, out bool _)) return new Atoms.Boolean(true);
                    if (TupleOperation(lhs, rhs, _operator, out Atoms.Tuple resultTuple)) return resultTuple;
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Boolean), typeof(Atom), typeof(Character), typeof(String), typeof(Unit), typeof(Atoms.Tuple));
                }
                case BinaryOperator.NotEquals:
                {
                    return new Atoms.Boolean(!((Atoms.Boolean)new Binary(BinaryOperator.Equals, _lhs, _rhs, Span).Evaluate()).Value);
                }
                case BinaryOperator.LessThan:
                {
                    if (Operation<Atoms.Integer, bool>(lhs, rhs, (l, r) => l.Value < r.Value, out bool resultInt)) return new Atoms.Boolean(resultInt);
                    if (Operation<Atoms.Float, bool>(lhs, rhs, (l, r) => l.Value < r.Value, out bool resultFloat)) return new Atoms.Boolean(resultFloat);
                    if (CrossOperation<Atoms.Integer, Atoms.Float, bool>(lhs, rhs, (l, r) => l.Value < r.Value, out bool resultIntFloat)) return new Atoms.Boolean(resultIntFloat);
                    if (CrossOperation<Atoms.Float, Atoms.Integer, bool>(lhs, rhs, (l, r) => l.Value < r.Value, out bool resultFloatInt)) return new Atoms.Boolean(resultFloatInt);
                    if (Operation<Atoms.Character, bool>(lhs, rhs, (l, r) => l.Value < r.Value, out bool resultChar)) return new Atoms.Boolean(resultChar);
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Character));
                }
                case BinaryOperator.LessThanEquals:
                {
                    if (Operation<Atoms.Integer, bool>(lhs, rhs, (l, r) => l.Value <= r.Value, out bool resultInt)) return new Atoms.Boolean(resultInt);
                    if (Operation<Atoms.Float, bool>(lhs, rhs, (l, r) => l.Value <= r.Value, out bool resultFloat)) return new Atoms.Boolean(resultFloat);
                    if (CrossOperation<Atoms.Integer, Atoms.Float, bool>(lhs, rhs, (l, r) => l.Value <= r.Value, out bool resultIntFloat)) return new Atoms.Boolean(resultIntFloat);
                    if (CrossOperation<Atoms.Float, Atoms.Integer, bool>(lhs, rhs, (l, r) => l.Value <= r.Value, out bool resultFloatInt)) return new Atoms.Boolean(resultFloatInt);
                    if (Operation<Atoms.Character, bool>(lhs, rhs, (l, r) => l.Value <= r.Value, out bool resultChar)) return new Atoms.Boolean(resultChar);
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Character));
                }
                case BinaryOperator.GreaterThan:
                {
                    if (Operation<Atoms.Integer, bool>(lhs, rhs, (l, r) => l.Value > r.Value, out bool resultInt)) return new Atoms.Boolean(resultInt);
                    if (Operation<Atoms.Float, bool>(lhs, rhs, (l, r) => l.Value > r.Value, out bool resultFloat)) return new Atoms.Boolean(resultFloat);
                    if (CrossOperation<Atoms.Integer, Atoms.Float, bool>(lhs, rhs, (l, r) => l.Value > r.Value, out bool resultIntFloat)) return new Atoms.Boolean(resultIntFloat);
                    if (CrossOperation<Atoms.Float, Atoms.Integer, bool>(lhs, rhs, (l, r) => l.Value > r.Value, out bool resultFloatInt)) return new Atoms.Boolean(resultFloatInt);
                    if (Operation<Atoms.Character, bool>(lhs, rhs, (l, r) => l.Value > r.Value, out bool resultChar)) return new Atoms.Boolean(resultChar);
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Character));
                }
                case BinaryOperator.GreaterThanEquals:
                {
                    if (Operation<Atoms.Integer, bool>(lhs, rhs, (l, r) => l.Value >= r.Value, out bool resultInt)) return new Atoms.Boolean(resultInt);
                    if (Operation<Atoms.Float, bool>(lhs, rhs, (l, r) => l.Value >= r.Value, out bool resultFloat)) return new Atoms.Boolean(resultFloat);
                    if (CrossOperation<Atoms.Integer, Atoms.Float, bool>(lhs, rhs, (l, r) => l.Value >= r.Value, out bool resultIntFloat)) return new Atoms.Boolean(resultIntFloat);
                    if (CrossOperation<Atoms.Float, Atoms.Integer, bool>(lhs, rhs, (l, r) => l.Value >= r.Value, out bool resultFloatInt)) return new Atoms.Boolean(resultFloatInt);
                    if (Operation<Atoms.Character, bool>(lhs, rhs, (l, r) => l.Value >= r.Value, out bool resultChar)) return new Atoms.Boolean(resultChar);
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Character));
                }
                case BinaryOperator.And:
                {
                    if (Operation<Atoms.Integer, int>(lhs, rhs, (l, r) => l.Value & r.Value, out int resultInt)) return new Atoms.Integer(resultInt);
                    if (Operation<Atoms.Boolean, bool>(lhs, rhs, (l, r) => l.Value && r.Value, out bool resultBool)) return new Atoms.Boolean(resultBool);
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Boolean));
                }
                case BinaryOperator.Or:
                {
                    if (Operation<Atoms.Integer, int>(lhs, rhs, (l, r) => l.Value | r.Value, out int resultInt)) return new Atoms.Integer(resultInt);
                    if (Operation<Atoms.Boolean, bool>(lhs, rhs, (l, r) => l.Value || r.Value, out bool resultBool)) return new Atoms.Boolean(resultBool);
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Boolean));
                }
                case BinaryOperator.Exclude:
                {
                    throw new NotImplementedException();
                }
                default: throw new EvaluateErrorException(ErrorHandler.EvaluateError(_lhs.Span.End, $"Unknown infix binary operator '{_operator}'!"));
            }
        }

        public override string ToString(string indent) => $"{_operator}({_lhs.ToString(indent)}, {_rhs.ToString(indent)})";
    }
}
