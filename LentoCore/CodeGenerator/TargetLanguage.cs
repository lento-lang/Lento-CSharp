using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.CodeGenerator
{
    public abstract class TargetLanguage
    {
        /// <summary>
        /// Visit callback for each node in the AST, the method should cover all
        /// cases of expressions and generate source code in another language based
        /// on each expression type.
        /// </summary>
        /// <param name="expression">The current node in the AST</param>
        /// <returns>Source code in the target language</returns>
        public abstract string GenerateNode(Expressions.Expression expression);
    }
}
