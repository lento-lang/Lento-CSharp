using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class AtomicType : Atomic
    {
        public string Name;
        private readonly string _stringRepresentation;
        public AtomicType(string name) : this(name, string.Empty) { }
        public AtomicType(string name, string stringRepresentation)
        {
            Name = name;
            _stringRepresentation = stringRepresentation;
        }
        public virtual bool Equals(AtomicType other) => Name.Equals(other.Name);

        public override string ToString() =>
            _stringRepresentation != string.Empty ? _stringRepresentation : Name;
    }
}
