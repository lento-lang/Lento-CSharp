using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Util;

namespace LentoCore.Atoms
{
    public class String : Atomic
    {
        public string Value;

        public String(string value) : base(BaseType)
        {
            Value = value;
        }
        public new static AtomicType BaseType => new AtomicType("string");
        public override string StringRepresentation() => Value;
        public override string ToString(string indent) => $"\"{Formatting.EscapeString(Value)}\"";
    }
}
