using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class TypedIdentifier : Atomic
    {
        public AtomicType IdentifierType;
        public Identifier Identifier;

        public TypedIdentifier(AtomicType type, Identifier identifier)
        {
            IdentifierType = type;
            Identifier = identifier;
            Type = new AtomicType(GetType().Name);
        }

        public override string ToString() => $"{IdentifierType.ToString()} {Identifier.ToString()}";
    }
}
