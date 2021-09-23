using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms.Types;
using LentoCore.Evaluator;
using LentoCore.Exception;
using LentoCore.Expressions;
using LentoCore.Util;

namespace LentoCore.Atoms
{
    public class Function : Atomic
    {
        public Dictionary<AtomicType[], Variation>
            Variations; /* Dictionary of parameter type vector signature corresponding to function variation */

        public string Name;

        public Function(string name, List<(string, AtomicType)> arguments, Expressions.Expression expression, AtomicType returnType,
            Scope scope) : base(BaseType)
        {
            Name = name;
            Variations = new Dictionary<AtomicType[], Variation>
            {
                {GetArgumentTypes(arguments), new UserDefinedVariation(arguments, expression, returnType, scope)}
            };
            UpdateType();
        }

        public Function(string name, Func<Atomic[], Atomic> func, AtomicType returnType, params AtomicType[] argumentTypes) : base(BaseType)
        {
            Name = name;
            Variations = new Dictionary<AtomicType[], Variation>
            {
                {argumentTypes, new BuiltInVariation(name, func, argumentTypes, returnType)}
            };
            UpdateType();
        }

        public void AddVariation(LineColumn position, List<(string, AtomicType)> arguments,
            Expressions.Expression expression, AtomicType returnType, Scope scope)
        {
            AtomicType[] argTypes = GetArgumentTypes(arguments);
            if (ValidateArgumentSignatureCollisions(argTypes))
                throw new RuntimeErrorException(ErrorHandler.EvaluateError(position,
                    $"Function already contains a definition matching: {Name}({GetArgumentTypesList(argTypes)})"));
            Variations.Add(argTypes, new UserDefinedVariation(arguments, expression, returnType, scope));
            UpdateType();
        }

        public void AddBuiltInVariation(Func<Atomic[], Atomic> func, AtomicType returnType, params AtomicType[] argumentTypes)
        {
            if (ValidateArgumentSignatureCollisions(argumentTypes))
                throw new RuntimeErrorException(
                    $"Function already contains a definition matching the given parameter signature");
            Variations.Add(argumentTypes, new BuiltInVariation(Name, func, argumentTypes, returnType));
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

        private static AtomicType[] GetArgumentTypes(List<(string, AtomicType)> arguments) =>
            arguments.Select(arg => arg.Item2).ToArray();

        private static string GetArgumentTypeNameList(List<(string, AtomicType)> arguments) =>
            string.Join(", ", arguments.Select(arg => $"{arg.Item2} {arg.Item1}"));

        private static string GetArgumentTypesList(AtomicType[] arguments) =>
            string.Join(", ", arguments.Select(arg => arg.ToString()));
        public new static AtomicType BaseType => new AtomicType(nameof(Function));

        private void UpdateType() => Type = new FunctionType(this);
        public override string StringRepresentation() => ToString();
        public override string ToString(string indent) => Type.ToString(indent);

        public string VariationsToString(string indent) => $"\n{Formatting.Indentation}" + string.Join("\n" + Formatting.Indentation, Variations.Select(
            v =>
            {
                string variation = $"{v.Value.ReturnType.StringRepresentation()} {Name}(";
                if (v.Value is UserDefinedVariation userDefinedVariation) variation += GetArgumentTypesList(userDefinedVariation.Arguments.Select(a => a.Item2).ToArray());
                if (v.Value is BuiltInVariation builtInVariation) variation += GetArgumentTypesList(builtInVariation.ParameterTypes);
                return variation + ')';
            })
        );

        public abstract class Variation {
            public AtomicType ReturnType;

            public Variation(AtomicType returnType)
            {
                ReturnType = returnType;
            }
        }

        public class UserDefinedVariation : Variation
        {
            private readonly Expressions.Expression _expression;
            public List<(string, AtomicType)> Arguments; // <Name, Type>
            public Scope Scope;
            public UserDefinedVariation(List<(string, AtomicType)> arguments, Expression expression, AtomicType returnType, Scope scope) : base(returnType)
            {
                _expression = expression;
                Arguments = arguments;
                Scope = scope;
            }

            public Atomic Evaluate(Scope scope)
            {
                return _expression.Evaluate(scope);
            }

            public AtomicType GetReturnType(TypeTable table) => _expression.GetReturnType(table);

            public string ToString(string indent) => $"{indent}Variation<{GetArgumentTypeNameList(Arguments)}>";
        }

        public class BuiltInVariation : Variation
        {
            public AtomicType[] ParameterTypes;
            private readonly string _name;
            private readonly Func<Atomic[], Atomic> _func;

            public BuiltInVariation(string name, Func<Atomic[], Atomic> func, AtomicType[] parameterTypes, AtomicType returnType) : base(returnType)
            {
                _name = name;
                _func = func;
                ParameterTypes = parameterTypes;
            }

            public Atomic Evaluate(Atomic[] arguments, LineColumnSpan span)
            {
                if (arguments.Length != ParameterTypes.Length) throw new RuntimeErrorException(ErrorHandler.EvaluateError(span.Start, $"Call to built-in function {_name} expected {GetArgumentTypesList(ParameterTypes)} arguments but got {GetArgumentTypesList(arguments.Select(a => a.Type).ToArray())}"));
                return _func(arguments);
            }
            public override string ToString() => $"BuiltInVariation<{GetArgumentTypesList(ParameterTypes)}>";

            public AtomicType GetReturnType() => ReturnType;
        }
    }
}
