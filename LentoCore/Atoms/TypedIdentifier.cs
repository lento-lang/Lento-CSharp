using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class TypedIdentifier : Atomic
    {
        public AtomicType Type;
        public Identifier Identifier;

        public TypedIdentifier(AtomicType  type, Identifier identifier)
        {
            Type = type;
            Identifier = identifier;
        }
        
        public override AtomicType GetAtomicType() => new AtomicType(GetType().Name);

        public override string ToString() => $"{Type.ToString()} {Identifier.ToString()}";
    }
}
