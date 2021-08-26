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

        public Boolean(bool value) : base(BaseType)
        {
            Value = value;
        }
        public new static AtomicType BaseType => new AtomicType(nameof(Boolean));
        public override string ToString() => Value.ToString().ToLower();
    }
}
