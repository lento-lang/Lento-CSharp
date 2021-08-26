using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Exception;
using LentoCore.Util;

namespace LentoCore.Atoms
{
    public class Function : Atomic
    {
        public Dictionary<Atoms.AtomicType[], Variation> FunctionVariations; /* Dictionary of parameter type vector signature corresponding to function variation */
        public string Name;

        public Function(string name, List<(string, Atoms.AtomicType)> arguments, Expressions.Expression expression)
        {
            Name = name;
            FunctionVariations = new Dictionary<Atoms.AtomicType[], Variation>
            {
                {GetArgumentTypes(arguments), new Variation(arguments, expression)}
            };
            Type = new AtomicObjectType(GetType().Name, $"{GetType().Name}[{Name}]<{FunctionVariations.Count}>", FunctionVariations.Count);
        }

        public void AddVariation(LineColumn position, List<(string, Atoms.AtomicType)> arguments, Expressions.Expression expression)
        {
            Atoms.AtomicType[] argTypes = GetArgumentTypes(arguments);
            if (ValidateArgumentSignatureCollisions(argTypes)) throw new RuntimeErrorException(ErrorHandler.EvaluateError(position, $"Function already contains a definition matching: {Name} {GetArgumentTypeNameList(arguments)}"));
            FunctionVariations.Add(argTypes, new Variation(arguments, expression));
        }

        /// <summary>
        /// Check if the new variation argument type vector collide with any of the current function variations.
        /// </summary>
        /// <param name="args">The new variation argument types signature</param>
        /// <returns>true if collision found</returns>
        private bool ValidateArgumentSignatureCollisions(AtomicType[] args)
        {
            foreach (var (key, _) in FunctionVariations)
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
            string.Join(", ", arguments.Select(arg => $"{arg.Item2.ToString()} {arg.Item1}"));
        public override string ToString() => Type.ToString();

        public class Variation : Atomic
        {
            public List<(string, Atoms.AtomicType)> Arguments; // <Name, Type>
            public Expressions.Expression Expression;

            public Variation(List<(string, Atoms.AtomicType)> arguments, Expressions.Expression expression)
            {
                Arguments = arguments;
                Expression = expression;
                Type = new AtomicObjectType("FunctionVariation", $"FunctionVariation<{GetArgumentTypeNameList(Arguments)}>", GetArgumentTypeNameList(Arguments));
            }

            public override string ToString() => Type.ToString();
        }
    }
}
