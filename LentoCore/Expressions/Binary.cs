﻿using LentoCore.Atoms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms.Types;
using LentoCore.Evaluator;
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

        private bool SymmetricOperation<TAtomic1, TAtomic2, TPrimitive>(Atomic lhs, Atomic rhs,
            Func<TAtomic1, TAtomic2, TPrimitive> op12, Func<TAtomic2, TAtomic1, TPrimitive> op21, out TPrimitive result)
        {
            if (CrossOperation(lhs, rhs, op12, out result)) return true;
            if (CrossOperation(lhs, rhs, op21, out result)) return true;
            return false;
        }
        private bool TupleCrossOperation<TAtomic>(Atomic lhs, Atomic rhs, BinaryOperator op, Scope scope,
            out Atoms.Tuple result)
        {
            if (lhs is TAtomic && rhs is Atoms.Tuple @tupleRhs)
            {
                Expressions.Tuple resultExpression = new Expressions.Tuple(_rhs.Span, @tupleRhs.BaseExpression.Elements.Select(e => (Expression)new Binary(_operator, _lhs, e, e.Span)).ToArray());
                result = (Atoms.Tuple)resultExpression.Evaluate(scope);
                return true;
            }
            if (rhs is TAtomic && lhs is Atoms.Tuple @tupleLhs)
            {
                Expressions.Tuple resultExpression = new Expressions.Tuple(_rhs.Span, @tupleLhs.BaseExpression.Elements.Select(e => (Expression)new Binary(_operator, _rhs, e, e.Span)).ToArray());
                result = (Atoms.Tuple)resultExpression.Evaluate(scope);
                return true;
            }

            result = default;
            return false;
        }

        private bool TupleOperation(Atomic lhs, Atomic rhs, BinaryOperator op, Scope scope,
            out Atoms.Tuple result)
        {
            if (lhs is Atoms.Tuple @tupleLhs && rhs is Atoms.Tuple @tupleRhs)
            {
                if (!@tupleLhs.Size.Equals(@tupleRhs.Size)) throw new RuntimeErrorException(ErrorHandler.EvaluateErrorTypeMismatch(_rhs.Span.Start, rhs, @tupleLhs.Type.ToString()));
                result = new Atoms.Tuple(new Tuple(Span, new Expression[@tupleLhs.Size]), new Atomic[@tupleLhs.Size]);
                for (int i = 0; i < result.Size; i++)
                {
                    Expression left = new AtomicValue<Atomic>(@tupleLhs.Elements[i], @tupleLhs.BaseExpression.Span);
                    Expression right = new AtomicValue<Atomic>(@tupleRhs.Elements[i], @tupleLhs.BaseExpression.Span);
                    result.BaseExpression.Elements[i] = new Binary(op, left, right, new LineColumnSpan(left.Span.Start, right.Span.End));
                }
                result = (Atoms.Tuple)result.BaseExpression.Evaluate(scope);
                return true;
            }

            result = default;
            return false;
        }

        private TypeErrorException OperationTypeError(Atomic lhs, BinaryOperator op, params Type[] expected)
        {
            return new TypeErrorException(ErrorHandler.EvaluateErrorTypeMismatch(_lhs.Span.Start, op.FastToString(), lhs, expected));
        }

        public override Atomic Evaluate(Scope scope)
        {
            Atomic lhs = _lhs.Evaluate(scope);
            Atomic rhs = _rhs.Evaluate(scope);
            switch (_operator)
            {
                case BinaryOperator.Add:
                {
                    if (Operation<Atoms.Integer, int>(lhs, rhs, (l, r) => l.Value + r.Value, out int resultInt)) return new Atoms.Integer(resultInt);
                    if (Operation<Atoms.Float, float>(lhs, rhs, (l, r) => l.Value + r.Value, out float resultFloat)) return new Atoms.Float(resultFloat);
                    if (SymmetricOperation<Atoms.Integer, Atoms.Float, float>(lhs, rhs, (l, r) => l.Value + r.Value, (l, r) => l.Value + r.Value, out float resultIntFloat)) return new Atoms.Float(resultIntFloat);
                    if (TupleOperation(lhs, rhs, _operator, scope, out Atoms.Tuple resultTuple)) return resultTuple;
                    if (Operation<Atoms.String, string>(lhs, rhs, (l, r) => l.Value + r.Value, out string resultStringString)) return new Atoms.String(resultStringString);
                    if (Operation<Atoms.List, System.Collections.Generic.List<Atomic>>(lhs, rhs, (l, r) => l.Elements.Concat(r.Elements).ToList(), out System.Collections.Generic.List<Atomic> resultList)) return new Atoms.List(resultList);
                    if (SymmetricOperation<Atoms.String, Atoms.Character, string>(lhs, rhs, (l, r) => l.Value + r.Value, (l, r) => l.Value + r.Value, out string resultStringChar)) return new Atoms.String(resultStringChar);
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(String), typeof(Tuple), typeof(List));
                }
                case BinaryOperator.Subtract:
                {
                    if (Operation<Atoms.Integer, int>(lhs, rhs, (l, r) => l.Value - r.Value, out int resultInt)) return new Atoms.Integer(resultInt);
                    if (Operation<Atoms.Float, float>(lhs, rhs, (l, r) => l.Value - r.Value, out float resultFloat)) return new Atoms.Float(resultFloat);
                    if (SymmetricOperation<Atoms.Integer, Atoms.Float, float>(lhs, rhs, (l, r) => l.Value - r.Value, (l, r) => l.Value - r.Value, out float resultIntFloat)) return new Atoms.Float(resultIntFloat);
                    if (TupleOperation(lhs, rhs, _operator, scope, out Atoms.Tuple resultTuple)) return resultTuple;
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float));
                }
                case BinaryOperator.Multiply:
                {
                    if (Operation<Atoms.Integer, int>(lhs, rhs, (l, r) => l.Value * r.Value, out int resultInt)) return new Atoms.Integer(resultInt);
                    if (Operation<Atoms.Float, float>(lhs, rhs, (l, r) => l.Value * r.Value, out float resultFloat)) return new Atoms.Float(resultFloat);
                    if (SymmetricOperation<Atoms.Integer, Atoms.Float, float>(lhs, rhs, (l, r) => l.Value * r.Value, (l, r) => l.Value * r.Value, out float resultIntFloat)) return new Atoms.Float(resultIntFloat);
                    if (TupleCrossOperation<Atoms.Integer>(lhs, rhs, _operator, scope, out Atoms.Tuple resultTupleInt)) return resultTupleInt;
                    if (TupleCrossOperation<Atoms.Float>(lhs, rhs, _operator, scope, out Atoms.Tuple resultTupleFloat)) return resultTupleFloat;
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float));
                }
                case BinaryOperator.Divide:
                {
                    if (Operation<Atoms.Integer, int>(lhs, rhs, (l, r) => l.Value / r.Value, out int resultInt)) return new Atoms.Integer(resultInt);
                    if (Operation<Atoms.Float, float>(lhs, rhs, (l, r) => l.Value / r.Value, out float resultFloat)) return new Atoms.Float(resultFloat);
                    if (SymmetricOperation<Atoms.Integer, Atoms.Float, float>(lhs, rhs, (l, r) => l.Value / r.Value, (l, r) => l.Value / r.Value, out float resultIntFloat)) return new Atoms.Float(resultIntFloat);
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float));
                }
                case BinaryOperator.Modulus:
                {
                    if (Operation<Atoms.Integer, int>(lhs, rhs, (l, r) => l.Value % r.Value, out int resultInt)) return new Atoms.Integer(resultInt);
                    if (Operation<Atoms.Float, float>(lhs, rhs, (l, r) => l.Value % r.Value, out float resultFloat)) return new Atoms.Float(resultFloat);
                    if (SymmetricOperation<Atoms.Integer, Atoms.Float, float>(lhs, rhs, (l, r) => l.Value % r.Value, (l, r) => l.Value % r.Value, out float resultIntFloat)) return new Atoms.Float(resultIntFloat);
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float));
                }
                case BinaryOperator.Equals:
                {
                    if (Operation<Atoms.Integer, bool>(lhs, rhs, (l, r) => l.Value == r.Value, out bool resultInt)) return new Atoms.Boolean(resultInt);
                    if (Operation<Atoms.Float, bool>(lhs, rhs, (l, r) => Math.Abs(l.Value - r.Value) < float.Epsilon, out bool resultFloat)) return new Atoms.Boolean(resultFloat);
                    if (SymmetricOperation<Atoms.Integer, Atoms.Float, bool>(lhs, rhs, (l, r) => Math.Abs(l.Value - r.Value) < float.Epsilon, (l, r) => Math.Abs(l.Value - r.Value) < float.Epsilon, out bool resultIntFloat)) return new Atoms.Boolean(resultIntFloat);
                    if (Operation<Atoms.Boolean, bool>(lhs, rhs, (l, r) => l.Value == r.Value, out bool resultBool)) return new Atoms.Boolean(resultBool);
                    if (Operation<Atoms.Atom, bool>(lhs, rhs, (l, r) => l.Name == r.Name, out bool resultAtom)) return new Atoms.Boolean(resultAtom);
                    if (Operation<Atoms.Character, bool>(lhs, rhs, (l, r) => l.Value == r.Value, out bool resultChar)) return new Atoms.Boolean(resultChar);
                    if (Operation<Atoms.String, bool>(lhs, rhs, (l, r) => l.Value == r.Value, out bool resultString)) return new Atoms.Boolean(resultString);
                    if (Operation<Atoms.Unit, bool>(lhs, rhs, (l, r) => true, out bool _)) return new Atoms.Boolean(true);
                        if (Operation<AtomicType, bool>(lhs, rhs, (l, r) => l.Equals(r), out bool resultType)) return new Boolean(resultType);
                    if (TupleOperation(lhs, rhs, _operator, scope, out Atoms.Tuple resultTuple))
                    {
                        if (resultTuple.Elements.All(e => e is Atoms.Boolean b && b.Value == true)) return new Boolean(true);
                        return new Boolean(false);
                    };
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Boolean), typeof(Atom), typeof(Character), typeof(String), typeof(Unit), typeof(Atoms.Tuple), typeof(AtomicType));
                }
                case BinaryOperator.NotEquals:
                {
                    return new Atoms.Boolean(!((Atoms.Boolean)new Binary(BinaryOperator.Equals, _lhs, _rhs, Span).Evaluate(scope)).Value);
                }
                case BinaryOperator.LessThan:
                {
                    if (Operation<Atoms.Integer, bool>(lhs, rhs, (l, r) => l.Value < r.Value, out bool resultInt)) return new Atoms.Boolean(resultInt);
                    if (Operation<Atoms.Float, bool>(lhs, rhs, (l, r) => l.Value < r.Value, out bool resultFloat)) return new Atoms.Boolean(resultFloat);
                    if (SymmetricOperation<Atoms.Integer, Atoms.Float, bool>(lhs, rhs, (l, r) => l.Value < r.Value, (l, r) => l.Value < r.Value, out bool resultIntFloat)) return new Atoms.Boolean(resultIntFloat);
                    if (Operation<Atoms.Character, bool>(lhs, rhs, (l, r) => l.Value < r.Value, out bool resultChar)) return new Atoms.Boolean(resultChar);
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Character));
                }
                case BinaryOperator.LessThanEquals:
                {
                    if (Operation<Atoms.Integer, bool>(lhs, rhs, (l, r) => l.Value <= r.Value, out bool resultInt)) return new Atoms.Boolean(resultInt);
                    if (Operation<Atoms.Float, bool>(lhs, rhs, (l, r) => l.Value <= r.Value, out bool resultFloat)) return new Atoms.Boolean(resultFloat);
                    if (SymmetricOperation<Atoms.Integer, Atoms.Float, bool>(lhs, rhs, (l, r) => l.Value <= r.Value, (l, r) => l.Value <= r.Value, out bool resultIntFloat)) return new Atoms.Boolean(resultIntFloat);
                    if (Operation<Atoms.Character, bool>(lhs, rhs, (l, r) => l.Value <= r.Value, out bool resultChar)) return new Atoms.Boolean(resultChar);
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Character));
                }
                case BinaryOperator.GreaterThan:
                {
                    if (Operation<Atoms.Integer, bool>(lhs, rhs, (l, r) => l.Value > r.Value, out bool resultInt)) return new Atoms.Boolean(resultInt);
                    if (Operation<Atoms.Float, bool>(lhs, rhs, (l, r) => l.Value > r.Value, out bool resultFloat)) return new Atoms.Boolean(resultFloat);
                    if (SymmetricOperation<Atoms.Integer, Atoms.Float, bool>(lhs, rhs, (l, r) => l.Value > r.Value, (l, r) => l.Value > r.Value, out bool resultIntFloat)) return new Atoms.Boolean(resultIntFloat);
                    if (Operation<Atoms.Character, bool>(lhs, rhs, (l, r) => l.Value > r.Value, out bool resultChar)) return new Atoms.Boolean(resultChar);
                    throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Character));
                }
                case BinaryOperator.GreaterThanEquals:
                {
                    if (Operation<Atoms.Integer, bool>(lhs, rhs, (l, r) => l.Value >= r.Value, out bool resultInt)) return new Atoms.Boolean(resultInt);
                    if (Operation<Atoms.Float, bool>(lhs, rhs, (l, r) => l.Value >= r.Value, out bool resultFloat)) return new Atoms.Boolean(resultFloat);
                    if (SymmetricOperation<Atoms.Integer, Atoms.Float, bool>(lhs, rhs, (l, r) => l.Value >= r.Value, (l, r) => l.Value >= r.Value, out bool resultIntFloat)) return new Atoms.Boolean(resultIntFloat);
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
                default: throw new RuntimeErrorException(ErrorHandler.EvaluateError(_lhs.Span.End, $"Unknown infix binary operator '{_operator}'!"));
            }
        }

        public override string ToString(string indent) => $"{_operator}({_lhs.ToString(indent)}, {_rhs.ToString(indent)})";

        public override AtomicType GetReturnType(TypeTable table)
        {
            AtomicType lhs = _lhs.GetReturnType(table);
            AtomicType rhs = _rhs.GetReturnType(table);
            switch (_operator)
            {
                case BinaryOperator.Add:
                    {
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Integer.BaseType)) return Integer.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Integer.BaseType)) return Float.BaseType;
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Float.BaseType)) return Float.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Float.BaseType)) return Float.BaseType;
                        if (lhs.Equals(Atoms.Tuple.BaseType) && rhs.Equals(Atoms.Tuple.BaseType)) return Atoms.Tuple.BaseType;
                        if (lhs.Equals(String.BaseType) && rhs.Equals(String.BaseType)) return String.BaseType;
                        if (lhs.Equals(String.BaseType) && rhs.Equals(Character.BaseType)) return String.BaseType;
                        if (lhs.Equals(Character.BaseType) && rhs.Equals(String.BaseType)) return String.BaseType;
                        if (lhs.Equals(Atoms.List.BaseType) && rhs.Equals(Atoms.List.BaseType)) return Atoms.List.BaseType;
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(String), typeof(Tuple), typeof(List));
                    }
                case BinaryOperator.Subtract:
                    {
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Integer.BaseType)) return Integer.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Integer.BaseType)) return Float.BaseType;
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Float.BaseType)) return Float.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Float.BaseType)) return Float.BaseType;
                        if (lhs.Equals(Atoms.Tuple.BaseType) && rhs.Equals(Atoms.Tuple.BaseType)) return Atoms.Tuple.BaseType;
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float));
                    }
                case BinaryOperator.Multiply:
                    {
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Integer.BaseType)) return Integer.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Integer.BaseType)) return Float.BaseType;
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Float.BaseType)) return Float.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Float.BaseType)) return Float.BaseType;
                        if (lhs.Equals(Atoms.Tuple.BaseType) && rhs.Equals(Atoms.Integer.BaseType)) return Atoms.Tuple.BaseType;
                        if (lhs.Equals(Atoms.Integer.BaseType) && rhs.Equals(Atoms.Tuple.BaseType)) return Atoms.Tuple.BaseType;
                        if (lhs.Equals(Atoms.Tuple.BaseType) && rhs.Equals(Atoms.Float.BaseType)) return Atoms.Tuple.BaseType;
                        if (lhs.Equals(Atoms.Float.BaseType) && rhs.Equals(Atoms.Tuple.BaseType)) return Atoms.Tuple.BaseType;
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float));
                    }
                case BinaryOperator.Divide:
                    {
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Integer.BaseType)) return Integer.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Integer.BaseType)) return Float.BaseType;
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Float.BaseType)) return Float.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Float.BaseType)) return Float.BaseType;
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float));
                    }
                case BinaryOperator.Modulus:
                    {
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Integer.BaseType)) return Integer.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Integer.BaseType)) return Float.BaseType;
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Float.BaseType)) return Float.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Float.BaseType)) return Float.BaseType;
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float));
                    }
                case BinaryOperator.Equals:
                    {
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Integer.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Integer.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Float.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Float.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Boolean.BaseType) && rhs.Equals(Boolean.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Atom.BaseType) && rhs.Equals(Atom.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Character.BaseType) && rhs.Equals(Character.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(String.BaseType) && rhs.Equals(String.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Atoms.Tuple.BaseType) && rhs.Equals(Atoms.Tuple.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(AtomicType.BaseType) && rhs.Equals(AtomicType.BaseType)) return Boolean.BaseType;
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Boolean), typeof(Atom), typeof(Character), typeof(String), typeof(Unit), typeof(Atoms.Tuple), typeof(AtomicType));
                    }
                case BinaryOperator.NotEquals:
                    {
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Integer.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Integer.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Float.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Float.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Boolean.BaseType) && rhs.Equals(Boolean.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Atom.BaseType) && rhs.Equals(Atom.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Character.BaseType) && rhs.Equals(Character.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(String.BaseType) && rhs.Equals(String.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Atoms.Tuple.BaseType) && rhs.Equals(Atoms.Tuple.BaseType)) return Boolean.BaseType;
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Boolean), typeof(Atom), typeof(Character), typeof(String), typeof(Unit), typeof(Atoms.Tuple));
                    }
                case BinaryOperator.LessThan:
                    {
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Integer.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Integer.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Float.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Float.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Character.BaseType) && rhs.Equals(Character.BaseType)) return Boolean.BaseType;
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Character));
                    }
                case BinaryOperator.LessThanEquals:
                    {
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Integer.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Integer.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Float.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Float.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Character.BaseType) && rhs.Equals(Character.BaseType)) return Boolean.BaseType; 
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Character));
                    }
                case BinaryOperator.GreaterThan:
                    {
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Integer.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Integer.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Float.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Float.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Character.BaseType) && rhs.Equals(Character.BaseType)) return Boolean.BaseType; 
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Character));
                    }
                case BinaryOperator.GreaterThanEquals:
                    {
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Integer.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Integer.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Float.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Float.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Character.BaseType) && rhs.Equals(Character.BaseType)) return Boolean.BaseType; 
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Character));
                    }
                case BinaryOperator.And:
                    {
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Integer.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Boolean.BaseType) && rhs.Equals(Boolean.BaseType)) return Boolean.BaseType;
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Boolean));
                    }
                case BinaryOperator.Or:
                    {
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Integer.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Boolean.BaseType) && rhs.Equals(Boolean.BaseType)) return Boolean.BaseType; 
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Boolean));
                    }
                case BinaryOperator.Exclude:
                    {
                        throw new NotImplementedException();
                    }
                default: throw new RuntimeErrorException(ErrorHandler.EvaluateError(_lhs.Span.End, $"Unknown infix binary operator '{_operator}'!"));
            }
        }
    }
}
