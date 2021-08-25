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
        public AtomicObjectType(string name, object properties) : this(name, string.Empty, properties) { }
        public AtomicObjectType(string name, string stringRepresentation, object properties) : base(name, stringRepresentation)
        {
            Properties = properties;
        }

        public override bool Equals(AtomicType other) => base.Equals(other) &&
                                                         other is AtomicObjectType atomicObjcType &&
                                                         Properties.Equals(atomicObjcType.Properties);
    }
}
