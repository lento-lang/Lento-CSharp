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

        public Character(char value)
        {
            Value = value;
            Type = new AtomicType(GetType().Name);
        }
        public override string ToString() => $"'{Formatting.EscapeChar(Value)}'";
    }
}
