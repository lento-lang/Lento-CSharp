using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Util;

namespace LentoCore.Evaluator
{
    public class ParseDoneEventArgs : EventArgs
    {
        public AST AST { get; }

        public ParseDoneEventArgs(AST ast)
        {
            AST = ast;
        }
    }
}
