using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCLI.Util;
using LentoCore.Evaluator;
using Console = EzConsole.EzConsole;

namespace LentoCLI
{
    public static class Interpreter
    {
        public static void Run(string[] files)
        {
            string currentFile = null;
            try
            {
                foreach(string file in files)
                {
                    currentFile = file;
                    if (FileHelper.ValidateAndOpen(file, out FileStream fs)) new Evaluator(true).EvaluateFile(fs);
                }
            }
            catch (Exception e)
            {
                if (currentFile != null) Console.Write($"File '{currentFile}':\n   ", ConsoleColor.Red);
                Console.WriteLine(e.Message, ConsoleColor.Red);
            }
        }
    }
}
