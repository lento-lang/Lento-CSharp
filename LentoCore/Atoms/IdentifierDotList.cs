using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class IdentifierDotList : Atomic
    {
        public Identifier[] Identifiers;

        public IdentifierDotList(Identifier[] identifiers)
        {
            Identifiers = identifiers;
        }

        public override string ToString() => string.Join<Identifier>('.', Identifiers);
    }
}
