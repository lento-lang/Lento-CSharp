using LentoCore.Atoms.Types;

namespace LentoCore.Atoms.Numerical
{
    public class BigInteger : NumericalAtomic
    {
        public new static NumericalAtomInfo NumericalInfo = new NumericalAtomInfo
        {
            Bits = 128,
            FloatingPoint = false,
            Signed = true
        };
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
