using LentoCore.Evaluator;
using LentoCore.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.TypeChecker
{
    public class TypeChecker
    {
        public TypeChecker() { }

        public TypeTable Check(AST ast, TypeTable tt)
        {
            ast.GetReturnType(tt);
            return tt;
        }
    }
}
