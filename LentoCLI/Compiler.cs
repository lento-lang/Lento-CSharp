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
    public static class Compiler
    {
        public static void Run(string file)
        {
            try
            {
                if (FileHelper.ValidateAndOpen(file, out FileStream fs)) Evaluator.EvaluateFile(fs);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, ConsoleColor.Red);
            }
        }
    }
}
