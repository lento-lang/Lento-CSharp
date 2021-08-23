using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Exception
{
    public class EvaluateErrorException : System.Exception
    {
        public EvaluateErrorException(string message) : base(message) { }
        public EvaluateErrorException(string message, System.Exception innerException) : base(message, innerException) { }
    }
}
