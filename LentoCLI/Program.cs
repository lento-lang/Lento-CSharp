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
	-l, --lint[files]
		Lints the given files.
	-c, --compile[files]          (Not implemented)
		Compiles the given files.

{COPYRIGHT}";

		static void Main(string[] args)
        {
			args = new[] { "--hlp", "-he" };
			Arguments arguments = Arguments.Parse(args, (char)KeySelector.Linux);
            
			if (args.Length == 0 || arguments.ContainsKey("-help") || arguments.ContainsKey("h")) Console.WriteLine(HELP);
			else if (arguments.ContainsKey("version") || arguments.ContainsKey("v")) Console.WriteLine(VERSION);
			else if (arguments.ContainsKey("repl") || arguments.ContainsKey("r")) REPL.Run(true);
			else if (arguments.Keyless.Count > 0)
            {

            }
			else Console.WriteLine("Error: Could not parse arguments!\nUnknown command: lt " + string.Join(' ', args) + "\n\nUse --help to get more information.");
		}
    }
}
