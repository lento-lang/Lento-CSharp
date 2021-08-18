using System;
using LentoCore.Util;

namespace LentoCore.Lexer
{
    public enum TokenType
    {
        LeftParen, RightParen, LeftBracket, RightBracket, LeftCurlyBracket, RightCurlyBracket,

        Identifier, Atom, Integer, Float, Character, String, Boolean,

        Mutable, Enum, If, Case, Condition, Type,
        Attribute,

        Assign, Addition, Subtraction, Multiplication, Division, Modulus,

        Equals, NotEquals, LessThan, GreaterThan, LessThanOrEquals, GreaterThanOrEquals,
        Negate, And, Or, QuestionMark,

        Colon, SemiColon, Comma, Dot, RightArrow, ThickRightArrow,

        SingleLineComment, MultiLineComment,

        Newline, EOF
    }

    public class Token
    {
        public readonly string Lexeme;
        public readonly TokenSpan Span;
        public TokenType Type;

        public Token(TokenType type, string lexeme, TokenSpan span)
        {
            Type = type;
            Lexeme = lexeme;
            Span = span;

            if (lexeme.Length != span.Length) throw new ArgumentException("Span must have the same length as the lexeme!", nameof(span));
        }

        public LineColumn Position => Span.Start;
        public int Length => Span.Length;

        public static Token EOF(LineColumn position)
        {
            return new Token(TokenType.EOF, "\0", new TokenSpan(position, position));
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
                case TokenType.Character:
                case TokenType.String:
                case TokenType.Boolean:
                case TokenType.Attribute:
                {
                    name += $" '{Lexeme}'";
                    break;
                }
            }

            return name;
        }
    }
}