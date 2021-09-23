using LentoCore.Atoms.Types;

namespace LentoCore.Atoms.Numerical
{
    public class Double : NumericalAtomic
    {
        public new static NumericalAtomInfo NumericalInfo = new NumericalAtomInfo
        {
            Bits = 64,
            FloatingPoint = true,
            Signed = true
        };
        public double Value;

        public Double(double value) : base(BaseType)
        {
            Value = value;
        }
        public new static AtomicType BaseType => new AtomicType("double");
        public override string StringRepresentation() => ToString();
        public override string ToString(string indent) => Value.ToString("0.0#############################").Replace(',','.');
    }
}
