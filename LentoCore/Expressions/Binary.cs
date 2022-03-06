using LentoCore.Atoms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms.Types;
using LentoCore.Evaluator;
using LentoCore.Exception;
using LentoCore.Parser;
using LentoCore.Util;
using Boolean = LentoCore.Atoms.Boolean;
using Double = LentoCore.Atoms.Numerical.Double;
using String = LentoCore.Atoms.String;
using LentoCore.Atoms.Numerical;

namespace LentoCore.Expressions
{
    public class Binary : Expression
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

        #region Helper functions

        private bool overflow_int_add(int x, int y)
        {
            if (x == 0 || y == 0) return false;
            if (y > int.MaxValue - x) return true;
            if (y < int.MinValue - x) return true;
            return false;
        }
        private bool overflow_long_add(long x, long y)
        {
            if (x == 0 || y == 0) return false;
            if (y > long.MaxValue - x) return true;
            if (y < long.MinValue - x) return true;
            return false;
        }
        private bool overflow_int_mul(int x, int y)
        {
            if (x == 0) return false;
            if (y > int.MaxValue / x) return true;
            if (y < int.MinValue / x) return true;
            return false;
        }
        private bool overflow_long_mul(long x, long y)
        {
            if (x == 0) return false;
            if (y > long.MaxValue / x) return true;
            if (y < long.MinValue / x) return true;
            return false;
        }

        #endregion

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
        private RuntimeErrorException OperationRuntimeError(Atomic lhs, BinaryOperator op, params Type[] expected)
        {
            return new RuntimeErrorException(ErrorHandler.EvaluateErrorTypeMismatch(_lhs.Span.Start, op.FastToString(), lhs, expected));
        }

        public override Atomic Evaluate(Scope scope)
        {
            Atomic lhs = _lhs.Evaluate(scope);
            Atomic rhs = _rhs.Evaluate(scope);
            switch (_operator)
            {
                case BinaryOperator.Add:
                {
                    if (lhs is Integer li && rhs is Integer ri && overflow_int_add(li.Value, ri.Value)) return new Long((long)li.Value + ri.Value);
                    if (lhs is Long ll && rhs is Long rl && overflow_long_add(ll.Value, rl.Value)) return new BigInteger(new System.Numerics.BigInteger(ll.Value) + rl.Value);
                    
                    if (Operation<Integer, int>(lhs, rhs, (l, r) => l.Value + r.Value, out int resultInt)) return new Integer(resultInt);
                    if (Operation<Float, float>(lhs, rhs, (l, r) => l.Value + r.Value, out float resultFloat)) return new Float(resultFloat);
                    if (SymmetricOperation<Integer, Float, float>(lhs, rhs, (l, r) => l.Value + r.Value, (l, r) => l.Value + r.Value, out float resultIntFloat)) return new Float(resultIntFloat);
                    if (TupleOperation(lhs, rhs, _operator, scope, out Atoms.Tuple resultTuple)) return resultTuple;
                    if (Operation<Atoms.String, string>(lhs, rhs, (l, r) => l.Value + r.Value, out string resultStringString)) return new Atoms.String(resultStringString);
                    if (Operation<Atoms.List, System.Collections.Generic.List<Atomic>>(lhs, rhs, (l, r) => l.Elements.Concat(r.Elements).ToList(), out System.Collections.Generic.List<Atomic> resultList)) return new Atoms.List(resultList);
                    if (SymmetricOperation<Atoms.String, Atoms.Character, string>(lhs, rhs, (l, r) => l.Value + r.Value, (l, r) => l.Value + r.Value, out string resultStringChar)) return new Atoms.String(resultStringChar);
                    throw OperationRuntimeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Long), typeof(Double), typeof(String), typeof(Tuple), typeof(List));
                }
                case BinaryOperator.Subtract:
                {
                    if (Operation<Integer, int>(lhs, rhs, (l, r) => l.Value - r.Value, out int resultInt)) return new Integer(resultInt);
                    if (Operation<Float, float>(lhs, rhs, (l, r) => l.Value - r.Value, out float resultFloat)) return new Float(resultFloat);
                    if (SymmetricOperation<Integer, Float, float>(lhs, rhs, (l, r) => l.Value - r.Value, (l, r) => l.Value - r.Value, out float resultIntFloat)) return new Float(resultIntFloat);
                    if (TupleOperation(lhs, rhs, _operator, scope, out Atoms.Tuple resultTuple)) return resultTuple;
                    throw OperationRuntimeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Long), typeof(Double));
                }
                case BinaryOperator.Multiply:
                {
                    if (lhs is Integer li && rhs is Integer ri && overflow_int_mul(li.Value, ri.Value)) return new Long((long)li.Value * ri.Value);
                    if (lhs is Long ll && rhs is Long rl && overflow_long_mul(ll.Value, rl.Value)) return new BigInteger(new System.Numerics.BigInteger(ll.Value) * rl.Value);
                    if (Operation<Integer, int>(lhs, rhs, (l, r) => l.Value * r.Value, out int resultInt)) return new Integer(resultInt);
                    if (Operation<Float, float>(lhs, rhs, (l, r) => l.Value * r.Value, out float resultFloat)) return new Float(resultFloat);
                    if (SymmetricOperation<Integer, Float, float>(lhs, rhs, (l, r) => l.Value * r.Value, (l, r) => l.Value * r.Value, out float resultIntFloat)) return new Float(resultIntFloat);
                    if (SymmetricOperation<Long, Integer, long>(lhs, rhs, (l, r) => l.Value * r.Value, (l, r) => l.Value * r.Value, out long resultLong)) return new Long(resultLong);
                    if (TupleCrossOperation<Integer>(lhs, rhs, _operator, scope, out Atoms.Tuple resultTupleInt)) return resultTupleInt;
                    if (TupleCrossOperation<Float>(lhs, rhs, _operator, scope, out Atoms.Tuple resultTupleFloat)) return resultTupleFloat;
                    throw OperationRuntimeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Long), typeof(Double));
                }
                case BinaryOperator.Divide:
                {
                    if (Operation<Integer, int>(lhs, rhs, (l, r) => l.Value / r.Value, out int resultInt)) return new Integer(resultInt);
                    if (Operation<Float, float>(lhs, rhs, (l, r) => l.Value / r.Value, out float resultFloat)) return new Float(resultFloat);
                    if (SymmetricOperation<Integer, Float, float>(lhs, rhs, (l, r) => l.Value / r.Value, (l, r) => l.Value / r.Value, out float resultIntFloat)) return new Float(resultIntFloat);
                    throw OperationRuntimeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Long), typeof(Double));
                }
                case BinaryOperator.Modulus:
                {
                    if (Operation<Integer, int>(lhs, rhs, (l, r) => l.Value % r.Value, out int resultInt)) return new Integer(resultInt);
                    if (Operation<Float, float>(lhs, rhs, (l, r) => l.Value % r.Value, out float resultFloat)) return new Float(resultFloat);
                    if (SymmetricOperation<Integer, Float, float>(lhs, rhs, (l, r) => l.Value % r.Value, (l, r) => l.Value % r.Value, out float resultIntFloat)) return new Float(resultIntFloat);
                    throw OperationRuntimeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Long), typeof(Double));
                }
                case BinaryOperator.Equals:
                {
                    if (Operation<Integer, bool>(lhs, rhs, (l, r) => l.Value == r.Value, out bool resultInt)) return new Atoms.Boolean(resultInt);
                    if (Operation<Float, bool>(lhs, rhs, (l, r) => Math.Abs(l.Value - r.Value) < float.Epsilon, out bool resultFloat)) return new Atoms.Boolean(resultFloat);
                    if (SymmetricOperation<Integer, Float, bool>(lhs, rhs, (l, r) => Math.Abs(l.Value - r.Value) < float.Epsilon, (l, r) => Math.Abs(l.Value - r.Value) < float.Epsilon, out bool resultIntFloat)) return new Atoms.Boolean(resultIntFloat);
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
                    throw OperationRuntimeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Long), typeof(Double), typeof(Boolean), typeof(Atom), typeof(Character), typeof(String), typeof(Unit), typeof(Atoms.Tuple), typeof(AtomicType));
                }
                case BinaryOperator.NotEquals:
                {
                    return new Atoms.Boolean(!((Atoms.Boolean)new Binary(BinaryOperator.Equals, _lhs, _rhs, Span).Evaluate(scope)).Value);
                }
                case BinaryOperator.LessThan:
                {
                    if (Operation<Integer, bool>(lhs, rhs, (l, r) => l.Value < r.Value, out bool resultInt)) return new Atoms.Boolean(resultInt);
                    if (Operation<Float, bool>(lhs, rhs, (l, r) => l.Value < r.Value, out bool resultFloat)) return new Atoms.Boolean(resultFloat);
                    if (SymmetricOperation<Integer, Float, bool>(lhs, rhs, (l, r) => l.Value < r.Value, (l, r) => l.Value < r.Value, out bool resultIntFloat)) return new Atoms.Boolean(resultIntFloat);
                    if (Operation<Atoms.Character, bool>(lhs, rhs, (l, r) => l.Value < r.Value, out bool resultChar)) return new Atoms.Boolean(resultChar);
                    throw OperationRuntimeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Long), typeof(Double), typeof(Character));
                }
                case BinaryOperator.LessThanEquals:
                {
                    if (Operation<Integer, bool>(lhs, rhs, (l, r) => l.Value <= r.Value, out bool resultInt)) return new Atoms.Boolean(resultInt);
                    if (Operation<Float, bool>(lhs, rhs, (l, r) => l.Value <= r.Value, out bool resultFloat)) return new Atoms.Boolean(resultFloat);
                    if (SymmetricOperation<Integer, Float, bool>(lhs, rhs, (l, r) => l.Value <= r.Value, (l, r) => l.Value <= r.Value, out bool resultIntFloat)) return new Atoms.Boolean(resultIntFloat);
                    if (Operation<Atoms.Character, bool>(lhs, rhs, (l, r) => l.Value <= r.Value, out bool resultChar)) return new Atoms.Boolean(resultChar);
                    throw OperationRuntimeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Long), typeof(Double), typeof(Character));
                }
                case BinaryOperator.GreaterThan:
                {
                    if (Operation<Integer, bool>(lhs, rhs, (l, r) => l.Value > r.Value, out bool resultInt)) return new Atoms.Boolean(resultInt);
                    if (Operation<Float, bool>(lhs, rhs, (l, r) => l.Value > r.Value, out bool resultFloat)) return new Atoms.Boolean(resultFloat);
                    if (SymmetricOperation<Integer, Float, bool>(lhs, rhs, (l, r) => l.Value > r.Value, (l, r) => l.Value > r.Value, out bool resultIntFloat)) return new Atoms.Boolean(resultIntFloat);
                    if (Operation<Atoms.Character, bool>(lhs, rhs, (l, r) => l.Value > r.Value, out bool resultChar)) return new Atoms.Boolean(resultChar);
                    throw OperationRuntimeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Long), typeof(Double), typeof(Character));
                }
                case BinaryOperator.GreaterThanEquals:
                {
                    if (Operation<Integer, bool>(lhs, rhs, (l, r) => l.Value >= r.Value, out bool resultInt)) return new Atoms.Boolean(resultInt);
                    if (Operation<Float, bool>(lhs, rhs, (l, r) => l.Value >= r.Value, out bool resultFloat)) return new Atoms.Boolean(resultFloat);
                    if (SymmetricOperation<Integer, Float, bool>(lhs, rhs, (l, r) => l.Value >= r.Value, (l, r) => l.Value >= r.Value, out bool resultIntFloat)) return new Atoms.Boolean(resultIntFloat);
                    if (Operation<Atoms.Character, bool>(lhs, rhs, (l, r) => l.Value >= r.Value, out bool resultChar)) return new Atoms.Boolean(resultChar);
                    throw OperationRuntimeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Long), typeof(Double), typeof(Character));
                }
                case BinaryOperator.And:
                {
                    if (Operation<Integer, int>(lhs, rhs, (l, r) => l.Value & r.Value, out int resultInt)) return new Integer(resultInt);
                    if (Operation<Atoms.Boolean, bool>(lhs, rhs, (l, r) => l.Value && r.Value, out bool resultBool)) return new Atoms.Boolean(resultBool);
                    throw OperationRuntimeError(lhs, _operator, typeof(Integer), typeof(Boolean));
                }
                case BinaryOperator.Or:
                {
                    if (Operation<Integer, int>(lhs, rhs, (l, r) => l.Value | r.Value, out int resultInt)) return new Integer(resultInt);
                    if (Operation<Atoms.Boolean, bool>(lhs, rhs, (l, r) => l.Value || r.Value, out bool resultBool)) return new Atoms.Boolean(resultBool);
                    throw OperationRuntimeError(lhs, _operator, typeof(Integer), typeof(Boolean));
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
                        if (CrossNumericalOperation(lhs, rhs, out AtomicType numericalType)) return numericalType;
                        if (lhs.Equals(Atoms.Tuple.BaseType) && rhs.Equals(Atoms.Tuple.BaseType)) return Atoms.Tuple.BaseType;
                        if (lhs.Equals(String.BaseType) && rhs.Equals(String.BaseType)) return String.BaseType;
                        if (lhs.Equals(String.BaseType) && rhs.Equals(Character.BaseType)) return String.BaseType;
                        if (lhs.Equals(Character.BaseType) && rhs.Equals(String.BaseType)) return String.BaseType;
                        if (lhs.Equals(Atoms.List.BaseType) && rhs.Equals(Atoms.List.BaseType)) return Atoms.List.BaseType;
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Long), typeof(Double), typeof(BigInteger), typeof(String), typeof(Tuple), typeof(List));
                    }
                case BinaryOperator.Subtract:
                    {
                        if (CrossNumericalOperation(lhs, rhs, out AtomicType numericalType)) return numericalType;
                        if (lhs.Equals(Atoms.Tuple.BaseType) && rhs.Equals(Atoms.Tuple.BaseType)) return Atoms.Tuple.BaseType;
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Long), typeof(Double), typeof(BigInteger));
                    }
                case BinaryOperator.Multiply:
                    {
                        if (CrossNumericalOperation(lhs, rhs, out AtomicType numericalType)) return numericalType;
                        if (lhs.Equals(Atoms.Tuple.BaseType) && rhs.Equals(Integer.BaseType)) return Atoms.Tuple.BaseType;
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Atoms.Tuple.BaseType)) return Atoms.Tuple.BaseType;
                        if (lhs.Equals(Atoms.Tuple.BaseType) && rhs.Equals(Float.BaseType)) return Atoms.Tuple.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Atoms.Tuple.BaseType)) return Atoms.Tuple.BaseType;
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Long), typeof(Double), typeof(BigInteger));
                    }
                case BinaryOperator.Divide:
                    {
                        if (CrossNumericalOperation(lhs, rhs, out AtomicType numericalType)) return numericalType;
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Long), typeof(Double), typeof(BigInteger));
                    }
                case BinaryOperator.Modulus:
                    {
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Integer.BaseType)) return Integer.BaseType;
                        throw OperationTypeError(lhs, _operator, typeof(Integer));
                    }
                case BinaryOperator.Equals:
                case BinaryOperator.NotEquals:
                    {
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Integer.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Integer.BaseType) && rhs.Equals(Float.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Integer.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Float.BaseType) && rhs.Equals(Float.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Boolean.BaseType) && rhs.Equals(Boolean.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Atom.BaseType) && rhs.Equals(Atom.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Character.BaseType) && rhs.Equals(Character.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(String.BaseType) && rhs.Equals(String.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(Atoms.Tuple.BaseType) && rhs.Equals(Atoms.Tuple.BaseType)) return Boolean.BaseType;
                        if (lhs.Equals(AtomicType.BaseType) && rhs.Equals(AtomicType.BaseType)) return Boolean.BaseType;
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Long), typeof(Double), typeof(BigInteger), typeof(Boolean), typeof(Atom), typeof(Character), typeof(String), typeof(Unit), typeof(Atoms.Tuple), typeof(AtomicType));
                    }
                case BinaryOperator.LessThan:
                    {
                        if (CrossNumericalOperation(lhs, rhs, out AtomicType _)) return Boolean.BaseType;
                        if (lhs.Equals(Character.BaseType) && rhs.Equals(Character.BaseType)) return Boolean.BaseType;
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Long), typeof(Double), typeof(BigInteger), typeof(Character));
                    }
                case BinaryOperator.LessThanEquals:
                    {
                        if (CrossNumericalOperation(lhs, rhs, out AtomicType _)) return Boolean.BaseType;
                        if (lhs.Equals(Character.BaseType) && rhs.Equals(Character.BaseType)) return Boolean.BaseType; 
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Long), typeof(Double), typeof(BigInteger), typeof(Character));
                    }
                case BinaryOperator.GreaterThan:
                    {
                        if (CrossNumericalOperation(lhs, rhs, out AtomicType _)) return Boolean.BaseType;
                        if (lhs.Equals(Character.BaseType) && rhs.Equals(Character.BaseType)) return Boolean.BaseType; 
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Long), typeof(Double), typeof(BigInteger), typeof(Character));
                    }
                case BinaryOperator.GreaterThanEquals:
                    {
                        if (CrossNumericalOperation(lhs, rhs, out AtomicType _)) return Boolean.BaseType;
                        if (lhs.Equals(Character.BaseType) && rhs.Equals(Character.BaseType)) return Boolean.BaseType; 
                        throw OperationTypeError(lhs, _operator, typeof(Integer), typeof(Float), typeof(Long), typeof(Double), typeof(BigInteger), typeof(Character));
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

        #region NumericalAtomInfo functions

        private static bool CrossNumericalOperation(AtomicType lhs, AtomicType rhs, out AtomicType result)
        {
            result = null;
            if (NumericalOperation<Integer, Integer>(lhs, rhs, out AtomicType intIntType)) result = intIntType;
            else if (NumericalOperation<Integer, Long>(lhs, rhs, out AtomicType intLongType)) result = intLongType;
            else if (NumericalOperation<Integer, BigInteger>(lhs, rhs, out AtomicType intBigIntegerType)) result = intBigIntegerType;
            else if (NumericalOperation<Integer, Float>(lhs, rhs, out AtomicType intFloatType)) result = intFloatType;
            else if (NumericalOperation<Integer, Double>(lhs, rhs, out AtomicType intDoubleType)) result = intDoubleType;
            else if (NumericalOperation<Long, Long>(lhs, rhs, out AtomicType longLongType)) result = longLongType;
            else if (NumericalOperation<Long, BigInteger>(lhs, rhs, out AtomicType longBigIntegerType)) result = longBigIntegerType;
            else if (NumericalOperation<Long, Float>(lhs, rhs, out AtomicType longFloatType)) result = longFloatType;
            else if (NumericalOperation<Long, Double>(lhs, rhs, out AtomicType longDoubleType)) result = longDoubleType;
            else if (NumericalOperation<BigInteger, BigInteger>(lhs, rhs, out AtomicType bigIntegerBigIntegerType)) result = bigIntegerBigIntegerType;
            else if (NumericalOperation<BigInteger, Float>(lhs, rhs, out AtomicType bigIntegerFloatType)) result = bigIntegerFloatType;
            else if (NumericalOperation<BigInteger, Double>(lhs, rhs, out AtomicType bigIntegerDoubleType)) result = bigIntegerDoubleType;
            else if (NumericalOperation<Float, Float>(lhs, rhs, out AtomicType floatFloatType)) result = floatFloatType;
            else if (NumericalOperation<Float, Double>(lhs, rhs, out AtomicType floatDoubleType)) result = floatDoubleType;
            else if (NumericalOperation<Double, Double>(lhs, rhs, out AtomicType doubleDoubleType)) result = doubleDoubleType;
            return result != null;
        }

        private static bool NumericalOperation<T1, T2>(AtomicType lhs, AtomicType rhs, out AtomicType result)
            where T1 : NumericalAtomic
            where T2 : NumericalAtomic
        {
            result = null;
            if (typeof(T1) != typeof(Integer) && typeof(T1) != typeof(Long) && typeof(T1) != typeof(BigInteger) &&
                typeof(T1) != typeof(Float) && typeof(T1) != typeof(Double)) return false;
            if (typeof(T2) != typeof(Integer) && typeof(T2) != typeof(Long) && typeof(T2) != typeof(BigInteger) &&
                typeof(T2) != typeof(Float) && typeof(T2) != typeof(Double)) return false;
            var b1 = (AtomicType)typeof(T1).GetProperty("BaseType")?.GetValue(null);
            var b2 = (AtomicType)typeof(T2).GetProperty("BaseType")?.GetValue(null);
            if (!(lhs.Equals(b1) && rhs.Equals(b2))
                && !(lhs.Equals(b2) && rhs.Equals(b1))) return false;
            var n1 = (NumericalAtomInfo)typeof(T1).GetField("NumericalInfo")?.GetValue(null);
            var n2 = (NumericalAtomInfo)typeof(T2).GetField("NumericalInfo")?.GetValue(null);
            if (n1 is null || n2 is null) return false;
            NumericalAtomInfo expected = new NumericalAtomInfo
            {
                Bits = Math.Max(n1.Bits, n2.Bits),
                FloatingPoint = n1.FloatingPoint ^ n2.FloatingPoint,
                Signed = n1.Signed || n2.Signed
            };
            bool match = GetMatchingNumericalType(expected, out SumType t);
            if (!match) return false;
            result = t.Types.Count == 1 ? t.Types[0] : t;
            return true;
        }

        private static bool GetMatchingNumericalType(NumericalAtomInfo info, out SumType result)
        {
            result = new SumType();
            if (info.FitsIn(Integer.NumericalInfo)) result.Add(Integer.BaseType);
            if (info.FitsIn(Long.NumericalInfo)) result.Add(Long.BaseType);
            if (info.FitsIn(BigInteger.NumericalInfo)) result.Add(BigInteger.BaseType);
            if (info.FitsIn(Float.NumericalInfo)) result.Add(Float.BaseType);
            if (info.FitsIn(Double.NumericalInfo)) result.Add(Double.BaseType);
            return result.Types.Count != 0;
        }

        #endregion
    }
}
