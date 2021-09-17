using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class ObjectType : AtomicType
    {
        public object Properties;
        public ObjectType(string name, object properties) : base(name)
        {
            Properties = properties;
        }

        public override bool Equals(AtomicType other) => other is AnyType || (base.Equals(other) &&
                                                         other is ObjectType atomicObjcType &&
                                                         Properties.Equals(atomicObjcType.Properties));
    }
}
