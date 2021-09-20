using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Expressions
{
    internal static class Hashing
    {
        public static string Function(string name, IEnumerable<Atoms.AtomicType> paramTypes) => name + ':' + string.Join(',', paramTypes);
    }
}
