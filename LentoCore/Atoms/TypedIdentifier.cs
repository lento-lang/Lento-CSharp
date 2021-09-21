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

        public TypedIdentifier(AtomicType type, Identifier identifier) : base(BaseType)
        {
            IdentifierType = type;
            Identifier = identifier;
        }
        public new static AtomicType BaseType => new AtomicType(nameof(TypedIdentifier));
        public override string StringRepresentation() => ToString();
        public override string ToString(string indent) => $"{IdentifierType} {Identifier}";
    }
}
