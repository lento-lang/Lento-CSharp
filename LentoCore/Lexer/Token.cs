using System;
using LentoCore.Util;

namespace LentoCore.Lexer
{
    public enum TokenType
    {
        LeftParen, RightParen, LeftBracket, RightBracket, LeftCurlyBracket, RightCurlyBracket,

        Identifier, Atom, Integer, Float, Character, String, Boolean,

        Mutable, Enum, If, For, Case, Condition, Type,
        Attribute,

        Assign, Addition, Subtraction, Multiplication, Division, Modulus,

        Equals, NotEquals, LessThan, GreaterThan, LessThanEquals, GreaterThanEquals,
        Not, And, Or, Exclude,

        Colon, SemiColon, Comma, Dot, RightArrow, ThickRightArrow, Reference, QuestionMark, TupleHashTag,

        SingleLineComment, MultiLineComment,

        Newline, EOF
    }

    public class Token
    {
        public readonly string Lexeme;
        public readonly LineColumnSpan Span;
        public TokenType Type;

        public Token(TokenType type, string lexeme, LineColumnSpan span)
        {
            Type = type;
            Lexeme = lexeme;
            Span = span;
        }

        public LineColumn Position => Span.Start;
        public int Length => Span.Length;

        public static Token EOF(LineColumn position)
        {
            return new Token(TokenType.EOF, "\0", new LineColumnSpan(position, position));
        }

        public override string ToString()
        {
            string name = Type.ToString();
            switch (Type)
            {
                case TokenType.Identifier:
                case TokenType.Atom:
                case TokenType.Integer:
                case TokenType.Float:
                case TokenType.Boolean:
                case TokenType.Attribute:
                {
                    name += $" '{Lexeme}'";
                    break;
                }
                case TokenType.Character:
                case TokenType.String:
                {
                    name += $" '{Formatting.EscapeString(Lexeme)}'";
                    break;
                }
            }

            return name;
        }
    }
}