using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms.Types;

namespace LentoCore.Atoms
{
    public class Unit : Tuple
    {
        public Unit() : base(new Atomic[0]) { }
        public new static AtomicType BaseType => new AtomicType(nameof(Unit));
        public override string ToString(string indent) => "#()";
    }
}
