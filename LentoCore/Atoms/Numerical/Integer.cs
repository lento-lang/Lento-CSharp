using LentoCore.Atoms.Types;

namespace LentoCore.Atoms.Numerical
{
    public class Integer : NumericalAtomic
    {
        public new static NumericalAtomInfo NumericalInfo = new NumericalAtomInfo
        {
            Bits = 32,
            FloatingPoint = false,
            Signed = true
        };
        public int Value;

        public Integer(int value) : base(BaseType)
        {
            Value = value;
        }
        public new static AtomicType BaseType => new AtomicType("int");
        public override string StringRepresentation() => ToString();
        public override string ToString(string indent) => Value.ToString();
    }
}
