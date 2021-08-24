using System;
using System.IO;
using Console = EzConsole.EzConsole;

namespace LentoCLI.Util
{
    public static class FileHelper
    {
        public static bool ValidateAndOpen(string file, out FileStream stream)
        {
            stream = null;
            if (!File.Exists(file))
            {
                Console.WriteLine($"File '{file}' does not exist!", ConsoleColor.Red);
                return false;
            }
            if (Directory.Exists(file))
            {
                Console.WriteLine($"Cannot open directory '{file}' as file!", ConsoleColor.Red);
                return false;
            }
            try
            {
                stream = File.OpenRead(file);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"File '{file}' could not be read! {e.Message}", ConsoleColor.Red);
                return false;
            }
        }
    }
}
