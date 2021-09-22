using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Expressions
{
    internal static class Hashing
    {
        private static readonly char _separator = ':';
        public static string Function(string name, IEnumerable<Atoms.AtomicType> paramTypes) => name + _separator + string.Join(',', paramTypes);
        public static Predicate<string> ByName(string name) => p => p.Split(_separator)[0].Equals(name);
    }
}
