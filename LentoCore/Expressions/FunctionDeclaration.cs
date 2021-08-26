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
            List<(string, Atoms.AtomicType)> arguments = new List<(string, Atoms.AtomicType)>();
            foreach (var parameter in _parameters) arguments.Add((parameter.Identifier.Name, parameter.IdentifierType));
            if (scope.Contains(_name))
            {
                Atomic match = scope.Get(_name);
                if (!(match is Atoms.Function currentFunction)) throw new RuntimeErrorException(ErrorHandler.EvaluateError(Span.Start, $"Cannot add variation to variable '{_name}'. {match.Type.ToString()} is not a function"));
                currentFunction.AddVariation(Span.Start, arguments, Body, scope);
                return currentFunction;
            }
            else
            {
                Atoms.Function newFunction = new Atoms.Function(_name, arguments, Body, scope);
                scope.Set(_name, newFunction);
                return newFunction;
            }
        }

        public override string ToString(string indent) => $"Function declaration: {_name}({string.Join(", ", _parameters.Select(p => p.ToString()))}) = {Body.ToString(indent)}";
    }
}
