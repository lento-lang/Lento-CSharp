using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Evaluator
{
    public class GlobalScope : Scope
    {
        public GlobalScope() : base("Global", null) { }
    }
}
