using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class TypedIdentifier : Atomic
    {
        public Identifier Type;
        public Identifier Identifier;

        public TypedIdentifier(Identifier type, Identifier identifier)
        {
            Type = type;
            Identifier = identifier;
        }

        public override string GetTypeName() => GetType().Name;

        public override string ToString() => $"{Type.ToString()} {Identifier.ToString()}";
    }
}
