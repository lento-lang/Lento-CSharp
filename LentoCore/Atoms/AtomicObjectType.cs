using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class AtomicObjectType : AtomicType
    {
        public object Properties;
        public AtomicObjectType(string name, object properties) : base(name)
        {
            Properties = properties;
        }

        public override bool Equals(AtomicType other) => other is AtomicAnyType || (base.Equals(other) &&
                                                         other is AtomicObjectType atomicObjcType &&
                                                         Properties.Equals(atomicObjcType.Properties));
    }
}
