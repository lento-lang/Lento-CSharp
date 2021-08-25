using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;
using LentoCore.Evaluator;
using LentoCore.Exception;
using LentoCore.Util;

namespace LentoCore.Expressions
{
    public class FunctionDeclaration : Expression
    {
        private readonly string _name;
        private readonly Atoms.TypedIdentifier[] _parameters;
        protected readonly Expression Body;
        public FunctionDeclaration(LineColumnSpan span, string name, Atoms.TypedIdentifier[] parameters, Expression body) : base(span)
        {
            _name = name;
            _parameters = parameters;
            Body = body;
        }

        public override Atomic Evaluate(Scope scope)
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            foreach (var parameter in _parameters) arguments.Add(parameter.Identifier.Name, parameter.Type.Name);
            Atoms.Function function = new Atoms.Function(_name, arguments, Body);
            scope.Set(_name, function);
            return function;
        }

        public override string ToString(string indent) => $"Function declaration: {_name}({string.Join(", ", _parameters.Select(p => p.ToString()))}) = {Body.ToString(indent)}";
    }
}
