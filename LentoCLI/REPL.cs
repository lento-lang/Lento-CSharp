using System;
using System.Linq;
using System.Text;
using LentoCore.Atoms;
using LentoCore.Exception;
using LentoCore.Lexer;
using LentoCore.Evaluator;
using LentoCore.Expressions;
using LentoCore.StandardLibrary;
using Console = EzConsole.EzConsole;

namespace LentoCLI
{
    public static class REPL
    {
        public static void Run(bool verbose)
        {
            Console.Title = "Lento | REPL";
            System.Console.InputEncoding = Encoding.Unicode;
            System.Console.OutputEncoding = Encoding.Unicode;
            Evaluator evaluator = new Evaluator(true);
            GlobalScope scope = new GlobalScope();
            StandardLibrary.LoadTypes(scope);
            StandardLibrary.LoadFunctions(scope);

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
                    Console.Write($"Result ({e.Result.Type}): "); // Atomic result is printed afterwards
                };
            }

            while (true)
            {
                Console.Write("LI> ");
                string expr = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(expr))
                {
                    try
                    {
                        Atomic result = evaluator.EvaluateInput(expr, scope);
                        if (!verbose && result.Type.Equals(Unit.BaseType)) continue; 
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
                    catch (TypeErrorException e)
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
