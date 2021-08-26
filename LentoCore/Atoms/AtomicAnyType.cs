using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class AtomicAnyType : AtomicType
    {
        public AtomicAnyType() : base("any") { }
        public new static AtomicType BaseType => new AtomicAnyType();
    }
}
