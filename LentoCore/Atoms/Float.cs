using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms.Types;

namespace LentoCore.Atoms
{
    public class Float : Atomic
    {
        public float Value;

        public Float(float value) : base(BaseType)
        {
            Value = value;
        }
        public new static AtomicType BaseType => new AtomicType("float");
        public override string StringRepresentation() => ToString();
        public override string ToString(string indent) => Value.ToString("0.0#############################").Replace(',','.');
    }
}
