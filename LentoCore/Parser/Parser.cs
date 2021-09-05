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
            if (!EndOfStream && (CanRead && Peek(false).Type != TokenType.EOF)) throw new ParseErrorException("Could not parse whole input");
            return new AST(rootExpressions.ToArray(), new LineColumnSpan(rootExpressions.First().Span.Start, rootExpressions.Last().Span.End));
        }

        #region Helper functions

        /// <summary>
        /// Wait until next token can be read
        /// </summary>
        /// <param name="nextExpressionExpect">Error message to show in case of EOF</param>
        private void AssureCanRead(string nextExpressionExpect)
        {
            while (!EndOfStream && !CanRead) Thread.Sleep(100);
            if (EndOfStream) throw new ParseErrorException(ErrorUnexpectedEOF(nextExpressionExpect));
        }
        private bool CanRead => _tokens.CanRead;
        private bool EndOfStream => _tokens.EndOfStream;
        private Token Peek() => Peek(true);
        private Token Peek(bool ignoreNewlines) => Peek(ignoreNewlines, 0);
        private Token Peek(bool ignoreNewlines, int offset) {
            if (EndOfStream || !_tokens.CanSeek(offset)) {
                if (ignoreNewlines)
                {
                    while (CanRead) Eat(false); // Eat the whitespace
                    return Token.EOF(null);
                }
                throw new IndexOutOfRangeException("Cannot peek in stream at offset " + offset + "! We have reached the end of the stream.");
            }
            Token ret = _tokens.Seek(offset);
            if (ignoreNewlines && ret.Type == TokenType.Newline) return Peek(true, offset + 1);
            return ret;
        }
        private Token Seek(int offset) => Seek(offset, true);
        private Token Seek(int offset, bool ignoreNewlines) {
            if (EndOfStream || !_tokens.CanSeek(offset)) throw new IndexOutOfRangeException("Cannot seek outside of the stream");
            Token ret = _tokens.Seek(offset);
            if (ignoreNewlines && ret.Type == TokenType.Newline) return Seek(offset + 1, true);
            return ret;
        }

        private Token Eat() => Eat(true);
        private Token Eat(bool ignoreNewlines) {
            if (EndOfStream) throw new IndexOutOfRangeException("Cannot read from stream! We have reached the end of the stream.");
            Token ret = _tokens.Read();
            if (ignoreNewlines && ret.Type == TokenType.Newline) return Eat(true);
            return ret;
        }

        private Token AssertNext(params TokenType[] expected) => AssertNext(Formatting.FormattableOptionsToString(expected), expected);
        private Token AssertNext(string expectedText, params TokenType[] expected)
        {
            AssureCanRead(expectedText);
            Token next = Eat();
            if (!expected.Contains(next.Type)) throw new ParseErrorException(ErrorUnexpected(next, expectedText));
            return next;
        }

        private string Error(Token errorToken, string message) => ErrorHandler.ParseError(errorToken.Position, message);
        private string ErrorUnexpected(Token errorToken, string expected) => Error(errorToken, $"Unexpected {errorToken} token. Expected {expected}");
        private string ErrorUnexpectedEOF(string expected) => Error(_tokens.Last(), $"Unexpected end of file. Expected {expected}");

        #endregion

        private Expression ParseNextToken(int minBindingPower)
        {
            AssureCanRead("expression");
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
                    if (CanRead && Peek().Type == TokenType.Dot)
                    {
                        List<Identifier> identifiers = new List<Identifier>
                        {
                            new Identifier(token.Lexeme)
                        };
                        Token identifier;
                        do
                        {
                            Eat(); // Dot
                            identifier = AssertNext("dot followed by identifier", TokenType.Identifier);
                            identifiers.Add(new Identifier(identifier.Lexeme));
                        } while (CanRead && Peek().Type == TokenType.Dot);
                        return new AtomicValue<Atoms.IdentifierDotList>(new IdentifierDotList(identifiers.ToArray()), new LineColumnSpan(token.Span.Start, identifier.Span.End));
                    }

                    Atoms.Identifier ident = new Atoms.Identifier(token.Lexeme);
                    if (CanRead)
                    {
                        if (Peek().Type == TokenType.Assign || Peek().Type == TokenType.Identifier) return ParseAssignExpression(token.Span.Start, ident);
                        if (Peek().Type == TokenType.LeftParen)
                            return ParseFunctionCallExpression(token.Span.Start, ident);
                    }
                    return new AtomicValue<Atoms.Identifier>(ident, token.Span);
                }
                case TokenType.LeftParen:
                { 
                    Expression expression = ParseExpression(0);
                    AssertNext("Right closing parenthesis", TokenType.RightParen);
                    return expression;
                }
                case TokenType.LeftBracket:
                {
                    List<Expression> expressions = ParseExpressions(TokenType.RightBracket, TokenType.Comma);
                    Token rightBracket = AssertNext("Right closing bracket", TokenType.RightBracket);
                    return new Expressions.List(new LineColumnSpan(token.Span.Start, rightBracket.Span.End), expressions);
                }
                case TokenType.LeftCurlyBracket:
                {
                    List<Expression> expressions = ParseExpressions(TokenType.RightCurlyBracket, TokenType.SemiColon, TokenType.Newline);
                    Token rightCurlyBracket = AssertNext("Right curly bracket", TokenType.RightCurlyBracket);
                    return new Expressions.Block(new LineColumnSpan(token.Span.Start, rightCurlyBracket.Span.End), expressions.ToArray());
                }
                case TokenType.TupleHashTag:
                {
                    List<Expression> expressions = ParseExpressions(TokenType.RightParen, TokenType.Comma);
                    Token rightParen = AssertNext("Right closing parenthesis", TokenType.RightParen);
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
            if (!CanRead) return null;
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
            Expression lhs = ParseNextToken(minBindingPower);
            while (true)
            {
                // Infix operators
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
            while (!EndOfStream)
            {
                AssureCanRead("expression");
                if (Peek(true).Type == endingTokenType) break;
                Expression expression = ParseExpression(0);
                expressions.Add(expression);
                if (!CanRead && endingTokenType == TokenType.EOF) break;
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

        /// <summary>
        /// Assign expressions could either be variable declaration, function declaration, or struct, tuple or list destructuring.
        /// </summary>
        /// <example>
        /// pi = 3.1415
        /// add a b = a + b
        /// [x | xs] = [1, 2, 3, 4, 5]
        /// #(ok, err, ign) = #(:Ok, :Error, :Ignore)
        /// </example>
        /// <returns>Expression for either case.</returns>
        private Expression ParseAssignExpression(LineColumn start, Atomic lhs)
        {
            if (lhs is Atoms.Identifier identifier)
            {
                // Variable or function
                if (Peek().Type == TokenType.Assign)
                {
                    // Variable
                    Eat(); // Assignment
                    Expression value = ParseExpression(0);
                    return new VariableDeclaration(new LineColumnSpan(start, value.Span.End), identifier.Name, value);
                }
                if (Peek().Type == TokenType.Identifier)
                {
                    // Function
                    Atoms.TypedIdentifier[] parameterList = ParseTypedIdentifierList(TokenType.Assign);
                    AssertNext(TokenType.Assign);
                    Expression body = ParseExpression(0); 
                    return new FunctionDeclaration(new LineColumnSpan(start, body.Span.End), identifier.Name, parameterList, body);
                }
                throw new ParseErrorException(ErrorUnexpected(Peek(), "Assignment (equal sign) or function parameter list (identifier)"));
            }
            if (lhs is Atoms.Tuple tuple)
            {
                // Destructuring
                throw new NotImplementedException(); // TODO: Implement this
            }
            if (lhs is Atoms.List list)
            {
                // Destructuring
                throw new NotImplementedException(); // TODO: Implement this
            }
            throw new NotImplementedException("Assignment is not implemented for left hand side atoms of type " + lhs.Type);
        }

        private TypedIdentifier[] ParseTypedIdentifierList(TokenType closingTokenType)
        {
            List<TypedIdentifier> identifiers = new List<TypedIdentifier>();
            while (CanRead)
            {
                Token paramType = Eat();
                if (paramType.Type != TokenType.Identifier) throw new ParseErrorException(ErrorUnexpected(paramType, "parameter identifier type"));
                AssureCanRead("parameter identifier name");
                Token paramName = Eat();
                if (paramName.Type != TokenType.Identifier) throw new ParseErrorException(ErrorUnexpected(paramName, "parameter identifier name"));

                TypedIdentifier duplicate = identifiers.Find(ti => ti.Identifier.Name == paramName.Lexeme);
                if (duplicate != null) throw new ParseErrorException(Error(paramName, $"duplicate parameter '{paramName.Lexeme}'"));
                identifiers.Add(new TypedIdentifier(GetAtomicType(paramType.Lexeme), new Identifier(paramName.Lexeme)));

                AssureCanRead("separating comma or assignment operator");
                if (Peek().Type == TokenType.Comma)
                {
                    Eat();
                    AssureCanRead("another typed function parameter");
                }
                else if (Peek().Type == closingTokenType) break;
            }

            return identifiers.ToArray();
        }

        private AtomicType GetAtomicType(string identifier)
        {
            switch (identifier)
            {
                case "any": return AtomicAnyType.BaseType;
                case "Nat": return Atoms.Integer.BaseType;
                case "Real": return Atoms.Float.BaseType;
                default: return new Atoms.AtomicType(identifier);
            }
        }
        private Expression ParseFunctionCallExpression(LineColumn spanStart, Identifier ident)
        {
            Eat(); // LeftParen
            List<Expression> arguments = ParseExpressions(TokenType.RightParen, TokenType.Comma);
            Token rightParen = AssertNext("Right closing parenthesis", TokenType.RightParen);
            return new FunctionCall(new LineColumnSpan(spanStart, rightParen.Span.End), ident, arguments.ToArray());
        }
    }
}
