using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class Integer : Atomic
    {
        public int Value;

        public Integer(int value)
        {
            Value = value;
        }
        
        public override AtomicType GetAtomicType() => new AtomicType(GetType().Name);
        public override string ToString() => Value.ToString();
    }
}
