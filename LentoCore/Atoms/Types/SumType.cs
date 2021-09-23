using System;
using System.Linq;

namespace LentoCore.Atoms.Types
{
    public class SumType : AtomicType
    {
        public SumType(params AtomicType[] types) : base("Sum type")
        {
            Types = types ?? throw new ArgumentNullException(nameof(types));
        }

        public AtomicType[] Types { get; }

        public override bool Equals(AtomicType other) => Types.Any(type => type.Equals(other));

        public override string StringRepresentation() => string.Join(" | ", Types.Select(t => t.StringRepresentation()));
    }
}
