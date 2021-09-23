using System;
using System.Linq;
using System.Text;
using ArgumentsUtil;
using LentoCore.Atoms;
using LentoCore.Evaluator;
using LentoCore.Exception;
using LentoCore.StandardLibrary;
using Console = EzConsole.EzConsole;

namespace LentoCLI
{
    class Program
    {
		private static readonly Version Version = new Version("2.3.0");
		private static readonly string VersionText = $"Lento CLI tool Version {Version}";
        private const string Description = "The Lento programming language command line interface.";
        private const string Copyright = "Copyright (c) 2021 William Rågstad";
        private static readonly string Help = $@"{VersionText}
{Description}

Usage: lt (<options>) (<files>)

Run file(s): lt [<files>]
    Interpret files.

Compile file: lt -c [<lang> | exe | dll] [<file>]
    (Cross) Compile a file to a target language,
    standalone executable or dynamically linked library.

Options:
    -h, --help                Prints this help message.
    -v, --version             Prints the version of the program.
    -e, --evaluate [<expr>]   Evaluate one or more expressions.
    -r, --repl (verbose)      Starts the REPL mode.
    -l, --lint [<files>]      Lints the given files.
    -c, --compile [<file>]    Compiles the given file. (Not implemented)

{Copyright}";

		static void Main(string[] args)
        {
            // args = new[] {"-r"};
			Arguments arguments = Arguments.Parse(args, (char)KeySelector.Linux);
            string prevTitle = Console.Title;
            Console.Title = "Lento";
			if (args.Length == 0 || arguments.ContainsKey("-help") || arguments.ContainsKey("h")) Console.WriteLine(Help);
			else if (arguments.ContainsKey("-version") || arguments.ContainsKey("v")) Console.WriteLine(VersionText);
            else if (arguments.ContainsKey("-evaluate")) EvaluateExpression(arguments["-evaluate"]); 
            else if (arguments.ContainsKey("e")) EvaluateExpression(arguments["e"]);
            else if (arguments.ContainsKey("-repl")) REPL.Run(arguments["-repl"].Contains("verbose"));
            else if (arguments.ContainsKey("r")) REPL.Run(arguments["r"].Contains("verbose"));
			else if (arguments.ContainsKey("-lint") || arguments.ContainsKey("l"))
			{
				string[] files = arguments.ContainsKey("-lint") ? arguments["-lint"] : arguments["l"];
				if (files.Length > 0) Linter.Run(files);
				else Console.WriteLine("No file(s) found to lint", ConsoleColor.Red);
			}
			else if (arguments.ContainsKey("-compile") || arguments.ContainsKey("c"))
			{
				string[] files = arguments.ContainsKey("-compile") ? arguments["-compile"] : arguments["c"];
				if (files.Length == 1) Compiler.Run(files[0]);
				else if (files.Length > 1) Console.WriteLine("Too many files, the compile option only accepts a single file", ConsoleColor.Red);
				else Console.WriteLine("No file found to compile", ConsoleColor.Red);
			}
			else if (arguments.Keyless.Count > 0) Interpreter.Run(arguments.Keyless.ToArray());
			else
			{
				Console.WriteLine("Error: Could not parse arguments!\nUnknown option: " + string.Join(' ', args), ConsoleColor.Red);
				Console.WriteLine("\nUse --help to get more information.");
			}

            Console.Title = prevTitle;
        }

        private static void EvaluateExpression(string[] args)
        {
            System.Console.InputEncoding = Encoding.Unicode;
            System.Console.OutputEncoding = Encoding.Unicode;
            Evaluator evaluator = new Evaluator(true);
            GlobalScope scope = new GlobalScope();
            StandardLibrary.LoadTypes(scope);
            StandardLibrary.LoadFunctions(scope);
            try
            {
                foreach(string arg in args)
                {
                    Atomic result = evaluator.EvaluateInput(arg, scope);
                    if (result.Type.Equals(Unit.BaseType)) continue;
                    Console.WriteLine(result.ToString(), ConsoleColor.Cyan);
                }
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
