using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;
using LentoCore.Atoms.Types;
using LentoCore.Evaluator;
using LentoCore.Exception;
using LentoCore.Expressions;
using LentoCore.Util;

namespace LentoCore.StandardLibrary
{
    public static class StandardLibrary
    {
        public static void LoadTypes(GlobalScope scope)
        {
            void AddType(string name, AtomicType type)
            {
                scope.Set(name, type);
                scope.TypeTable.Set(name, type);
            }
            // Add all built in primitive types
            AddType(Atoms.Integer.BaseType.Name, Atoms.Integer.BaseType);
            AddType(Atoms.Long.BaseType.Name, Atoms.Long.BaseType);
            AddType(Atoms.BigInteger.BaseType.Name, Atoms.BigInteger.BaseType);
            AddType(Atoms.Float.BaseType.Name, Atoms.Float.BaseType);
            AddType(Atoms.Double.BaseType.Name, Atoms.Double.BaseType);
            AddType(Atoms.Atom.BaseType.Name, Atoms.Atom.BaseType);
            AddType(Atoms.Boolean.BaseType.Name, Atoms.Boolean.BaseType);
            AddType(Atoms.Character.BaseType.Name, Atoms.Character.BaseType);
            AddType(Atoms.String.BaseType.Name, Atoms.String.BaseType);
            AddType(Atoms.List.BaseType.Name, Atoms.List.BaseType);
            AddType(Atoms.Tuple.BaseType.Name, Atoms.Tuple.BaseType);
            AddType(Atoms.Unit.BaseType.Name, Atoms.Unit.BaseType);
            AddType(Atoms.Function.BaseType.Name, Atoms.Function.BaseType);
            AddType(AnyType.BaseType.Name, AnyType.BaseType);

            // Add mathematical aliases
            AddType("Nat", Atoms.Integer.BaseType);
            AddType("BNat", Atoms.BigInteger.BaseType);
            AddType("Real", Atoms.Float.BaseType);
            AddType("BReal", Atoms.Double.BaseType);
        }
        public static void LoadFunctions(GlobalScope scope)
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

            Atomic Str(Atomic[] args)
            {
                Atomic arg = args[0];
                if (arg is Atoms.List list && list.Elements.All(e => e is Atoms.Character)) return new Atoms.String(string.Join("", list.Elements.Select(e => ((Atoms.Character)e).Value)));
                return new Atoms.String(args[0].StringRepresentation());
            }
            Atomic Lst(Atomic[] args)
            {
                Atomic arg = args[0];
                if (arg is Atoms.Tuple tuple) return new Atoms.List(tuple.Elements.ToList());
                if (arg is Atoms.String str) return new Atoms.List(str.Value.Select(c => (Atomic)new Character(c)).ToList());
                return new Atoms.List(new List<Atomic>());
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
            BuiltIn("typeof", (args) => new Atoms.String(args[0].Type.Name), Atoms.String.BaseType, AnyType.BaseType);
            BuiltIn("nameof", (args) => new Atoms.String(((AtomicType)args[0]).Name), Atoms.String.BaseType, Atoms.Types.AtomicType.BaseType);
            BuiltIn("str", Str, Atoms.String.BaseType, AnyType.BaseType);
            BuiltIn("lst", Lst, Atoms.List.BaseType, AnyType.BaseType);
            BuiltIn("tpl", (args) => new Atoms.Tuple(((Atoms.List)args[0]).Elements.ToArray()), Atoms.Tuple.BaseType, Atoms.List.BaseType);
            BuiltIn("parse_int", (args) => new Atoms.Tuple(new Atoms.Boolean(int.TryParse(args[0].StringRepresentation(), out int r)), new Atoms.Integer(r)), Atoms.Tuple.BaseType, Atoms.String.BaseType);
            BuiltIn("parse_float", (args) => new Atoms.Tuple(new Atoms.Boolean(float.TryParse(args[0].StringRepresentation(), out float r)), new Atoms.Float(r)), Atoms.Tuple.BaseType, Atoms.String.BaseType);
            BuiltIn("parse_bool", (args) => new Atoms.Tuple(new Atoms.Boolean(bool.TryParse(args[0].StringRepresentation(), out bool r)), new Atoms.Boolean(r)), Atoms.Tuple.BaseType, Atoms.String.BaseType);
            BuiltIn("parse_atom", (args) => new Atoms.Tuple(new Atoms.Boolean(true), new Atoms.Atom(args[0].StringRepresentation())), Atoms.Tuple.BaseType, Atoms.String.BaseType);
        }

        internal static void LoadParser(Parser.Parser parser)
        {
            parser.AddParseIdentifiedFunction("print", 5);
            parser.AddParseIdentifiedFunction("println", 5);
            parser.AddParseIdentifiedFunction("typeof", 1, true);
            parser.AddParseIdentifiedFunction("nameof", 1, true);
            parser.AddParseIdentifiedFunction("str", 1);
            parser.AddParseIdentifiedFunction("lst", 1);
            parser.AddParseIdentifiedFunction("tpl", 1);
            parser.AddParseIdentifiedFunction("parse_int", 1);
            parser.AddParseIdentifiedFunction("parse_float", 1);
            parser.AddParseIdentifiedFunction("parse_bool", 1);
            parser.AddParseIdentifiedFunction("parse_atom", 1);
        }
    }
}
