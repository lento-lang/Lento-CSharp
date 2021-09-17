using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class FunctionType : ObjectType
    {
        public FunctionType(string name, Dictionary<AtomicType[], Function.Variation> variations) : base(name, variations)
        {
            Variations = variations;
        }

        public Dictionary<AtomicType[], Function.Variation> Variations { get; }

        public override bool Equals(AtomicType other) => other is AnyType || (other is ObjectType atomicObjcType &&
                                                         Properties.Equals(atomicObjcType.Properties));
        public override string ToString() => $"Function[{Name}]<{Variations.Count}>";
    }
}
