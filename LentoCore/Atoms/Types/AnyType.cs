using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class AnyType : AtomicType
    {
        public AnyType() : base("any") { }
        public new static AtomicType BaseType => new AnyType();
    }
}
