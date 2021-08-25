using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;
using LentoCore.Util;

namespace LentoCore.Evaluator
{
    public class EvaluationDoneEventArgs : EventArgs
    {
        public Atomic Result;
        public EvaluationDoneEventArgs(Atomic result)
        {
            Result = result;
        }
    }
}
