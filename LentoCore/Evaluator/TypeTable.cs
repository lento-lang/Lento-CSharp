using LentoCore.Atoms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms.Types;

namespace LentoCore.Evaluator
{
    public class TypeTable : ICloneable
    {
        private readonly Dictionary<string, AtomicType> types;

        public TypeTable()
        {
            types = new Dictionary<string, AtomicType>();
        }

        public bool Contains(string name) => types.ContainsKey(name);
        public KeyValuePair<string, AtomicType>[] Find(Predicate<string> keyPredicate) => types.ToList().FindAll(k => keyPredicate(k.Key)).ToArray();
        public KeyValuePair<string, AtomicType>[] Find(Predicate<KeyValuePair<string, AtomicType>> predicate) => types.ToList().FindAll(predicate).ToArray();
        public AtomicType Get(string name) => types[name];
        public void Set(string name, AtomicType type)
        {
            if (Contains(name)) types[name] = type;
            else types.Add(name, type);
        }
        public void Join(TypeTable other)
        {
            foreach(KeyValuePair<string, AtomicType> pairs in other.types) Set(pairs.Key, pairs.Value);
        }

        public object Clone()
        {
            var n = new TypeTable();
            n.Join(this);
            return n;
        }
    }
}
