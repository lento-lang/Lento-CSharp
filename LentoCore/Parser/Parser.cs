﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LentoCore.Atoms;
using LentoCore.Atoms.Numerical;
using LentoCore.Atoms.Types;
using LentoCore.Exception;
using LentoCore.Expressions;
using LentoCore.Lexer;
using LentoCore.Util;

namespace LentoCore.Parser
{
    public class Parser
    {
        private class FunctionInfo
        {
            public int MaxParameters;
            public readonly bool SingleValue;
            /// <summary>
            /// Function information
            /// </summary>
            /// <param name="maxParameters">Function parameters of largest variation signature</param>
            /// <param name="singleValue">Only accept identifiers and value types, no larger expressions. Function can only accept one canonical expression.</param>
            public FunctionInfo(int maxParameters, bool singleValue = false)
            {
                MaxParameters = maxParameters;
                SingleValue = singleValue;
                if (singleValue && maxParameters != 1)
                    throw new ArgumentOutOfRangeException(nameof(maxParameters),
                        "maxParameters must be 1 if function expects a single value expression");
            }
        }

        private readonly Dictionary<string, FunctionInfo> _parsedFunctions = new Dictionary<string, FunctionInfo>();
        public void AddParseIdentifiedFunction(string name, int parameters, bool singleValue = false)
        {
            if (_parsedFunctions.ContainsKey(name) && _parsedFunctions[name].MaxParameters < parameters) _parsedFunctions[name].MaxParameters = parameters;
            _parsedFunctions.Add(name, new FunctionInfo(parameters, singleValue));
        }

        private TokenStream _tokens;

        public AST Parse(TokenStream tokens)
        {
            _tokens = tokens;
            List<Expression> rootExpressions = ParseExpressions(TokenType.EOF, TokenType.Newline, TokenType.SemiColon);
            LineColumn first = new LineColumn();
            LineColumn last = null;
            if (rootExpressions.Count > 0)
            {
                first = rootExpressions.First().Span.Start;
                last = rootExpressions.Last().Span.End;
            }
            if (!EndOfStream && CanRead)
            {
                Token n = Peek(false, false);
                last ??= n.Span.End;
                TokenType t = n.Type;
                if (t != TokenType.EOF
                    && t != TokenType.Newline
                    && t != TokenType.MultiLineComment
                    && t != TokenType.SingleLineComment) throw new ParseErrorException("Could not parse whole input");
            }
            return new AST(rootExpressions.ToArray(), new LineColumnSpan(first, last));
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
        private Token Peek() => Peek(true, true);
        private Token Peek(bool ignoreNewlines, bool ignoreComments) {
            if (EndOfStream || !_tokens.CanSeek(0)) {
                if (ignoreNewlines)
                {
                    while (CanRead) Eat(false, false); // Eat the whitespace
                    return Token.EOF(null);
                }
                throw new IndexOutOfRangeException("We have reached the end of the stream");
            }

            return Seek(0, ignoreNewlines, ignoreComments);
        }
        private Token Seek(int offset) => Seek(offset, true, true);
        private Token Seek(int offset, bool ignoreNewlines, bool ignoreComments) {
            if (EndOfStream || !_tokens.CanSeek(offset)) throw new IndexOutOfRangeException("Cannot seek outside of the stream");
            Token ret = _tokens.Seek(offset);
            if (ignoreNewlines && ret.Type == TokenType.Newline)
            {
                if (!_tokens.CanSeek(offset + 1)) return Token.EOF(ret.Position);
                return Seek(offset + 1, true, ignoreComments);
            }

            if (ignoreComments && (ret.Type == TokenType.MultiLineComment || ret.Type == TokenType.SingleLineComment))
            {
                if (!_tokens.CanSeek(offset + 1)) return Token.EOF(ret.Position);
                return Seek(offset + 1, ignoreNewlines, true);
            }
            return ret;
        }
        // TODO: Add some check that makes SeekPattern halt if it stumbles upon some token that aren't allowed
        private bool SeekPattern(params TokenType[] pattern)
        {
            if (pattern.Length == 0) throw new ArgumentException("No TokenTypes in pattern", nameof(pattern));
            for(int offset = 0; _tokens.CanSeek(offset + pattern.Length); offset++)
            {
                if (_tokens.Seek(offset).Type != pattern[0]) continue;
                // Token matches the first type in the pattern, if this does not match fail
                bool allMatch = true;
                for (int i = 1; i < pattern.Length; i++)
                {
                    if (_tokens.Seek(offset + i).Type != pattern[i])
                    {
                        allMatch = false;
                        break;
                    }
                }
                if (allMatch) return true;
                // else continue
            }
            return false;
        }

        private Token Eat() => Eat(true, true);
        private Token Eat(bool ignoreNewlines, bool ignoreComments) {
            if (EndOfStream) throw new IndexOutOfRangeException("Cannot read from stream! We have reached the end of the stream.");
            Token ret = _tokens.Read();
            if (ignoreNewlines && ret.Type == TokenType.Newline) {
                if (!_tokens.CanRead) return Token.EOF(ret.Position);
                return Eat(true, ignoreComments);
            }
            if (ignoreComments && (ret.Type == TokenType.MultiLineComment || ret.Type == TokenType.SingleLineComment)) {
                if (!_tokens.CanRead) return Token.EOF(ret.Position);
                return Eat(ignoreNewlines, true);
            }
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

        private Expression ParseNextToken(int minBindingPower, bool allowEOF = false)
        {
            AssureCanRead("expression");
            Token token = Eat();
            switch (token.Type)
            {
                // Values
                case TokenType.Atom: return new AtomicValue<Atoms.Atom>(new Atoms.Atom(token.Lexeme), token.Span);
                case TokenType.Boolean: return new AtomicValue<Atoms.Boolean>(new Atoms.Boolean(token.Lexeme == "true"), token.Span);
                case TokenType.Integer:
                    {
                        if (int.TryParse(token.Lexeme, out int intValue))
                            return new AtomicValue<Integer>(new Integer(intValue), token.Span);
                        if (long.TryParse(token.Lexeme, out long longValue))
                            return new AtomicValue<Long>(new Long(longValue), token.Span);
                        if (System.Numerics.BigInteger.TryParse(token.Lexeme, out System.Numerics.BigInteger bigIntegerValue))
                            return new AtomicValue<LentoCore.Atoms.Numerical.BigInteger>(new Atoms.Numerical.BigInteger(bigIntegerValue), token.Span);
                        throw new ParseErrorException(ErrorHandler.ParseError(token.Position, "Integer value is too large"));
                    }
                case TokenType.Float:
                {
                    if (float.TryParse(token.Lexeme.Replace('.',','), out float floatValue) && token.Lexeme.TrimStart('0').TrimEnd('0').Length < 7 + 2)
                        return new AtomicValue<Float>(new Float(floatValue), token.Span);
                    if (double.TryParse(token.Lexeme.Replace('.',','), out double doubleValue))
                        return new AtomicValue<Atoms.Numerical.Double>(new Atoms.Numerical.Double(doubleValue), token.Span);
                    throw new ParseErrorException(ErrorHandler.ParseError(token.Position, "Float value is invalid"));
                }
                case TokenType.String: return new AtomicValue<Atoms.String>(new Atoms.String(token.Lexeme), token.Span);
                case TokenType.Character:
                {
                    if (token.Lexeme.Length != 1) throw new ParseErrorException(Error(token, "Character is invalid! Did you intend to use a string?"));
                    return new AtomicValue<Atoms.Character>(new Atoms.Character(token.Lexeme[0]), token.Span);
                }
                case TokenType.Identifier:
                {
                    Atoms.Identifier ident = new Atoms.Identifier(token.Lexeme);
                    if (CanRead && Peek().Type == TokenType.Dot)
                    {
                        List<Identifier> identifiers = new List<Identifier>{ ident };
                        Token identifier;
                        do
                        {
                            Eat(); // Dot
                            identifier = AssertNext("dot followed by identifier", TokenType.Identifier);
                            identifiers.Add(new Identifier(identifier.Lexeme));
                        } while (CanRead && Peek().Type == TokenType.Dot);
                        return new AtomicValue<Atoms.IdentifierDotList>(new IdentifierDotList(identifiers.ToArray()), new LineColumnSpan(token.Span.Start, identifier.Span.End));
                    }
                    // minBindingPower == 0 makes variable assignments and function declarations not allowed inside expressions.
                    if (CanRead && Peek().Type == TokenType.Assign) return ParseAssignExpression(token.Span.Start, ident);
                    if (CanRead && SeekPattern(TokenType.Identifier, TokenType.Assign)) return ParseNoParenthesisedFunctionDeclaration(token.Span.Start, ident);
                    if (CanRead && Peek().Type == TokenType.LeftParen && SeekPattern(TokenType.RightParen, TokenType.Assign)) return ParseParenthesisedFunctionDeclaration(token.Span.Start, ident);
                    if (CanRead && Peek().Type == TokenType.LeftParen) return ParseParenthesisedFunctionCall(token.Span.Start, ident);
                    if (CanRead && TryParseNoParenthesisedFunctionCall(token.Span.Start, ident, out Expression result)) return result;
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
                    if (allowEOF && (token.Type == TokenType.EOF || token.Type == TokenType.Newline)) return new AtomicValue<Atoms.Unit>(new Atoms.Unit(), token.Span);
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
        private Expression ParseExpression(int minBindingPower, bool allowEOF = false)
        {
            Expression lhs = ParseNextToken(minBindingPower, allowEOF);
            while (true)
            {
                // Infix operators
                BinaryOperator? op = PeekNextBinaryOperator();
                if (op == null) break;
                InfixBindingPower bindingPower = GetBinaryOperatorBindingPower((BinaryOperator)op);
                if (bindingPower.Left < minBindingPower) break;
                Eat(); // The binary operator
                Expression rhs = ParseExpression(bindingPower.Right, allowEOF); // Right hand side of the binary operator
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
                if (Peek().Type == endingTokenType) break;
                Expression expression = ParseExpression(0, false);
                expressions.Add(expression);
                if (!CanRead && endingTokenType == TokenType.EOF) break;
                Token next = Peek(false, true);
                if (next.Type == endingTokenType) break;
                if (expressionDelimiterTokenTypes.Contains(next.Type)) Eat(false, true); // Eat the separating token
                else throw new ParseErrorException(ErrorUnexpected(next,
                    "expression" + (expressionDelimiterTokenTypes.Length > 0
                        ? " separated by a " + Formatting.FormattableOptionsToString(expressionDelimiterTokenTypes)
                        : string.Empty)
                ));
            }
            return expressions;
        }
        /// <summary>
        /// Expressions not separated by any delmimitering tokens and not ending with any specific tokens
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private List<Expression> ParseExpressions(int count)
        {
            List<Expression> expressions = new List<Expression>(count);
            while (!EndOfStream)
            {
                if (expressions.Count >= count || Peek().Type == TokenType.RightParen) break; // '(func a b) c' will only pass a b to func
                AssureCanRead($"{count - expressions.Count} more expressions");
                Expression expression = ParseExpression(0);
                expressions.Add(expression);
            }
            return expressions;
        }

        private TypedIdentifier[] ParseTypedIdentifierList(TokenType closingTokenType, bool delmitingComma)
        {
            List<TypedIdentifier> identifiers = new List<TypedIdentifier>();
            while (CanRead)
            {
                if (Peek().Type == closingTokenType) break;
                Token paramType = Eat();
                if (paramType.Type != TokenType.Identifier) throw new ParseErrorException(ErrorUnexpected(paramType, "parameter identifier type"));
                string paramTypeName = paramType.Lexeme;
                if (Peek().Type == TokenType.LessThan)
                {
                    paramTypeName += Eat().Lexeme; // <
                    // Parse generic type parameters
                    while (!EndOfStream && Peek().Type != TokenType.GreaterThan)
                    {
                        Token t = Eat();
                        if (!(t.Type == TokenType.Identifier || t.Type == TokenType.Integer)) throw new ParseErrorException(ErrorUnexpected(t, $"invalid generic type argument '{t.Lexeme}'"));
                        paramTypeName += t.Lexeme;
                    }
                    paramTypeName += Eat().Lexeme; // >
                }

                AssureCanRead("parameter identifier name");
                Token paramName = Eat();
                if (paramName.Type != TokenType.Identifier) throw new ParseErrorException(ErrorUnexpected(paramName, "parameter identifier name"));

                TypedIdentifier duplicate = identifiers.Find(ti => ti.Identifier.Name == paramName.Lexeme);
                if (duplicate != null) throw new ParseErrorException(Error(paramName, $"duplicate parameter '{paramName.Lexeme}'"));
                identifiers.Add(new TypedIdentifier(GetAtomicType(paramTypeName), new Identifier(paramName.Lexeme)));

                if (Peek().Type == closingTokenType) break;
                if (delmitingComma)
                {
                    AssureCanRead("separating comma");
                    if (Peek().Type != TokenType.Comma) throw new ParseErrorException(ErrorUnexpected(Peek(), "separating comma"));
                    Eat();
                }
                AssureCanRead("another typed function parameter");
            }

            return identifiers.ToArray();
        }

        private AtomicType GetAtomicType(string identifier)
        {
            switch (identifier)
            {
                case "any": return AnyType.BaseType;
                case "Nat": return Integer.BaseType;
                case "Real": return Float.BaseType;
                default: return new AtomicType(identifier);
            }
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
                throw new ParseErrorException(ErrorUnexpected(Peek(), "Assignment (equal sign)"));
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

        private Expression ParseParenthesisedFunctionDeclaration(LineColumn start, Identifier ident)
        {
            // Function declaration using 'name(type param) = body' notation
            Eat(); // LeftParen
            Atoms.TypedIdentifier[] parameterList = ParseTypedIdentifierList(TokenType.RightParen, true);
            AssertNext(TokenType.RightParen);
            AssertNext(TokenType.Assign);
            Expression body = ParseExpression(0);
            if (_parsedFunctions.ContainsKey(ident.Name)) _parsedFunctions[ident.Name].MaxParameters = Math.Max(_parsedFunctions[ident.Name].MaxParameters, parameterList.Length);
            else _parsedFunctions.Add(ident.Name, new FunctionInfo(parameterList.Length));
            return new FunctionDeclaration(new LineColumnSpan(start, body.Span.End), ident.Name, parameterList, body);
        }

        private Expression ParseNoParenthesisedFunctionDeclaration(LineColumn start, Identifier ident)
        {
            // Function declaration using 'name type param = body' notation
            Atoms.TypedIdentifier[] parameterList = ParseTypedIdentifierList(TokenType.Assign, false);
            AssertNext(TokenType.Assign);
            Expression body = ParseExpression(0);
            if (_parsedFunctions.ContainsKey(ident.Name)) _parsedFunctions[ident.Name].MaxParameters = Math.Max(_parsedFunctions[ident.Name].MaxParameters, parameterList.Length);
            else _parsedFunctions.Add(ident.Name, new FunctionInfo(parameterList.Length));
            return new FunctionDeclaration(new LineColumnSpan(start, body.Span.End), ident.Name, parameterList, body);
        }
        private Expression ParseParenthesisedFunctionCall(LineColumn spanStart, Identifier ident)
        {
            // Function call using 'name(arg1, arg2, ...)' notation
            Eat(); // LeftParen
            List<Expression> arguments = ParseExpressions(TokenType.RightParen, TokenType.Comma);
            Token rightParen = AssertNext("Right closing parenthesis", TokenType.RightParen);
            return new FunctionCall(new LineColumnSpan(spanStart, rightParen.Span.End), ident, arguments.ToArray());
        }
        private bool TryParseNoParenthesisedFunctionCall(LineColumn spanStart, Identifier ident, out Expression result)
        {
            // Function call using 'name arg1 arg2 ...' notation
            if (!_parsedFunctions.ContainsKey(ident.Name))
            {
                result = default;
                return false;
            }
            FunctionInfo identData = _parsedFunctions[ident.Name];
            var arguments = identData.SingleValue ? new List<Expression>{ ParseNextToken(0) } : ParseExpressions(identData.MaxParameters);
            if (arguments.Count == 0) result = new AtomicValue<Identifier>(ident, new LineColumnSpan(spanStart, spanStart.CloneAndAdd(ident.Name.Length)));
            else result = new FunctionCall(new LineColumnSpan(spanStart, arguments.Last().Span.End), ident, arguments.ToArray());
            return true;
        }
    }
}
