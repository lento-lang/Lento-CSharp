using System;
using LentoCore.Exception;
using LentoCore.Lexer;

using Console = EzConsole.EzConsole;

namespace LentoCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Tokenizer lex = new Tokenizer();
            while (true)
            {
                Console.Write("LI> "); 
                string expr = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(expr))
                {
                    try
                    {
                        Token[] tokens = lex.Tokenize(expr);
                        Console.Write("Tokens: ");
                        for (int i = 0; i < tokens.Length; i++)
                        {
                            Console.Write(tokens[i].ToString(), ConsoleColor.Yellow);
                            if (i < tokens.Length - 2) Console.Write(", ");
                            else if (i == tokens.Length - 2) Console.Write(" & ");
                            else Console.WriteLine();
                        }
                    }
                    catch (SyntaxErrorException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }
    }
}
