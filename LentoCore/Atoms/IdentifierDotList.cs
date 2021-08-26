﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public override string ToString() => string.Join('.', Identifiers.Select(i => i.ToString()));
    }
}
