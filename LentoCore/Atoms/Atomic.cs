using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public abstract class Atomic
    {
        public static AtomicType BaseType { get; }
        public AtomicType Type;
        protected Atomic(AtomicType type)
        {
            Type = type;
        }
        public abstract override string ToString();
    }
}
