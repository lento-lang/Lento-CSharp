using LentoCore.Atoms.Types;

namespace LentoCore.Atoms.Numerical
{
    public class Long : NumericalAtomic
    {
        public new static NumericalAtomInfo NumericalInfo = new NumericalAtomInfo
        {
            Bits = 64,
            FloatingPoint = false,
            Signed = true
        };
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
