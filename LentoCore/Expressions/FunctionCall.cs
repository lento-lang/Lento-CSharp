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
            this._identifier = identifier;
            this._arguments = arguments;
        }

        public override Atomic Evaluate(Scope scope)
        {
            Atomic functionAtom = scope.Get(_identifier.Name);
            if (!(functionAtom is Atoms.Function function)) throw new RuntimeErrorException(ErrorHandler.EvaluateErrorTypeMismatch(Span.Start, functionAtom, typeof(Function)));
            Atomic[] arguments = _arguments.Select(argumentExpression => argumentExpression.Evaluate(scope)).ToArray();
            AtomicType[] argumentTypes = arguments.Select(a => a.Type).ToArray();
            foreach (KeyValuePair<AtomicType[], Function.Variation> variation in function.FunctionVariations)
            {
                if (variation.Key.Length == argumentTypes.Length)
                {
                    bool allMatch = true;
                    for (int i = 0; i < variation.Key.Length; i++)
                    {
                        if (!variation.Key[i].Equals(argumentTypes[i]))
                        {
                            allMatch = false;
                            break;
                        }
                    }

                    if (allMatch)
                    {
                        Scope variationScope = variation.Value.Scope;
                        Scope callScope = variation.Value.Scope.Derive("Function Call: " + _identifier.Name);
                        string[] argumentNames = variation.Value.Arguments.Select(a => a.Item1).ToArray();
                        for (int i = 0; i < argumentNames.Length; i++)
                        {
                            callScope.Set(argumentNames[i], _arguments[i].Evaluate(variationScope));
                        }

                        return variation.Value.Expression.Evaluate(callScope);
                    }
                }
            }
            throw new RuntimeErrorException(ErrorHandler.EvaluateError(Span.Start, $"No function variation of '{_identifier.Name}' matches the given argument signature: {string.Join(", ", argumentTypes.Select(t => t.ToString()))}"));
        }

        public override string ToString(string indent) => $"{_identifier.Name}({string.Join(", ", _arguments.Select(a => a.ToString()))})";
    }
}
