using System;
using System.Linq;
using ArgumentsUtil;
using Console = EzConsole.EzConsole;

namespace LentoCLI
{
    class Program
    {
		private static readonly Version VERSION = new Version("2.3.0");
		private static readonly string VERSION_TEXT = $"Lento CLI - Version {VERSION}";
		private static readonly string DESCRIPTION = "A command line interface tool for the Lento programming language.";
		private static readonly string COPYRIGHT = "Copyright (c) 2021 William Rågstad";
		private static readonly string HELP = $@"{VERSION_TEXT}
{DESCRIPTION}

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

{COPYRIGHT}";

		static void Main(string[] args)
        {
			Arguments arguments = Arguments.Parse(args, (char)KeySelector.Linux);
            
			if (args.Length == 0 || arguments.ContainsKey("-help") || arguments.ContainsKey("h")) Console.WriteLine(HELP);
			else if (arguments.ContainsKey("-version") || arguments.ContainsKey("v")) Console.WriteLine(VERSION_TEXT);
			else if (arguments.ContainsKey("-repl") || arguments.ContainsKey("r")) REPL.Run(true);
			else if (arguments.ContainsKey("-lint") || arguments.ContainsKey("l"))
			{
				if (arguments.Keyless.Count > 0) Linter.Run(arguments.Keyless.ToArray());
				else Console.WriteLine("No files found to lint");
			}
			else if (arguments.ContainsKey("-compile") || arguments.ContainsKey("c"))
			{
				if (arguments.Keyless.Count == 1) Compiler.Run(arguments.Keyless[0]);
				else if (arguments.Keyless.Count > 1) Console.WriteLine("The compile option only accepts one file");
				else Console.WriteLine("No file found to compile");
			}
			else if (arguments.Keyless.Count > 0) Interpreter.Run(arguments.Keyless.ToArray());
			else Console.WriteLine("Error: Could not parse arguments!\nUnknown command: lt " + string.Join(' ', args) + "\n\nUse --help to get more information.");
		}
    }
}
