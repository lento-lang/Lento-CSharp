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
        
        public List(List<Atomic> elements)
        {
            Elements = elements;
        }
        public List(Expressions.List baseExpression, List<Atomic> elements)
        {
            BaseExpression = baseExpression;
            Elements = elements;
        }
        public override AtomicType GetAtomicType() => new AtomicType(GetType().Name);
        public override string ToString() => $"[{string.Join(", ", Elements.Select(e => e.ToString()))}]";
    }
}
