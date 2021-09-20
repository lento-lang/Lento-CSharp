using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class SumType : AtomicType
    {
        public SumType(params AtomicType[] types) : base("Sum type")
        {
            if (types is null)
            {
                throw new ArgumentNullException(nameof(types));
            }
            Types = types;
        }

        public AtomicType[] Types { get; }

        public override bool Equals(AtomicType other) => Types.Any(type => type.Equals(other));

        public override string StringRepresentation() => string.Join(" | ", Types.Select(t => t.StringRepresentation()));
    }
}
