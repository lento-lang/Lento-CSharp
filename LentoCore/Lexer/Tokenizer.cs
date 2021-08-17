using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Exception;
using LentoCore.Util;

namespace LentoCore.Lexer
{
    public class Tokenizer
    {
        private string _currentToken;
        private StreamReader _input;
        private readonly LineColumn _position;
        private readonly List<Token> _tokens;

        public Tokenizer()
        {
            _position = new LineColumn();
            _tokens = new List<Token>();
        }

        #region Tokenize
        
        public Token[] Tokenize(Stream sourceStream, Encoding encoding)
        {
            _position.Clear();
            _tokens.Clear();
            _input = new StreamReader(sourceStream, encoding);
            while (!EOF()) AddNext();
            return _tokens.ToArray();
        }
        public Token[] Tokenize(Stream sourceStream) => Tokenize(sourceStream, Encoding.UTF8);

        public Token[] Tokenize(string input, Encoding encoding) =>
            Tokenize(new MemoryStream(encoding.GetBytes(input ?? "")), encoding);
        public Token[] Tokenize(string input) => Tokenize(input, Encoding.UTF8);

        #endregion

        #region Helper functions

        private bool EOF() => _input.EndOfStream;
        private char Peek() => (char)_input.Peek();
        private char Eat()
        {
            int c = _input.Read();
            if (c == '\n') _position.NextLine();
            else if (c != '\r') _position.NextColumn();
            _currentToken += (char)c;
            return (char) c;
        }

        private void ClearCurrentToken() => _currentToken = string.Empty;
        private string GetCurrentToken()
        {
            string ct = _currentToken;
            ClearCurrentToken();
            return ct;
        }
        private string Error(string message)
        {
            return ErrorHandler.SyntaxError(_position, message);
        }
        private void Add(TokenType type, string lexeme) => _tokens.Add(new Token(type, lexeme, new TokenSpan(_position.CloneAndSubtract(lexeme.Length), _position.Clone())));
        private void Add(TokenType type, char lexeme) => _tokens.Add(new Token(type, lexeme.ToString(), new TokenSpan(_position.CloneAndSubtract(1), _position.Clone())));

        #endregion

        #region Lexing functions

        private void AddNext()
        {
            ClearCurrentToken();
            LineColumn start = _position.Clone();
            char c = Eat();
            switch (c)
            {
                case '(': { Add(TokenType.LParen, c); break; }
                case ')': { Add(TokenType.RParen, c); break; }
                case '[': { Add(TokenType.LBracket, c); break; }
                case ']': { Add(TokenType.RBracket, c); break; }
                case '{': { Add(TokenType.LCurlyBracket, c); break; }
                case '}': { Add(TokenType.RCurlyBracket, c); break; }

                case '\n': { Add(TokenType.Newline, c); break; }

                default:
                {
                    if (char.IsWhiteSpace(c)) break;

                    if (char.IsLetter(c) || c == '_') ScanIdentifier();
                    else throw new SyntaxErrorException(Error($"Unexpected character '{c}'"));
                    break;
                }
            }
        }

        private void ScanIdentifier()
        {
            bool expectIdent = false;
            while (!EOF() && (char.IsLetter(Peek()) || char.IsDigit(Peek()) || Peek() == '_')) {
                Eat();
                expectIdent = false;
                if (!EOF() && Peek() == '.') {
                    Eat();
                    expectIdent = true;
                }
            }

            if (expectIdent) {
                throw new SyntaxErrorException(Error("Expected identifier, but got " + Peek()));
            }

            string lexeme = GetCurrentToken();

            switch (lexeme) {
                case "true":
                case "false": { Add(TokenType.Boolean, lexeme); break; }
                default:
                {
                    Add(TokenType.Identifier, lexeme);
                    break;
                }
            }
        }

        #endregion
    }
}
