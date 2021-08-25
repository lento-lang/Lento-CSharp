using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class Reference : Atomic
    {
        public Atomic Referenced;
        public Reference(Atomic referenced)
        {
            Referenced = referenced;
        }
        
        public override string GetTypeName() => GetType().Name;
        public override string ToString() => $"<Reference: {Referenced.ToString()}>";
    }
}
