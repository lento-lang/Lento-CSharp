﻿using System.Linq;
using LentoCore.Atoms;
using LentoCore.Atoms.Types;
using LentoCore.Evaluator;
using LentoCore.Expressions;
using LentoCore.Util;

namespace LentoCore.Util
{
    public class AST : Expression
    {
        public Expression[] CompilationUnit;

        public AST(Expression[] compilationUnit, LineColumnSpan span) : base(span)
        {
            CompilationUnit = compilationUnit;
        }

        public override Atomic Evaluate(Scope scope)
        {
            Atomic result = new Atoms.Unit();
            foreach (Expression expression in CompilationUnit) result = expression.Evaluate(scope);
            return result;
        }

        public override AtomicType GetReturnType(TypeTable table) => CompilationUnit.Length > 0
            ? CompilationUnit.Select(e => e.GetReturnType(table)).ToList().Last() // .ToList is required to make sure the Select method maps over all elements.
            : Unit.BaseType;
        public override string ToString(string indent) => string.Join('\n', CompilationUnit.Select(e => e.ToString(indent)));
    }
}
