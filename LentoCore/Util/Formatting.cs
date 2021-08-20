using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Lexer;

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
        
        public static string FormattableOptionsToString(params IFormattable[] types) =>
            FormattableOptionsToString(types.Select(t => t.ToString()));
        public static string FormattableOptionsToString(params TokenType[] types) =>
            FormattableOptionsToString(types.Select(t => t.ToString()));
        public static string FormattableOptionsToString(IEnumerable<string> types)
        {
            var enumerable = types as string[] ?? types.ToArray();
            if (enumerable.Length == 1) return enumerable[0];
            string result = string.Empty;
            for (int i = 0; i < enumerable.Length; i++)
            {
                result += enumerable[i];
                if (i < enumerable.Length - 2) result += ", ";
                else if (i == enumerable.Length - 2) result += " or ";
            }
            return result;
        }
    }
}
