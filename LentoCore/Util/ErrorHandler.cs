using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Util
{
    public static class ErrorHandler
    {
        public static string SyntaxError(LineColumn position, string message)
        {
            return "Syntax error at line " + position.Line + " column " + position.Column + ": " + message;
        }
        public static string ParseError(LineColumn position, string message)
        {
            return "Parse error at line " + position.Line + " column " + position.Column + ": " + message;
        }
    }
}
