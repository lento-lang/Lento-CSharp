using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;
using LentoCore.Util;

namespace LentoCore.Expressions
{
    public class VariableDeclaration : Expression
    {
        private readonly Atoms.Identifier _name;
        private readonly Expression _value;
        public VariableDeclaration(LineColumnSpan span, Identifier name, Expression value) : base(span)
        {
            _name = name;
            _value = value;
        }

        public override Atomic Evaluate() => _value.Evaluate(); // TODO: Add key value to scope

        public override string ToString(string indent) => $"Variable declaration: {_name.ToString()} = {_value.ToString(indent)}";
    }
}
