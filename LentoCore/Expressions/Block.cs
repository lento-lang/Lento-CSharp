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
    public class Block : Expression
    {
        private readonly Expression[] _expressions;
        public Block(LineColumnSpan span, Expression[] expressions) : base(span)
        {
            _expressions = expressions;
        }

        public override Atomic Evaluate(Scope scope)
        {
            Scope blockScope = scope.Derive("Block");
            Atomic result = new Unit();
            foreach (Expression expression in _expressions) result = expression.Evaluate(blockScope);
            return result;
        }

        public override AtomicType GetReturnType(TypeTable table) => _expressions.Length > 0
            ? _expressions.Select(e => e.GetReturnType(table)).ToList().Last() // .ToList is required to make sure the Select method maps over all elements.
            : Unit.BaseType;
        public override string ToString(string indent) => _expressions.Length > 0 ? $"{{\n{indent + Formatting.Indentation}{string.Join(indent + Formatting.Indentation, _expressions.Select(e => e.ToString(indent + Formatting.Indentation)))}\n{indent}}}" : "{ }";
    }
}
