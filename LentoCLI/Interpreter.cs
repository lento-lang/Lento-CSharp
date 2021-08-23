using System;
using System.Collections.Generic;
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
                if (FileHelper.Validate(file))
                {

                    Evaluator.Evaluate();
                }
            }
        }
    }
}
