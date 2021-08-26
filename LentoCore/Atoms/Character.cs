 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Util;

namespace LentoCore.Atoms
{
    public class Character : Atomic
    {
        public char Value;

        public Character(char value) : base(BaseType)
        {
            Value = value;
        }
        public new static AtomicType BaseType => new AtomicType(nameof(Character));
        public override string StringRepresentation() => Value.ToString();
        public override string ToString() => $"'{Formatting.EscapeChar(Value)}'";
    }
}
