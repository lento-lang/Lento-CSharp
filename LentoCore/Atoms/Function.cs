using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Evaluator;
using LentoCore.Exception;
using LentoCore.Expressions;
using LentoCore.Util;

namespace LentoCore.Atoms
{
    public class Function : Atomic
    {
        public Dictionary<Atoms.AtomicType[], Variation>
            Variations; /* Dictionary of parameter type vector signature corresponding to function variation */

        public string Name;

        public Function(string name, List<(string, Atoms.AtomicType)> arguments, Expressions.Expression expression,
            Scope scope) : base(BaseType)
        {
            Name = name;
            Variations = new Dictionary<Atoms.AtomicType[], Variation>
            {
                {GetArgumentTypes(arguments), new UserDefinedVariation(arguments, expression, scope)}
            };
            UpdateType();
        }

        public Function(string name, Func<Atomic[], Atomic> func, params AtomicType[] argumentTypes) : base(BaseType)
        {
            Name = name;
            Variations = new Dictionary<Atoms.AtomicType[], Variation>
            {
                {argumentTypes, new BuiltInVariation(name, func, argumentTypes)}
            };
            UpdateType();
        }

        public void AddVariation(LineColumn position, List<(string, Atoms.AtomicType)> arguments,
            Expressions.Expression expression, Scope scope)
        {
            Atoms.AtomicType[] argTypes = GetArgumentTypes(arguments);
            if (ValidateArgumentSignatureCollisions(argTypes))
                throw new RuntimeErrorException(ErrorHandler.EvaluateError(position,
                    $"Function already contains a definition matching: {Name} {GetArgumentTypeNameList(arguments)}"));
            Variations.Add(argTypes, new UserDefinedVariation(arguments, expression, scope));
            UpdateType();
        }

        public void AddBuiltInVariation(Func<Atomic[], Atomic> func, params AtomicType[] argumentTypes)
        {
            if (ValidateArgumentSignatureCollisions(argumentTypes))
                throw new RuntimeErrorException(
                    $"Function already contains a definition matching the given parameter signature");
            Variations.Add(argumentTypes, new BuiltInVariation(Name, func, argumentTypes));
            UpdateType();
        }

        /// <summary>
        /// Check if the new variation argument type vector collide with any of the current function variations.
        /// </summary>
        /// <param name="args">The new variation argument types signature</param>
        /// <returns>true if collision found</returns>
        private bool ValidateArgumentSignatureCollisions(params AtomicType[] args)
        {
            foreach (var (key, _) in Variations)
            {
                if (key.Length == args.Length)
                {
                    bool allMatch = true;
                    for (int i = 0; i < key.Length; i++)
                    {
                        if (!key[i].Equals(args[i])) allMatch = false;
                    }

                    if (allMatch) return true;
                }
            }

            return false;
        }

        private static Atoms.AtomicType[] GetArgumentTypes(List<(string, Atoms.AtomicType)> arguments) =>
            arguments.Select(arg => arg.Item2).ToArray();

        private static string GetArgumentTypeNameList(List<(string, Atoms.AtomicType)> arguments) =>
            string.Join(", ", arguments.Select(arg => $"{arg.Item2} {arg.Item1}"));

        private static string GetArgumentTypesList(Atoms.AtomicType[] arguments) =>
            string.Join(", ", arguments.Select(arg => arg.ToString()));
        public new static AtomicType BaseType => new AtomicType(nameof(Function));

        private void UpdateType() => Type = new AtomicObjectType($"{GetType().Name}[{Name}]<{Variations.Count}>"
            , Variations.Count);
        public override string StringRepresentation() => ToString();
        public override string ToString() => Type.ToString();

        public abstract class Variation { }

        public class UserDefinedVariation : Variation
        {
            private readonly Expressions.Expression _expression;
            public List<(string, Atoms.AtomicType)> Arguments; // <Name, Type>
            public Scope Scope;
            public UserDefinedVariation(List<(string, AtomicType)> arguments, Expression expression, Scope scope)
            {
                _expression = expression;
                Arguments = arguments;
                Scope = scope;
            }
            public Atomic EvaluateVariation(Scope callScope)
            {
                return _expression.Evaluate(callScope);
            }
            public override string ToString() => $"Variation<{GetArgumentTypeNameList(Arguments)}>";
        }

        public class BuiltInVariation : Variation
        {
            public AtomicType[] ParameterTypes;
            private readonly string _name;
            private readonly Func<Atomic[], Atomic> _func;

            public BuiltInVariation(string name, Func<Atomic[], Atomic> func, AtomicType[] parameterTypes)
            {
                _name = name;
                _func = func;
                ParameterTypes = parameterTypes;
            }

            public Atomic EvaluateVariation(Atomic[] arguments, LineColumnSpan span)
            {
                if (arguments.Length != ParameterTypes.Length) throw new RuntimeErrorException(ErrorHandler.EvaluateError(span.Start, $"Call to built-in function {_name} expected {GetArgumentTypesList(ParameterTypes)} arguments but got {GetArgumentTypesList(arguments.Select(a => a.Type).ToArray())}"));
                return _func(arguments);
            }
            public override string ToString() => $"BuiltInVariation<{GetArgumentTypesList(ParameterTypes)}>";
        }
    }
}
