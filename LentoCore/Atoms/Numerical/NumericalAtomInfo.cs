using System;
using LentoCore.Expressions;

namespace LentoCore.Atoms.Numerical
{
    public class NumericalAtomInfo : IEquatable<NumericalAtomInfo>
    {
        public int Bits;
        public bool FloatingPoint;
        public bool Signed;

        public bool Equals(NumericalAtomInfo other)
            => other is not null
            && other.Bits == Bits
            && other.FloatingPoint == FloatingPoint
            && other.Signed == Signed;
        public bool FitsIn(NumericalAtomInfo other) =>
                Bits <= other.Bits 
                && Signed == other.Signed
                && FloatingPoint == other.FloatingPoint;
    }
}
