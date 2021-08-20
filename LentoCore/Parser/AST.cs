using LentoCore.Expressions;

namespace LentoCore.Parser
{
    public class AST
    {
        public Expression[] CompilationUnit;

        public AST(params Expression[] compilationUnit)
        {
            CompilationUnit = compilationUnit;
        }
    }
}
