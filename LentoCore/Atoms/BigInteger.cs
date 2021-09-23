using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms.Types;

namespace LentoCore.Atoms
{
    public class BigInteger : Atomic
    {
        public System.Numerics.BigInteger Value;

        public BigInteger(System.Numerics.BigInteger value) : base(BaseType)
        {
            Value = value;
        }
        public new static AtomicType BaseType => new AtomicType(nameof(BigInteger));
        public override string StringRepresentation() => ToString();
        public override string ToString(string indent) => Value.ToString();
    }
}
