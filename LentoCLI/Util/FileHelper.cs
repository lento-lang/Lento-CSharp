using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCLI.Util
{
    public static class FileHelper
    {
        public static bool Validate(string file)
        {
            if (!File.Exists(file))
            {
                Console.WriteLine($"File '{file}' does not exist!");
                return false;
            }
            else if (Directory.Exists(file))
            {
                Console.WriteLine($"Cannot open directory '{file}' as file!");
                return false;
            }
            return true;
        }
    }
}
