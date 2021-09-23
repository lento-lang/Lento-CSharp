using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class Atom : Atomic
    {
        public string Name;

        public Atom(string name) : base(BaseType)
        {
            Name = name;
        }
        
        public new static AtomicType BaseType => new AtomicType(nameof(Atom));
        public override string StringRepresentation() => ToString();
        public override string ToString(string indent) => $":{Name}";

    }
}
