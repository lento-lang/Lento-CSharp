using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Atoms
{
    public class List : Atomic
    {
        public Expressions.List BaseExpression;
        public List<Atomic> Elements;
        public int Size => Elements.Count;
        
        public List(List<Atomic> elements) : this(null, elements) { }
        public List(Expressions.List baseExpression, List<Atomic> elements) : base(BaseType)
        {
            BaseExpression = baseExpression;
            Elements = elements;
        }
        public new static AtomicType BaseType => new AtomicType(nameof(List));
        public override string ToString() => $"[{string.Join(", ", Elements.Select(e => e.ToString()))}]";
    }
}
