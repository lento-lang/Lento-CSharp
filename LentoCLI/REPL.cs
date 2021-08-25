using System;
using System.Linq;
using LentoCore.Atoms;
using LentoCore.Exception;
using LentoCore.TypeChecker;
using LentoCore.Lexer;
using LentoCore.Parser;
using LentoCore.Util;
using LentoCore.Evaluator;
using LentoCore.Expressions;
using Console = EzConsole.EzConsole;

namespace LentoCLI
{
    public static class REPL
    {
        public static void Run(bool verbose)
        {
            Evaluator evaluator = new Evaluator();
            if (verbose)
            {
                evaluator.OnTokenizeDone += (sender, e) =>
                {
                    Console.Write("Tokens: ");
                    int tokenCount = e.TokenStream.Count();
                    foreach ((Token token, int i) in e.TokenStream.Select((value, index) => (value, index)))
                    {
                        Console.Write(token.ToString(), ConsoleColor.Yellow);
                        if (i < tokenCount - 1) Console.Write(", ");
                        else Console.WriteLine();
                    }
                };
                evaluator.OnParseDone += (sender, e) =>
                {
                    Console.WriteLine("Expressions: ");
                    foreach (Expression expression in e.AST.CompilationUnit)
                    {
                        Console.Write("- ");
                        Console.WriteLine(expression.ToString(), ConsoleColor.Magenta);
                    }
                };
                evaluator.OnEvaluationDone += (sender, e) =>
                {
                    Console.Write("Result: "); // Atomic result is printed afterwards
                };
            }

            GlobalScope scope = new GlobalScope();

            while (true)
            {
                Console.Write("LI> ");
                string expr = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(expr))
                {
                    try
                    {
                        Atomic result = evaluator.EvaluateInput(expr, scope);
                        Console.WriteLine(result.ToString(), ConsoleColor.Cyan);
                    }
                    catch (SyntaxErrorException e)
                    {
                        Console.WriteLine(e.Message, ConsoleColor.Red);
                    }
                    catch (ParseErrorException e)
                    {
                        Console.WriteLine(e.Message, ConsoleColor.Red);
                    }
                    catch (RuntimeErrorException e)
                    {
                        Console.WriteLine(e.Message, ConsoleColor.Red);
                    }
                }
            }
        }
    }
}
