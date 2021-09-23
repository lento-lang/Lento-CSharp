using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms.Types;

namespace LentoCore.Atoms
{
    public class Integer : Atomic
    {
        public int Value;

        public Integer(int value) : base(BaseType)
        {
            Value = value;
        }
        public new static AtomicType BaseType => new AtomicType("int");
        public override string StringRepresentation() => ToString();
        public override string ToString(string indent) => Value.ToString();
    }
}
