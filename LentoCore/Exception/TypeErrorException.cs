using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Exception
{
    public class TypeErrorException : System.Exception
    {
        public TypeErrorException(string? message) : base(message) { }
        public TypeErrorException(string? message, System.Exception? innerException) : base(message, innerException) { }
    }
}
