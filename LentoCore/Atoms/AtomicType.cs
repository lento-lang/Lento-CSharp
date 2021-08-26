using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class AtomicType : Atomic
    {
        public string Name;
        public AtomicType(string name) : base(null)
        {
            Name = name;
        }

        public virtual bool Equals(AtomicType other)
        {
            if (this is AtomicAnyType || other is AtomicAnyType) return true;
            return Name.Equals(other.Name);
        }

        public new static AtomicType BaseType => null; // Base type
        
        public override string StringRepresentation() => ToString();
        public override string ToString() => Name;
    }
}
