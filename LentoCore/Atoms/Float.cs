using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class Float : Atomic
    {
        public float Value;

        public Float(float value) : base(BaseType)
        {
            Value = value;
        }
        public new static AtomicType BaseType => new AtomicType(nameof(Float));
        public override string StringRepresentation() => ToString();
        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture).Replace(',','.');
    }
}
