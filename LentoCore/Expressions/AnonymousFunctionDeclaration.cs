using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;
using LentoCore.Util;

namespace LentoCore.Expressions
{
    public class AnonymousFunctionDeclaration : FunctionDeclaration
    {
        public AnonymousFunctionDeclaration(LineColumnSpan span, Atoms.TypedIdentifier[] parameters, Expression value) : base(span, null, parameters, value) { }
        public override string ToString(string indent) => $"Anonymous function declaration: {Body.ToString(indent)}";
    }
}
