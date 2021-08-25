using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Exception
{
    public class RuntimeErrorException : System.Exception
    {
        public RuntimeErrorException(string message) : base(message) { }
        public RuntimeErrorException(string message, System.Exception innerException) : base(message, innerException) { }
    }
}
