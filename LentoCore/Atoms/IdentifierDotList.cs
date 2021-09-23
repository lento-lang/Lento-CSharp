using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms.Types;

namespace LentoCore.Atoms
{
    public class IdentifierDotList : Atomic
    {
        public Identifier[] Identifiers;

        public IdentifierDotList(Identifier[] identifiers) : base(BaseType)
        {
            Identifiers = identifiers;
        }
        public new static AtomicType BaseType => new AtomicType(nameof(IdentifierDotList));
        public override string StringRepresentation() => ToString();
        public override string ToString(string indent) => string.Join('.', Identifiers.Select(i => i.ToString()));
    }
}
