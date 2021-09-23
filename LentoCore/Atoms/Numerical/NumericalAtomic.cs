using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms.Types;

namespace LentoCore.Atoms.Numerical
{
    public abstract class NumericalAtomic : Atomic
    {
        public static NumericalAtomInfo NumericalInfo;

        protected NumericalAtomic(AtomicType type) : base(type) { }
    }
}
