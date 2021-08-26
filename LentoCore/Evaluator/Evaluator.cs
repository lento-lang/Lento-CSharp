using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;
using LentoCore.Expressions;
using LentoCore.Lexer;
using LentoCore.Parser;
using LentoCore.Util;

namespace LentoCore.Evaluator
{
    public class Evaluator
    {
        private readonly Tokenizer _lexer;
        private readonly Parser.Parser _parser;
        private readonly TypeChecker.TypeChecker _typeChecker;
        
        public event EventHandler<TokenizeDoneEventArgs> OnTokenizeDone;
        public event EventHandler<ParseDoneEventArgs> OnParseDone;
        public event EventHandler<EvaluationDoneEventArgs> OnEvaluationDone;

        public Evaluator()
        {
            _lexer = new Tokenizer();
            _parser = new Parser.Parser();
            _typeChecker = new TypeChecker.TypeChecker();
        }

        public Atomic EvaluateFile(Stream fileStream, bool loadStandardLibrary)
        {
            TokenStream tokens = _lexer.Tokenize(fileStream);
            OnTokenizeDone?.Invoke(this, new TokenizeDoneEventArgs(tokens));
            AST ast = _parser.Parse(tokens);
            OnParseDone?.Invoke(this, new ParseDoneEventArgs(ast));
            _typeChecker.Check(ast);
            Scope globalScope = new GlobalScope();
            if (loadStandardLibrary) StandardLibrary.StandardLibrary.Load(globalScope);
            Atomic result = ast.Evaluate(globalScope);
            OnEvaluationDone?.Invoke(this, new EvaluationDoneEventArgs(result));
            return result;
        }

        public Atomic EvaluateInput(string input, bool loadStandardLibrary)
        {
            Scope globalScope = new GlobalScope();
            if (loadStandardLibrary) StandardLibrary.StandardLibrary.Load(globalScope);
            return EvaluateInput(input, globalScope);
        }

        public Atomic EvaluateInput(string input, Scope scope)
        {
            TokenStream tokens = _lexer.Tokenize(input);
            OnTokenizeDone?.Invoke(this, new TokenizeDoneEventArgs(tokens));
            AST ast = _parser.Parse(tokens);
            OnParseDone?.Invoke(this, new ParseDoneEventArgs(ast));
            _typeChecker.Check(ast);
            Atomic result = ast.Evaluate(scope);
            OnEvaluationDone?.Invoke(this, new EvaluationDoneEventArgs(result));
            return result;
        }
    }
}
