using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using LentoCore.Exception;
using LentoCore.Util;

namespace LentoCore.Lexer
{
    public class Tokenizer
    {
        private string _currentToken;
        private StreamReader _input;
        private TokenStream _tokenStream;
        private readonly LineColumn _position;

        public Tokenizer()
        {
            _position = new LineColumn();
        }

        #region Tokenize
        
        public TokenStream Tokenize(Stream sourceStream, Encoding encoding)
        {
            _position.Clear();
            _tokenStream = new TokenStream();
            _input = new StreamReader(sourceStream, encoding);
            while (!EOF()) AddNext(); // Put in a thread and add a start method?
            _tokenStream.WriteEndOfStream();
            return _tokenStream;
        }
        public TokenStream Tokenize(Stream sourceStream) => Tokenize(sourceStream, Encoding.UTF8);
        public TokenStream Tokenize(string input, Encoding encoding) =>
            Tokenize(new MemoryStream(encoding.GetBytes(input ?? "")), encoding);
        public TokenStream Tokenize(string input) => Tokenize(input, Encoding.UTF8);

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

        private void Add(TokenType type)
        {
            if (_currentToken == string.Empty) throw new InvalidOperationException("Cannot use Add(TokenType type) here! _currentToken is empty, probably due to a previous call to GetCurrentToken().");
            Add(type, _currentToken);
        }
        private void Add(TokenType type, string lexeme) => Add(type, lexeme, new LineColumnSpan(_position.CloneAndSubtract(lexeme.Length), _position.Clone()));

        private void Add(TokenType type, string lexeme, LineColumnSpan span, bool validateSpanLexemeLength = true)
        {
            if (validateSpanLexemeLength && lexeme.Length != span.Length) throw new ArgumentException("Span must have the same length as the lexeme!", nameof(span));
            _tokenStream.Write(new Token(type, lexeme, span));
        }

        private string Error(string message) => ErrorHandler.SyntaxError(_position.CloneAndSubtract(1), message);
        private string ErrorUnexpected(char c) => Error($"Unexpected character '{c}'");
        private string ErrorUnexpected(char c, string expected) => Error($"Unexpected character '{Formatting.EscapeChar(c)}'. Expected {expected}");
        private string ErrorUnexpectedEOF(string expected) => Error($"Unexpected end of file. Expected {expected}");
        #endregion

        #region Lexing functions

        private void AddNext()
        {
            ClearCurrentToken();
            LineColumn start = _position.Clone();
            char c = Eat();
            switch (c)
            {
                case '(': { Add(TokenType.LeftParen); break; }
                case ')': { Add(TokenType.RightParen); break; }
                case '[': { Add(TokenType.LeftBracket); break; }
                case ']': { Add(TokenType.RightBracket); break; }
                case '{': { Add(TokenType.LeftCurlyBracket); break; }
                case '}': { Add(TokenType.RightCurlyBracket); break; }

                case '#':
                {
                    if (Peek() == '(') Add(TokenType.TupleHashTag);
                    else throw new SyntaxErrorException(ErrorUnexpected(Peek(), "Left parenthesis to declare tuple"));
                    break;
                }
                case '@':
                {
                    if (char.IsLetter(Peek())) ScanAttribute();
                    else throw new SyntaxErrorException(ErrorUnexpected(c, "attribute name"));
                    break;
                }
                case ':':
                {
                    if (char.IsLetter(Peek())) ScanAtom();
                    else Add(TokenType.Colon);
                    break;
                }
                case '=':
                {
                    if (Peek() == '=') { Eat(); Add(TokenType.Equals); }
                    else if (Peek() == '>') { Eat(); Add(TokenType.ThickRightArrow); }
                    else Add(TokenType.Assign);
                    break;
                }
                case '!':
                {
                    if (Peek() == '=') { Eat(); Add(TokenType.NotEquals); }
                    else Add(TokenType.Not);
                    break;
                }
                case '<':
                {
                    if (Peek() == '=') { Eat(); Add(TokenType.LessThanEquals); }
                    else Add(TokenType.LessThan);
                    break;
                }
                case '>':
                {
                    if (Peek() == '=') { Eat(); Add(TokenType.GreaterThanEquals); }
                    else Add(TokenType.GreaterThan);
                    break;
                }
                case '-':
                {
                    if (Peek() == '>') { Eat(); Add(TokenType.RightArrow); }
                    else Add(TokenType.Subtraction);
                    break;
                }
                case '/':
                {
                    if (Peek() == '/') ScanSingleLineComment();
                    else if (Peek() == '*') ScanMultiLineComment();
                    else Add(TokenType.Division);
                    break;
                }
                case '&':
                {
                    if (Peek() == '&') { Eat(); Add(TokenType.And); }
                    else if (char.IsLetter(Peek())) { Eat(); Add(TokenType.Reference); }
                    else throw new SyntaxErrorException(ErrorUnexpected(Peek(), "logical AND or referenced identifier"));
                    break;
                }
                case '+': { Add(TokenType.Addition); break; }
                case '*': { Add(TokenType.Multiplication); break; }
                case '%': { Add(TokenType.Modulus); break; }
                case '|': { Add(TokenType.Or); break; }
                case '\\': { Add(TokenType.Exclude); break; }
                case '?': { Add(TokenType.QuestionMark); break; }
                
                case '.': { Add(TokenType.Dot); break; }
                case ',': { Add(TokenType.Comma); break; }
                case ';': { Add(TokenType.SemiColon); break; }
                case '\n': { Add(TokenType.Newline); break; }

                case '"': { ScanString(start); break; }
                case '\'': { ScanCharacter(start); break; }

                default:
                {
                    if (char.IsWhiteSpace(c)) break;

                    if (char.IsLetter(c) || c == '_') ScanIdentifier();
                    else if (char.IsDigit(c)) ScanNumber();
                    else throw new SyntaxErrorException(ErrorUnexpected(c));
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
            if (expectIdent) throw new SyntaxErrorException(Error("Expected identifier, but got " + Peek()));

            string lexeme = GetCurrentToken();
            switch (lexeme) {
                case "true":
                case "false": { Add(TokenType.Boolean, lexeme); break; }
                case "mut": { Add(TokenType.Mutable, lexeme); break; }
                case "enum": { Add(TokenType.Enum, lexeme); break; }
                case "type": { Add(TokenType.Type, lexeme); break; }
                case "if": { Add(TokenType.If, lexeme); break; }
                case "for": { Add(TokenType.For, lexeme); break; }
                case "case": { Add(TokenType.Case, lexeme); break; }
                case "cond": { Add(TokenType.Condition, lexeme); break; }
                default:
                {
                    if (lexeme.IndexOf('.') != -1)
                    {
                        string[] identifiers = lexeme.Split('.');
                        for (int i = 0; i < identifiers.Length; i++)
                        {
                            Add(TokenType.Identifier, identifiers[i]);
                            if (i < identifiers.Length - 1) Add(TokenType.Dot, ".");
                        }
                    }
                    else Add(TokenType.Identifier, lexeme);
                    break;
                }
            }
        }

        private void ScanAtom()
        { 
            ClearCurrentToken();
            while (!EOF() && char.IsLetter(Peek())) Eat();
            Add(TokenType.Atom);
        }

        private void ScanNumber()
        {
            bool isFloat = false;
            while (!EOF() && (char.IsDigit(Peek()) || Peek() == '.'))
            {
                if (Peek() == '.')
                {
                    Eat(); // Eat the dot
                    if (char.IsDigit(Peek())) isFloat = true; // 7.;
                    else
                    {
                        // First add number
                        string numberWithDot = GetCurrentToken();
                        string number = numberWithDot.Substring(0, numberWithDot.Length - 1);
                        Add(isFloat ? TokenType.Float : TokenType.Integer, number);
                        // Then add dot
                        Add(TokenType.Dot, ".");
                        return;
                    }
                }

                Eat();
            }
            Add(isFloat ? TokenType.Float : TokenType.Integer);
        }

        
        private char ScanCharacterToken()
        {
            if (_currentToken != string.Empty) throw new InvalidOperationException("Current token must be empty when calling ScanCharacterToken()");
            if (EOF()) throw new SyntaxErrorException(ErrorUnexpectedEOF("character"));
            char c = Eat();
            if (c == '\\')
            {
                // Escaped or special character
                if (Formatting.CharacterEscapeCodes.ContainsKey(Peek())) c = Formatting.CharacterEscapeCodes[Eat()];
                else if (Peek() == 'u')
                {
                    // Unicode character
                    Eat();
                    string unicodeHex = string.Empty;
                    for (int i = 0; i < 4; i++)
                    {
                        if (EOF()) throw new SyntaxErrorException(ErrorUnexpectedEOF("unicode character value \\uHHHH"));
                        char nx = char.ToLower(Peek());
                        if (char.IsDigit(nx) || (nx >= 'a' && nx <= 'f') ) unicodeHex += Eat();
                        else throw new SyntaxErrorException(ErrorUnexpected(nx, "hexadecimal value"));
                    }
                    if (int.TryParse(unicodeHex, NumberStyles.AllowHexSpecifier, new NumberFormatInfo(), out int unicodeCharacter)) c = (char) unicodeCharacter;
                    else throw new SyntaxErrorException(Error("Invalid unicode character value! Numbers must be hexadecimal \\uHHHH"));
                }
                else throw new SyntaxErrorException(ErrorUnexpected(Peek(), "character escape code or unicode hexadecimal value"));
            }
            // Normal character
            ClearCurrentToken(); // Clear current token
            return c;
        }
        private void ScanCharacter(LineColumn start)
        {
            ClearCurrentToken();
            if (Peek() == '\'') throw new SyntaxErrorException(ErrorUnexpected('\'', "single character value, '' is not valid."));
            char character = ScanCharacterToken();
            if (Peek() == '\'') Eat();
            else if (EOF()) throw new SyntaxErrorException(ErrorUnexpectedEOF("closing character quotation"));
            else throw new SyntaxErrorException(ErrorUnexpected(Peek(), "closing character quotation. Did you intend to use a string?"));
            Add(TokenType.Character, character.ToString(), new LineColumnSpan(start, _position), false);
        }

        private void ScanString(LineColumn start)
        {
            ClearCurrentToken();
            string str = string.Empty;
            while (!EOF() && Peek() != '"') str += ScanCharacterToken();
            if (Peek() == '"') Eat();
            else if (EOF()) throw new SyntaxErrorException(ErrorUnexpectedEOF("closing string quotation"));
            else throw new SyntaxErrorException(ErrorUnexpected(Peek(), "closing string quotation"));
            Add(TokenType.String, str, new LineColumnSpan(start, _position), false);
        }

        private void ScanAttribute()
        {
            while (!EOF() && char.IsLetter(Peek())) Eat();
            Add(TokenType.Attribute);
        }
        
        private void ScanSingleLineComment()
        {
            while (!EOF() && Peek() != '\n') Eat();
            Add(TokenType.SingleLineComment);
        }
        private void ScanMultiLineComment()
        {
            char c;
            while (!EOF())
            {
                c = Eat();
                if (c == '*' && Peek() == '/')
                {
                    Eat();
                    break;
                }
            }

            Add(TokenType.MultiLineComment);
        }

        #endregion
    }
}
