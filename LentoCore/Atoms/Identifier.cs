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

        public Identifier(string name)
        {
            Name = name;
            Type = new AtomicType(GetType().Name);
        }
        public override string ToString() => Name;
    }
}
