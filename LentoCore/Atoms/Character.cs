﻿ using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 using LentoCore.Atoms.Types;
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
        public new static AtomicType BaseType => new AtomicType("char");
        public override string StringRepresentation() => Value.ToString();
        public override string ToString(string indent) => $"'{Formatting.EscapeChar(Value)}'";
    }
}
