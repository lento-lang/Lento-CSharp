using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Exception
{
    public class SyntaxErrorException : System.Exception
    {
        public SyntaxErrorException(string message) : base(message) { }
        public SyntaxErrorException(string message, System.Exception innerException) : base(message, innerException) { }
    }
}
