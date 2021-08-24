using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;
using LentoCore.Expressions;
using LentoCore.Lexer;
using LentoCore.Util;

namespace LentoCore.Evaluator
{
    public class Evaluator
    {
        public static Atomic EvaluateFile(Stream fileStream)
        {
            Tokenizer lex = new Tokenizer();
            TokenStream tokens = lex.Tokenize(fileStream);
            Parser.Parser parser = new Parser.Parser();
            AST ast = parser.Parse(tokens);
            TypeChecker.TypeChecker tc = new TypeChecker.TypeChecker(ast);
            tc.Run();
            return EvaluateExpression(ast);
        }
        public static Atomic EvaluateInput(string input)
        {
            Tokenizer lex = new Tokenizer();
            TokenStream tokens = lex.Tokenize(input);
            Parser.Parser parser = new Parser.Parser();
            AST ast = parser.Parse(tokens);
            TypeChecker.TypeChecker tc = new TypeChecker.TypeChecker(ast);
            tc.Run();
            return EvaluateExpression(ast);
        }
        public static Atomic EvaluateExpression(Expression expression)
        {
            return expression.Evaluate();
        }
    }
}
