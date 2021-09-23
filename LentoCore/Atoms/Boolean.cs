using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms.Types;

namespace LentoCore.Atoms
{
    public class Boolean : Atomic
    {
        public bool Value;

        public Boolean(bool value) : base(BaseType)
        {
            Value = value;
        }
        public new static AtomicType BaseType => new AtomicType("bool");
        public override string StringRepresentation() => ToString();
        public override string ToString(string indent) => Value.ToString().ToLower();
    }
}
