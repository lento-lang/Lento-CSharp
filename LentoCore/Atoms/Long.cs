using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class Long : Atomic
    {
        public long Value;

        public Long(long value) : base(BaseType)
        {
            Value = value;
        }
        public new static AtomicType BaseType => new AtomicType("long");
        public override string StringRepresentation() => ToString();
        public override string ToString(string indent) => Value.ToString();
    }
}
