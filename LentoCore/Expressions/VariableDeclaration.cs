﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;
using LentoCore.Atoms.Types;
using LentoCore.Evaluator;
using LentoCore.Exception;
using LentoCore.Util;

namespace LentoCore.Expressions
{
    public class VariableDeclaration : Expression
    {
        private readonly string _name;
        private readonly Expression _value;
        public VariableDeclaration(LineColumnSpan span, string name, Expression value) : base(span)
        {
            _name = name;
            _value = value;
        }

        public override Atomic Evaluate(Scope scope)
        {
            if (scope.Contains(_name)) throw new RuntimeErrorException(ErrorHandler.EvaluateError(Span.Start, $"A local variable or function named '{_name}' is already defined in this scope"));
            Atomic value = scope.Set(_name, _value.Evaluate(scope));
            scope.TypeTable.Set(_name, value.Type);
            return value;
        }
        public override AtomicType GetReturnType(TypeTable table)
        {
            AtomicType returnType = _value.GetReturnType(table);
            table.Set(_name, returnType);
            return returnType;
        }

        public override string ToString(string indent) => $"Variable declaration: {_name.ToString()} = {_value.ToString(indent)}";
    }
}
