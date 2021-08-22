using LentoCore.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Optimizer
{
    public class Optimizer
    {
        private AST ast;

        public Optimizer(AST ast)
        {
            this.ast = ast;
        }

        public AST RunAllOptimizations()
        {
            // Preform in-place optimization
            return ast;
        }
    }
}
