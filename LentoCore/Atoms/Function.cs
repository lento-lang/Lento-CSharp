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
        public Dictionary<string[], Variation> FunctionVariations; /* Dictionary of argument types corresponding to function variation */ // TODO: Replace string[] with DataType type
        public string Name;

        public Function(string name, Dictionary<string, string> arguments, Expressions.Expression expression)
        {
            Name = name;
            FunctionVariations = new Dictionary<string[], Variation>
            {
                {GetArgumentTypes(arguments), new Variation(arguments, expression)}
            };
        }

        public void AddVariation(Dictionary<string, string> arguments, Expressions.Expression expression)
        {
            string[] argTypes = GetArgumentTypes(arguments);
            if (FunctionVariations.ContainsKey(argTypes)) throw new EvaluateErrorException($"Function already contains a definition matching: {Name} {GetArgumentTypeNameList(arguments)}");
            FunctionVariations.Add(argTypes, new Variation(arguments, expression));
        }

        private static string[] GetArgumentTypes(Dictionary<string, string> arguments) =>
            arguments.Select(kvp => kvp.Value).ToArray();

        private static string GetArgumentTypeNameList(Dictionary<string, string> arguments) =>
            string.Join(", ", arguments.Select(kvp => $"{kvp.Value} {kvp.Key}"));
        
        public override AtomicType GetAtomicType() => new AtomicObjectType(GetType().Name, $"{GetType().Name}[{Name}]<{FunctionVariations.Count}>", FunctionVariations.Count);
        public override string ToString() => GetAtomicType().ToString();

        public class Variation : Atomic
        {
            public Dictionary<string, string> Arguments; // <Name, Type>
            public Expressions.Expression Expression;

            public Variation(Dictionary<string, string> arguments, Expressions.Expression expression)
            {
                Arguments = arguments;
                Expression = expression;
            }
            
            public override AtomicType GetAtomicType() => new AtomicObjectType("FunctionVariation", $"FunctionVariation<{GetArgumentTypeNameList(Arguments)}>", GetArgumentTypeNameList(Arguments));

            public override string ToString() => GetAtomicType().ToString();
        }
    }
}
