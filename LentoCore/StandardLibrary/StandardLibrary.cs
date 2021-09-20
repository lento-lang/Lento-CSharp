using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;
using LentoCore.Evaluator;
using LentoCore.Exception;
using LentoCore.Expressions;
using LentoCore.Util;

namespace LentoCore.StandardLibrary
{
    public static class StandardLibrary
    {
        public static void Load(Scope scope)
        {
            void BuiltIn(string name, Func<Atomic[], Atomic> func, AtomicType returnType, params AtomicType[] parameterTypes)
            {
                if (scope.Contains(name))
                {
                    Atomic match = scope.Get(name);
                    if (!(match is Atoms.Function currentFunction)) throw new ArgumentException($"Cannot add variation to variable '{name}'. {match.Type} is not a function");
                    currentFunction.AddBuiltInVariation(func, returnType, parameterTypes);
                }
                else scope.Set(name, new Function(name, func, returnType, parameterTypes));
                scope.TypeTable.Set(Hashing.Function(name, parameterTypes), returnType);
            }
            Atomic Print(Atomic[] args) {
                Console.Write(string.Join(' ', args.Select(a => a.StringRepresentation())));
                return new Unit();
            }
            Atomic PrintLine(Atomic[] args) {
                Console.WriteLine(string.Join(' ', args.Select(a => a.StringRepresentation())));
                return new Unit();
            }
            BuiltIn("print", Print, Unit.BaseType, AnyType.BaseType);
            BuiltIn("print", Print, Unit.BaseType, AnyType.BaseType, AnyType.BaseType);
            BuiltIn("print", Print, Unit.BaseType, AnyType.BaseType, AnyType.BaseType, AnyType.BaseType);
            BuiltIn("print", Print, Unit.BaseType, AnyType.BaseType, AnyType.BaseType, AnyType.BaseType, AnyType.BaseType);
            BuiltIn("print", Print, Unit.BaseType, AnyType.BaseType, AnyType.BaseType, AnyType.BaseType, AnyType.BaseType, AnyType.BaseType);
            BuiltIn("println", PrintLine, Unit.BaseType, AnyType.BaseType);
            BuiltIn("println", PrintLine, Unit.BaseType, AnyType.BaseType, AnyType.BaseType);
            BuiltIn("println", PrintLine, Unit.BaseType, AnyType.BaseType, AnyType.BaseType, AnyType.BaseType);
            BuiltIn("println", PrintLine, Unit.BaseType, AnyType.BaseType, AnyType.BaseType, AnyType.BaseType, AnyType.BaseType);
            BuiltIn("println", PrintLine, Unit.BaseType, AnyType.BaseType, AnyType.BaseType, AnyType.BaseType, AnyType.BaseType, AnyType.BaseType);
            BuiltIn("typeof", (args) => args[0].Type, AtomicType.BaseType, AnyType.BaseType);
            BuiltIn("str", (args) => new Atoms.String(args[0].StringRepresentation()), Atoms.String.BaseType, AnyType.BaseType);
            BuiltIn("parse_int", (args) => new Atoms.Tuple(new Atoms.Boolean(int.TryParse(args[0].StringRepresentation(), out int r)), new Atoms.Integer(r)), Atoms.Tuple.BaseType, Atoms.String.BaseType);
            BuiltIn("parse_float", (args) => new Atoms.Tuple(new Atoms.Boolean(float.TryParse(args[0].StringRepresentation(), out float r)), new Atoms.Float(r)), Atoms.Tuple.BaseType, Atoms.String.BaseType);
            BuiltIn("parse_bool", (args) => new Atoms.Tuple(new Atoms.Boolean(bool.TryParse(args[0].StringRepresentation(), out bool r)), new Atoms.Boolean(r)), Atoms.Tuple.BaseType, Atoms.String.BaseType);
            BuiltIn("parse_atom", (args) => new Atoms.Tuple(new Atoms.Boolean(true), new Atoms.Atom(args[0].StringRepresentation())), Atoms.Tuple.BaseType, Atoms.String.BaseType);
        }

        internal static void Load(Parser.Parser parser)
        {
            parser.AddParseIdentifiedFunction("print", 5);
            parser.AddParseIdentifiedFunction("println", 5);
            parser.AddParseIdentifiedFunction("typeof", 1);
            parser.AddParseIdentifiedFunction("str", 1);
            parser.AddParseIdentifiedFunction("parse_int", 1);
            parser.AddParseIdentifiedFunction("parse_float", 1);
            parser.AddParseIdentifiedFunction("parse_bool", 1);
            parser.AddParseIdentifiedFunction("parse_atom", 1);
        }
    }
}
