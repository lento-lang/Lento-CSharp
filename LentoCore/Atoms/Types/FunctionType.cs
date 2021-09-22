using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class FunctionType : ObjectType
    {
        private readonly Function function;

        public FunctionType(Function function) : base(function.Name, function.Variations)
        {
            this.function = function;
        }

        public override bool Equals(AtomicType other) => other is AnyType || (other is ObjectType atomicObjcType &&
                                                         Properties.Equals(atomicObjcType.Properties));
        public override string ToString(string indent) => $"Function[{Name}]{{{function.VariationsToString(indent)}\n}}";
    }
}
