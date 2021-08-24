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
            try
            {
                foreach(string file in files)
                {
                    if (FileHelper.ValidateAndOpen(file, out FileStream fs)) Evaluator.EvaluateFile(fs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, ConsoleColor.Red);
            }
        }
    }
}
