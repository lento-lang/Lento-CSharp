using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCLI.Util;
using LentoCore.Evaluator;

namespace LentoCLI
{
    public static class Interpreter
    {
        public static void Run(string[] files)
        {            
            foreach(string file in files)
            {
                if (FileHelper.Validate(file)) Evaluator.EvaluateFile(File.OpenRead(file));
                else throw new ArgumentException($"File '{file}' could not be read!");
            }
        }
    }
}
