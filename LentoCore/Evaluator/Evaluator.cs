using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;
using LentoCore.Expressions;

namespace LentoCore.Evaluator
{
    public class Evaluator
    {
        public static Atomic Evaluate(Expression expression)
        {
            return expression.Evaluate();
        }
    }
}
