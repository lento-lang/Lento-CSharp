using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;
using LentoCore.Exception;
using LentoCore.Util;

namespace LentoCore.Expressions
{
    public class FunctionDeclaration : Expression
    {
        protected readonly Atoms.Identifier Name;
        protected readonly Atoms.TypedIdentifier[] Parameters;
        protected readonly Expression Body;
        public FunctionDeclaration(LineColumnSpan span, Identifier name, Atoms.TypedIdentifier[] parameters, Expression body) : base(span)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }

        public override Atomic Evaluate()
        {
            // TODO: Add key value to scope
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            foreach (var parameter in Parameters) arguments.Add(parameter.Identifier.Name, parameter.Type.Name);
            return new Atoms.Function(Name.Name, arguments, Body);
        }

        public override string ToString(string indent) => $"Function declaration: {Name.ToString()}({string.Join(", ", Parameters.Select(p => p.ToString()))}) = {Body.ToString(indent)}";
    }
}
