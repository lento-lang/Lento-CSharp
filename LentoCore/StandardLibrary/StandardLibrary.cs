using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;
using LentoCore.Evaluator;
using LentoCore.Exception;
using LentoCore.Util;

namespace LentoCore.StandardLibrary
{
    public static class StandardLibrary
    {
        public static void Load(Scope scope)
        {
            void BuiltIn(string name, Func<Atomic[], Atomic> func, params AtomicType[] parameterTypes)
            {
                if (scope.Contains(name))
                {
                    Atomic match = scope.Get(name);
                    if (!(match is Atoms.Function currentFunction)) throw new ArgumentException($"Cannot add variation to variable '{name}'. {match.Type} is not a function");
                    currentFunction.AddBuiltInVariation(func, parameterTypes);
                }
                else scope.Set(name, new Function(name, func, parameterTypes));
            }
            Atomic Print(Atomic[] args) {
                Console.Write(string.Join(' ', args.Select(a => a.StringRepresentation())));
                return new Unit();
            }
            Atomic PrintLine(Atomic[] args) {
                Console.WriteLine(string.Join(' ', args.Select(a => a.StringRepresentation())));
                return new Unit();
            }
            BuiltIn("print", Print, AtomicAnyType.BaseType);
            BuiltIn("print", Print, AtomicAnyType.BaseType, AtomicAnyType.BaseType);
            BuiltIn("print", Print, AtomicAnyType.BaseType, AtomicAnyType.BaseType, AtomicAnyType.BaseType);
            BuiltIn("print", Print, AtomicAnyType.BaseType, AtomicAnyType.BaseType, AtomicAnyType.BaseType, AtomicAnyType.BaseType);
            BuiltIn("print", Print, AtomicAnyType.BaseType, AtomicAnyType.BaseType, AtomicAnyType.BaseType, AtomicAnyType.BaseType, AtomicAnyType.BaseType);
            BuiltIn("println", PrintLine, AtomicAnyType.BaseType);
            BuiltIn("println", PrintLine, AtomicAnyType.BaseType, AtomicAnyType.BaseType);
            BuiltIn("println", PrintLine, AtomicAnyType.BaseType, AtomicAnyType.BaseType, AtomicAnyType.BaseType);
            BuiltIn("println", PrintLine, AtomicAnyType.BaseType, AtomicAnyType.BaseType, AtomicAnyType.BaseType, AtomicAnyType.BaseType);
            BuiltIn("println", PrintLine, AtomicAnyType.BaseType, AtomicAnyType.BaseType, AtomicAnyType.BaseType, AtomicAnyType.BaseType, AtomicAnyType.BaseType);
        }
    }
}
