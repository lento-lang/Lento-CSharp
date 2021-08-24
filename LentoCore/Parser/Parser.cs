using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LentoCore.Atoms;
using LentoCore.Exception;
using LentoCore.Expressions;
using LentoCore.Lexer;
using LentoCore.Util;

namespace LentoCore.Parser
{
    public class Parser
    {
        private TokenStream _tokens;

        public AST Parse(TokenStream tokens)
        {
            _tokens = tokens;
            List<Expression> rootExpressions = ParseExpressions(TokenType.EOF, TokenType.Newline, TokenType.SemiColon);
            if (!_tokens.EndOfStream && (_tokens.CanRead && Peek(false).Type != TokenType.EOF)) throw new ParseErrorException("Could not parse whole input");
            return new AST(rootExpressions.ToArray(), new LineColumnSpan(rootExpressions.First().Span.Start, rootExpressions.Last().Span.End));
        }

        #region Helper functions

        private Token Peek() => Peek(true);
        private Token Peek(bool ignoreNewlines) => Peek(ignoreNewlines, 0);
        private Token Peek(bool ignoreNewlines, int offset) {
            if (_tokens.EndOfStream || !_tokens.CanSeek(offset)) throw new IndexOutOfRangeException("Cannot peek in stream at offset " + offset + "! We have reached the end of the stream.");
            Token ret = _tokens.Seek(offset);
            if (ignoreNewlines && ret.Type == TokenType.Newline) return Peek(true, offset + 1);
            return ret;
        }

        private Token Eat() => Eat(true);
        private Token Eat(bool ignoreNewlines) {
            if (_tokens.EndOfStream) throw new IndexOutOfRangeException("Cannot read from stream! We have reached the end of the stream.");
            Token ret = _tokens.Read();
            if (ignoreNewlines && ret.Type == TokenType.Newline) return Eat(true);
            return ret;
        }

        private string Error(Token errorToken, string message) => ErrorHandler.SyntaxError(errorToken.Position, message);
        private string ErrorUnexpected(Token errorToken, string expected) => Error(errorToken, $"Unexpected {errorToken} token. Expected {expected}");
        private string ErrorUnexpectedEOF(string expected) => Error(_tokens.Last(), $"Unexpected end of file. Expected {expected}");

        #endregion

        private Expression ParseNextToken(int minBindingPower)
        {
            Token token = Eat();
            switch (token.Type)
            {
                // Values
                case TokenType.Atom: return new AtomicValue<Atoms.Atom>(new Atoms.Atom(token.Lexeme), token.Span);
                case TokenType.Boolean: return new AtomicValue<Atoms.Boolean>(new Atoms.Boolean(token.Lexeme == "true"), token.Span);
                case TokenType.Integer: return new AtomicValue<Atoms.Integer>(new Atoms.Integer(int.Parse(token.Lexeme)), token.Span);
                case TokenType.Float: return new AtomicValue<Atoms.Float>(new Atoms.Float(float.Parse(token.Lexeme.Replace('.',','))), token.Span);
                case TokenType.String: return new AtomicValue<Atoms.String>(new Atoms.String(token.Lexeme), token.Span);
                case TokenType.Character:
                {
                    if (token.Lexeme.Length != 1) throw new ParseErrorException(Error(token, "Invalid character! Must be a single character value. Did you intend to use a string?"));
                    return new AtomicValue<Atoms.Character>(new Atoms.Character(token.Lexeme[0]), token.Span);
                }
                case TokenType.Identifier:
                {
                    if (_tokens.CanRead && Peek().Type == TokenType.Dot)
                    {
                        List<Identifier> identifiers = new List<Identifier>
                        {
                            new Identifier(token.Lexeme)
                        };
                        Token identifier;
                        do
                        {
                            Eat(); // Dot
                            identifier = Eat();
                            if (identifier.Type == TokenType.Identifier) identifiers.Add(new Identifier(identifier.Lexeme));
                            else throw new ParseErrorException(ErrorUnexpected(identifier, "identifier"));
                        } while (_tokens.CanRead && Peek().Type == TokenType.Dot);
                        return new AtomicValue<Atoms.IdentifierDotList>(new IdentifierDotList(identifiers.ToArray()), new LineColumnSpan(token.Span.Start, identifier.Span.End));
                    }
                    return new AtomicValue<Atoms.Identifier>(new Atoms.Identifier(token.Lexeme), token.Span);
                }
                case TokenType.LeftParen:
                {
                    List<Expression> expressions = ParseExpressions(TokenType.RightParen, TokenType.Comma);
                    Token rightParen = Eat();
                    if (expressions.Count > 0) return new Expressions.Tuple(new LineColumnSpan(token.Span.Start, rightParen.Span.End), expressions.ToArray());
                    return new Expressions.AtomicValue<Atoms.Unit>(new Unit(), new LineColumnSpan(token.Span.Start, rightParen.Span.End));
                }
                // Prefix operators
                case TokenType.Reference:
                case TokenType.Subtraction:
                case TokenType.Not:
                {
                    PrefixOperator op = GetPrefixOperator(token);
                    Expression rhs = ParseExpression(GetPrefixOperatorBindingPower(op).Right);
                    return new Expressions.Prefix(op, rhs, new LineColumnSpan(token.Span.Start, rhs.Span.End));
                }
                default:
                {
                    if (token.Type == TokenType.EOF || token.Type == TokenType.Newline) return new AtomicValue<Atoms.Unit>(new Atoms.Unit(), token.Span);
                    throw new ParseErrorException(ErrorUnexpected(token, "expression"));
                }
            }
        }

        private BinaryOperator? PeekNextBinaryOperator()
        {
            switch (Peek().Type)
            {
                case TokenType.Addition: return BinaryOperator.Add;
                case TokenType.Subtraction: return BinaryOperator.Subtract;
                case TokenType.Multiplication: return BinaryOperator.Multiply;
                case TokenType.Division: return BinaryOperator.Divide;
                case TokenType.Modulus: return BinaryOperator.Modulus;
                case TokenType.Equals: return BinaryOperator.Equals;
                case TokenType.NotEquals: return BinaryOperator.NotEquals;
                case TokenType.LessThan: return BinaryOperator.LessThan;
                case TokenType.GreaterThan: return BinaryOperator.GreaterThan;
                case TokenType.LessThanEquals: return BinaryOperator.LessThanEquals;
                case TokenType.GreaterThanEquals: return BinaryOperator.GreaterThanEquals;
                case TokenType.And: return BinaryOperator.And;
                case TokenType.Or: return BinaryOperator.Or;
                case TokenType.Exclude: return BinaryOperator.Exclude;
                default: return null;
            }
        }
        private InfixBindingPower GetBinaryOperatorBindingPower(BinaryOperator op)
        {
            switch (op) {
                case BinaryOperator.Add:
                case BinaryOperator.Subtract: return new InfixBindingPower(4, 5);
                case BinaryOperator.Multiply:
                case BinaryOperator.Divide:
                case BinaryOperator.Modulus: return new InfixBindingPower(6, 7);
                case BinaryOperator.Equals:
                case BinaryOperator.NotEquals:
                case BinaryOperator.LessThan:
                case BinaryOperator.GreaterThan:
                case BinaryOperator.LessThanEquals:
                case BinaryOperator.GreaterThanEquals: return new InfixBindingPower(2, 3);
                case BinaryOperator.And:
                case BinaryOperator.Or:
                case BinaryOperator.Exclude: return new InfixBindingPower(0, 1);
                default: throw new ArgumentException($"Failed to get binding power for operator! {op} does not match any of the current alternatives in GetBinaryOperatorBindingPower");
            }
        }
        private PrefixOperator GetPrefixOperator(Token token)
        {
            switch (token.Type) {
                case TokenType.Not: return PrefixOperator.Not;
                case TokenType.Subtraction: return PrefixOperator.Negative;
                case TokenType.Reference: return PrefixOperator.Referenced;
                default: throw new ArgumentException("Unreachable hopefully");
            }
        }
        private PrefixBindingPower GetPrefixOperatorBindingPower(PrefixOperator op)
        {
            switch (op)
            {
                case PrefixOperator.Not:
                case PrefixOperator.Referenced:
                case PrefixOperator.Negative: return new PrefixBindingPower(10);
                default: throw new ArgumentException("Unreachable hopefully");
            }
        }
        private Expression ParseExpression(int minBindingPower)
        {
            if (_tokens.EndOfStream) throw new ParseErrorException(ErrorUnexpectedEOF("expression"));
            Expression lhs = ParseNextToken(minBindingPower);
            while (true)
            {
                // Infix operators
                if (_tokens.EndOfStream) break;
                if (!_tokens.CanRead)
                {
                    Thread.Sleep(100);
                    continue;
                }
                BinaryOperator? op = PeekNextBinaryOperator();
                if (op == null) break;
                InfixBindingPower bindingPower = GetBinaryOperatorBindingPower((BinaryOperator)op);
                if (bindingPower.Left < minBindingPower) break;
                Eat(); // The binary operator
                Expression rhs = ParseExpression(bindingPower.Right); // Right hand side of the binary operator
                lhs = new Expressions.Binary((BinaryOperator)op, lhs, rhs, new LineColumnSpan(lhs.Span.Start, rhs.Span.End)); // Aggregate downwards
            }
            return lhs;
        }
        private List<Expression> ParseExpressions(TokenType endingTokenType, params TokenType[] expressionDelimiterTokenTypes)
        {
            List<Expression> expressions = new List<Expression>();
            while (!_tokens.EndOfStream)
            {
                if (!_tokens.CanRead)
                {
                    Thread.Sleep(100);
                    continue;
                }

                if (Peek(true).Type == endingTokenType) break;
                Expression expression = ParseExpression(0);
                expressions.Add(expression);
                if (_tokens.EndOfStream) break;
                while (!_tokens.EndOfStream && !_tokens.CanRead) Thread.Sleep(100);
                Token next = Peek(false);
                if (next.Type == endingTokenType) break;
                if (expressionDelimiterTokenTypes.Contains(next.Type)) Eat(false); // Eat the separating token
                else throw new ParseErrorException(ErrorUnexpected(next,
                    "expression" + (expressionDelimiterTokenTypes.Length > 0
                        ? " separated by a " + Formatting.FormattableOptionsToString(expressionDelimiterTokenTypes)
                        : string.Empty)
                ));
            }
            return expressions;
        }
    }
}
