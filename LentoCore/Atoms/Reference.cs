using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms.Types;

namespace LentoCore.Atoms
{
    public class Reference : Atomic
    {
        public Atomic Referenced;
        public Reference(Atomic referenced) : base(BaseType)
        {
            Referenced = referenced;
        }
        public new static AtomicType BaseType => new AtomicType(nameof(Reference));
        public override string StringRepresentation() => ToString();
        public override string ToString(string indent) => $"<Reference: {Referenced}>";
    }
}
