using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Util
{
    public static class Formatting
    {
        public static readonly Dictionary<char, char> CharacterEscapeCodes = new Dictionary<char, char> {
            {'0', '\0'},
            {'a', (char)7},
            {'n', '\n'},
            {'r', '\r'},
            {'t', '\t'},
            {'v', (char)11},
            {'b', '\b'},
            {'f', '\f'},
            {'e', (char)27},
            {'\\', '\\'},
            {'\'', '\''},
            {'\"', '\"'},
        };

        public static string EscapeString(string toEscape)
        {
            string result = string.Empty;
            foreach (char c in toEscape) result += EscapeChar(c);
            return result;
        }

        public static string EscapeChar(char toEscape)
        {
            if (CharacterEscapeCodes.ContainsValue(toEscape)) return "\\" + CharacterEscapeCodes.FirstOrDefault(v => v.Value == toEscape).Key;
            return toEscape.ToString();
        }
    }
}
