using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;
using LentoCore.Evaluator;
using LentoCore.Exception;
using LentoCore.Util;

namespace LentoCore.Expressions
{
    public class FunctionCall : Expression
    {
        private readonly Identifier _identifier;
        private readonly Expression[] _arguments;
        public FunctionCall(LineColumnSpan span, Identifier identifier, Expression[] arguments) : base(span)
        {
            _identifier = identifier;
            _arguments = arguments;
        }

        public override Atomic Evaluate(Scope scope)
        {
            Atomic functionAtom = scope.Get(_identifier.Name);
            if (functionAtom == null) throw new RuntimeErrorException(ErrorHandler.EvaluateError(Span.Start, $"Undefined function '{_identifier}'"));
            if (!(functionAtom is Atoms.Function function)) throw new RuntimeErrorException(ErrorHandler.EvaluateErrorTypeMismatch(Span.Start, functionAtom, typeof(Function)));
            Atomic[] arguments = _arguments.Select(argumentExpression => argumentExpression.Evaluate(scope)).ToArray();
            AtomicType[] argumentTypes = arguments.Select(a => a.Type).ToArray();
            foreach (var (variationTypes, variation) in function.Variations)
            {
                if (variationTypes.Length == argumentTypes.Length)
                {
                    bool allMatch = true;
                    for (int i = 0; i < variationTypes.Length; i++)
                    {
                        if (!variationTypes[i].Equals(argumentTypes[i]))
                        {
                            allMatch = false;
                            break;
                        }
                    }

                    if (allMatch)
                    {
                        if (variation is Function.UserDefinedVariation userDefinedVariation)
                        {
                            Scope variationScope = userDefinedVariation.Scope;
                            Scope callScope = userDefinedVariation.Scope.Derive("Function Call: " + _identifier.Name);
                            string[] argumentNames = userDefinedVariation.Arguments.Select(a => a.Item1).ToArray();
                            for (int i = 0; i < argumentNames.Length; i++)
                            {
                                callScope.Set(argumentNames[i], _arguments[i].Evaluate(variationScope));
                            }

                            return userDefinedVariation.Evaluate(callScope);
                        }

                        if (variation is Function.BuiltInVariation builtInVariation)
                        {
                            return builtInVariation.Evaluate(arguments, Span);
                        }

                        throw new RuntimeErrorException(ErrorHandler.EvaluateError(Span.Start, $"Unknown variation value '{variation}'"));
                    }
                }
            }
            throw new RuntimeErrorException(ErrorHandler.EvaluateError(Span.Start, $"No function variation matches the given signature '{_identifier.Name}({string.Join(", ", argumentTypes.Select(t => t.ToString()))})'." +
                "\nValid function variations are:\n" + function.VariationsToString(Formatting.Indentation)));
        }
        public override AtomicType GetReturnType(TypeTable table)
        {
            string hash = Hashing.Function(_identifier.Name, _arguments.Select(a => a.GetReturnType(table)));
            if (table.Contains(hash)) return table.Get(hash);
            var matches = table.Find(Hashing.ByName(_identifier.Name));
            if (matches.Length == 0) return new UnknownType();
            AtomicType firstType = matches.First().Value;
            if (matches.All(m => m.Value.Equals(firstType))) return firstType;
            return new UnknownType();
            
        }
        private static string GetArgumentTypesList(Atoms.AtomicType[] arguments) =>
            string.Join(", ", arguments.Select(arg => arg.ToString()));
        public override string ToString(string indent) => $"{_identifier.Name}({string.Join(", ", _arguments.Select(a => a.ToString()))})";
    }
}
