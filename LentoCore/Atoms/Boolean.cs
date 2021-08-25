using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class Boolean : Atomic
    {
        public bool Value;

        public Boolean(bool value)
        {
            Value = value;
        }
        
        public override AtomicType GetAtomicType() => new AtomicType(GetType().Name);
        public override string ToString() => Value.ToString().ToLower();
    }
}
