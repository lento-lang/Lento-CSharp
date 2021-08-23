using System;
using System.Linq;
using ArgumentsUtil;
using Console = EzConsole.EzConsole;

namespace LentoCLI
{
    class Program
    {
		private static readonly string VERSION = "2.3.0";
		private static readonly string DESCRIPTION = "A command line interface tool for the Lento programming language.";
		private static readonly string COPYRIGHT = "Copyright (c) 2021 William Rågstad";
		private static readonly string HELP = $@"Lento CLI - version {VERSION}
{DESCRIPTION}

Usage: lt (options) (files)
¨¨¨¨¨

Run file(s): lt [files]
		Interpret files.

Compile file: lt -c -l {"\"Java\""} [file]
		(Cross) Compile a file to a target language, architecture standalone executable or dynamically linked library.

Options:
¨¨¨¨¨¨¨
	-h, --help
		Prints this help message.
	-v, --version
		Prints the version of the program.
	-r, --repl
		Starts the REPL mode.
	-l, --lint[files]
		Lints the given files.
	-c, --compile[files]          (Not implemented)
		Compiles the given files.

{COPYRIGHT}";

		static void Main(string[] args)
        {
			Arguments arguments = Arguments.Parse(args);
            
			if (args.Length == 0 || arguments.ContainsKey("help"))
            {
				Console.WriteLine(HELP);
            }
			else if (arguments.ContainsKey("repl"))
            {
				REPL.Run(true);
			}
        }
    }
}
