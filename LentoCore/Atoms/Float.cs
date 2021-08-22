using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class Float : Atomic
    {
        public float Value;

        public Float(float value)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString().Replace(',','.');
    }
}
