using System;
using System.Linq;
using ArgumentsUtil;
using Console = EzConsole.EzConsole;

namespace LentoCLI
{
    class Program
    {
		private static readonly Version Version = new Version("2.3.0");
		private static readonly string VersionText = $"Lento CLI - Version {Version}";
        private const string Description = "A command line interface tool for the Lento programming language.";
        private const string Copyright = "Copyright (c) 2021 William Rågstad";
        private static readonly string Help = $@"{VersionText}
{Description}

Usage: lt (<options>) (<files>)
¨¨¨¨¨

Run file(s): lt [<files>]
		Interpret files.

Compile file: lt -c [<lang> | exe | dll] [<file>]
		(Cross) Compile a file to a target language,
		standalone executable or dynamically linked library.

Options:
¨¨¨¨¨¨¨
	-h, --help
		Prints this help message.
	-v, --version
		Prints the version of the program.
	-r, --repl
		Starts the REPL mode.
	-l, --lint [<files>]
		Lints the given files.
	-c, --compile [<file>]          (Not implemented)
		Compiles the given file.

{Copyright}";

		static void Main(string[] args)
        {
			Arguments arguments = Arguments.Parse(args, (char)KeySelector.Linux);

			if (args.Length == 0 || arguments.ContainsKey("-help") || arguments.ContainsKey("h")) Console.WriteLine(Help);
			else if (arguments.ContainsKey("-version") || arguments.ContainsKey("v")) Console.WriteLine(VersionText);
			else if (arguments.ContainsKey("-repl") || arguments.ContainsKey("r")) REPL.Run(true);
			else if (arguments.ContainsKey("-lint") || arguments.ContainsKey("l"))
			{
				string[] files = arguments.ContainsKey("-lint") ? arguments["-lint"] : arguments["l"];
				if (files.Length > 0) Linter.Run(files);
				else Console.WriteLine("No files found to lint");
			}
			else if (arguments.ContainsKey("-compile") || arguments.ContainsKey("c"))
			{
				string[] files = arguments.ContainsKey("-compile") ? arguments["-compile"] : arguments["c"];
				if (files.Length == 1) Compiler.Run(files[0]);
				else if (files.Length > 1) Console.WriteLine("Too many files, the compile option only accepts a single file");
				else Console.WriteLine("No file found to compile");
			}
			else if (arguments.Keyless.Count > 0) Interpreter.Run(arguments.Keyless.ToArray());
			else
			{
				Console.WriteLine("Error: Could not parse arguments!\nUnknown command: lt " + string.Join(' ', args), ConsoleColor.Red);
				Console.WriteLine("\nUse --help to get more information.");
			}
		}
    }
}
