using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Lexer;
using LentoCore.Parser;

namespace LentoCore.Util
{
    public static class Formatting
    {
        public static readonly string Indentation = "    ";
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
            FormattableOptionsToString(types.Select(t => t.FastToString()));
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

    static class FastEnumExtensions
    {
        /// <summary>
        /// Fast convert enum value to string.
        /// Learn more: https://www.youtube.com/watch?v=BoE5Y6Xkm6w
        /// </summary>
        /// <typeparam name="TEnum">Supported enum type</typeparam>
        /// <param name="enumValue">String representation</param>
        /// <returns></returns>
        public static string FastToString<TEnum>(this TEnum enumValue) where TEnum : struct, Enum
        {
            if (enumValue is TokenType valueTokenType)
            {
                switch (valueTokenType)
                {
                    case TokenType.LeftParen: return nameof(TokenType.LeftParen);
                    case TokenType.RightParen: return nameof(TokenType.RightParen);
                    case TokenType.LeftBracket: return nameof(TokenType.LeftBracket);
                    case TokenType.RightBracket: return nameof(TokenType.RightBracket);
                    case TokenType.LeftCurlyBracket: return nameof(TokenType.LeftCurlyBracket);
                    case TokenType.RightCurlyBracket: return nameof(TokenType.RightCurlyBracket);
                    case TokenType.Identifier: return nameof(TokenType.Identifier);
                    case TokenType.Atom: return nameof(TokenType.Atom);
                    case TokenType.Integer: return nameof(TokenType.Integer);
                    case TokenType.Float: return nameof(TokenType.Float);
                    case TokenType.Character: return nameof(TokenType.Character);
                    case TokenType.String: return nameof(TokenType.String);
                    case TokenType.Boolean: return nameof(TokenType.Boolean);
                    case TokenType.Mutable: return nameof(TokenType.Mutable);
                    case TokenType.Struct: return nameof(TokenType.Struct);
                    case TokenType.Enum: return nameof(TokenType.Enum);
                    case TokenType.If: return nameof(TokenType.If);
                    case TokenType.Else: return nameof(TokenType.Else);
                    case TokenType.For: return nameof(TokenType.For);
                    case TokenType.Case: return nameof(TokenType.Case);
                    case TokenType.Condition: return nameof(TokenType.Condition);
                    case TokenType.TypeDeclaration: return nameof(TokenType.TypeDeclaration);
                    case TokenType.Assign: return nameof(TokenType.Assign);
                    case TokenType.Addition: return nameof(TokenType.Addition);
                    case TokenType.Subtraction: return nameof(TokenType.Subtraction);
                    case TokenType.Multiplication: return nameof(TokenType.Multiplication);
                    case TokenType.Division: return nameof(TokenType.Division);
                    case TokenType.Modulus: return nameof(TokenType.Modulus);
                    case TokenType.Equals: return nameof(TokenType.Equals);
                    case TokenType.NotEquals: return nameof(TokenType.NotEquals);
                    case TokenType.LessThan: return nameof(TokenType.LessThan);
                    case TokenType.GreaterThan: return nameof(TokenType.GreaterThan);
                    case TokenType.LessThanEquals: return nameof(TokenType.LessThanEquals);
                    case TokenType.GreaterThanEquals: return nameof(TokenType.GreaterThanEquals);
                    case TokenType.Not: return nameof(TokenType.Not);
                    case TokenType.And: return nameof(TokenType.And);
                    case TokenType.Or: return nameof(TokenType.Or);
                    case TokenType.Exclude: return nameof(TokenType.Exclude);
                    case TokenType.Colon: return nameof(TokenType.Colon);
                    case TokenType.SemiColon: return nameof(TokenType.SemiColon);
                    case TokenType.Comma: return nameof(TokenType.Comma);
                    case TokenType.Dot: return nameof(TokenType.Dot);
                    case TokenType.RightArrow: return nameof(TokenType.RightArrow);
                    case TokenType.ThickRightArrow: return nameof(TokenType.ThickRightArrow);
                    case TokenType.Reference: return nameof(TokenType.Reference);
                    case TokenType.QuestionMark: return nameof(TokenType.QuestionMark);
                    case TokenType.TupleHashTag: return nameof(TokenType.TupleHashTag);
                    case TokenType.SingleLineComment: return nameof(TokenType.SingleLineComment);
                    case TokenType.MultiLineComment: return nameof(TokenType.MultiLineComment);
                    case TokenType.Newline: return nameof(TokenType.Newline);
                    case TokenType.EOF: return nameof(TokenType.EOF);
                }
            }
            else if (enumValue is BinaryOperator valueBinaryOperator)
            {
                switch (valueBinaryOperator)
                {
                    case BinaryOperator.Add: return nameof(BinaryOperator.Add);
                    case BinaryOperator.Subtract: return nameof(BinaryOperator.Subtract);
                    case BinaryOperator.Multiply: return nameof(BinaryOperator.Multiply);
                    case BinaryOperator.Divide: return nameof(BinaryOperator.Divide);
                    case BinaryOperator.Modulus: return nameof(BinaryOperator.Modulus);
                    case BinaryOperator.Equals: return nameof(BinaryOperator.Equals);
                    case BinaryOperator.NotEquals: return nameof(BinaryOperator.NotEquals);
                    case BinaryOperator.LessThan: return nameof(BinaryOperator.LessThan);
                    case BinaryOperator.GreaterThan: return nameof(BinaryOperator.GreaterThan);
                    case BinaryOperator.LessThanEquals: return nameof(BinaryOperator.LessThanEquals);
                    case BinaryOperator.GreaterThanEquals: return nameof(BinaryOperator.GreaterThanEquals);
                    case BinaryOperator.And: return nameof(BinaryOperator.And);
                    case BinaryOperator.Or: return nameof(BinaryOperator.Or);
                    case BinaryOperator.Exclude: return nameof(BinaryOperator.Exclude);
                }
            }
            else if (enumValue is PrefixOperator valuePrefixOperator)
            {
                switch (valuePrefixOperator)
                {
                    case PrefixOperator.Not: return nameof(PrefixOperator.Not);
                    case PrefixOperator.Negative: return nameof(PrefixOperator.Negative);
                    case PrefixOperator.Referenced: return nameof(PrefixOperator.Referenced);
                }
            }
            throw new ArgumentOutOfRangeException(nameof(enumValue), enumValue, null);
        }
    }
}
