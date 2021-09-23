using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms.Types;

namespace LentoCore.Atoms
{
    public class Double : Atomic
    {
        public double Value;

        public Double(double value) : base(BaseType)
        {
            Value = value;
        }
        public new static AtomicType BaseType => new AtomicType("double");
        public override string StringRepresentation() => ToString();
        public override string ToString(string indent) => Value.ToString("0.0#############################").Replace(',','.');
    }
}
