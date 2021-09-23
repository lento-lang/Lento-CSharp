using LentoCore.Atoms.Types;

namespace LentoCore.Atoms.Numerical
{
    public class Float : NumericalAtomic
    {
        public new static NumericalAtomInfo NumericalInfo = new NumericalAtomInfo
        {
            Bits = 32,
            FloatingPoint = true,
            Signed = true
        };
        public float Value;

        public Float(float value) : base(BaseType)
        {
            Value = value;
        }
        public new static AtomicType BaseType => new AtomicType("float");
        public override string StringRepresentation() => ToString();
        public override string ToString(string indent) => Value.ToString("0.0#############################").Replace(',','.');
    }
}
