using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms.Types;

namespace LentoCore.Atoms
{
    public abstract class Atomic
    {
        public static AtomicType BaseType;
        public AtomicType Type;
        protected Atomic(AtomicType type)
        {
            Type = type;
        }
        public abstract string StringRepresentation();
        public abstract string ToString(string indent);
        public sealed override string ToString() => ToString("");
    }
}
