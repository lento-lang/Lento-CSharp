using System;
using System.Collections.Generic;
using System.Linq;

namespace LentoCore.Atoms.Types
{
    public class SumType : AtomicType
    {
        public SumType(params AtomicType[] types) : base("Sum type")
        {
            Types = types?.ToList() ?? throw new ArgumentNullException(nameof(types));
        }

        public List<AtomicType> Types { get; }
        public void Add(AtomicType type) => Types.Add(type);
        public override bool Equals(AtomicType other) => Types.Any(type => type.Equals(other));

        public override string StringRepresentation() => string.Join(" | ", Types.Select(t => t.StringRepresentation()));
    }
}
