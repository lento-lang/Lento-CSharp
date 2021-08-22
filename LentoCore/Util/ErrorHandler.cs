using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;

namespace LentoCore.Util
{
    public static class ErrorHandler
    {
        public static string SyntaxError(LineColumn position, string message) => $"Syntax error at line {position.Line} column {position.Column}: {message}";
        public static string ParseError(LineColumn position, string message) => $"Parse error at line {position.Line} column {position.Column}: {message}";
        public static string TypeError(LineColumn position, string message) => $"Type error at line {position.Line} column {position.Column}: {message}";
        public static string EvaluateError(LineColumn position, string message) => $"Evaluation runtime error at line {position.Line} column {position.Column}: {message}";

        public static string EvaluateErrorTypeMismatch(LineColumn position, Atomic got, params Type[] expectedTypes) =>
            EvaluateError(position, $"Type mismatch. Expected {Formatting.FormattableOptionsToString(expectedTypes.Select(et => et.Name))} but got '{got.ToString()}' of type {got.GetType().Name}");
    }
}
