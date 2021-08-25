using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;
using LentoCore.Evaluator;
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

        public override Atomic Evaluate(Scope scope) => scope.Set(_name, _value.Evaluate(scope));

        public override string ToString(string indent) => $"Variable declaration: {_name.ToString()} = {_value.ToString(indent)}";
    }
}
