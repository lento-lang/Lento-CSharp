using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Exception
{
    public class ParseErrorException : System.Exception
    {
        public ParseErrorException(string? message) : base(message) { }
        public ParseErrorException(string? message, System.Exception? innerException) : base(message, innerException) { }
    }
}
