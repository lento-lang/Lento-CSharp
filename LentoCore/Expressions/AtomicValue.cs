using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;
using LentoCore.Util;

namespace LentoCore.Expressions
{
    class AtomicValue<TAtom> : Expression where TAtom : Atomic
    {
        private readonly TAtom _value;

        public AtomicValue(TAtom value, LineColumnSpan span) : base(span)
        {
            _value = value;
        }

        public override Atomic Evaluate()
        {
            return _value;
        }

        public override string ToString() => $"Atomic {typeof(TAtom).Name}: {_value.ToString()}";
    }
    /*
    class AtomicValue : AtomicValue<Atom>
    {
        public AtomicValue(Atom value, LineColumnSpan span) : base(value, span) { }
    }
    */
}
