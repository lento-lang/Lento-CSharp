using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Evaluator
{
    public class GlobalScope : Scope
    {
        public GlobalScope() : base("Global", null) {
            // Add all built in primitive types
            Environment.Add(Atoms.Integer.BaseType.Name, Atoms.Integer.BaseType);
            Environment.Add(Atoms.Float.BaseType.Name, Atoms.Float.BaseType);
            Environment.Add(Atoms.Boolean.BaseType.Name, Atoms.Boolean.BaseType);
            Environment.Add(Atoms.Character.BaseType.Name, Atoms.Character.BaseType);
            Environment.Add(Atoms.String.BaseType.Name, Atoms.String.BaseType);
            Environment.Add(Atoms.List.BaseType.Name, Atoms.List.BaseType);
            Environment.Add(Atoms.Tuple.BaseType.Name, Atoms.Tuple.BaseType);
            Environment.Add(Atoms.Unit.BaseType.Name, Atoms.Unit.BaseType);
            Environment.Add(Atoms.Function.BaseType.Name, Atoms.Function.BaseType);
        }
    }
}
