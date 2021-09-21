using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class Identifier : Atomic
    {
        public string Name;

        public Identifier(string name) : base(BaseType)
        {
            Name = name;
        }
        public new static AtomicType BaseType => new AtomicType(nameof(Identifier));
        public override string StringRepresentation() => ToString();
        public override string ToString(string indent) => Name;
    }
}
