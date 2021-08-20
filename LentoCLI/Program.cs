using System;
using System.Linq;
using LentoCore.Exception;
using LentoCore.Expressions;
using LentoCore.Lexer;
using LentoCore.Parser;
using LentoCore.Util;
using Console = EzConsole.EzConsole;

namespace LentoCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Tokenizer lex = new Tokenizer();
            Parser parser = new Parser();
            while (true)
            {
                Console.Write("LI> "); 
                string expr = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(expr))
                {
                    try
                    {
                        TokenStream tokens = lex.Tokenize(expr);
                        Console.Write("Tokens: ");
                        int tokenCount = tokens.Count();
                        foreach ((Token token, int i) in tokens.Select((value, index) => (value, index)))
                        {
                            Console.Write(token.ToString(), ConsoleColor.Yellow);
                            if (i < tokenCount - 1) Console.Write(", ");
                            else Console.WriteLine();
                        }

                        AST ast = parser.Parse(tokens);
                        Console.WriteLine("Expressions: ");
                        foreach (Expression expression in ast.CompilationUnit)
                        {
                            Console.WriteLine($" {expression.ToString()}", ConsoleColor.Magenta);
                        }
                    }
                    catch (SyntaxErrorException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    catch (ParseErrorException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }
    }
}
