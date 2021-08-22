using LentoCore.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.CodeGenerator
{
    public class CodeGenerator
    {
        private readonly AST ast;
        private readonly TargetLanguage language;

        public CodeGenerator(AST ast, TargetLanguage language)
        {
            this.ast = ast;
            this.language = language;
        }

        public void GenerateFile(string file)
        {
            using StreamWriter fileStream = new(file);
            foreach(string fragment in LanguageFragments()) fileStream.WriteLine(fragment);
        }
        public string Generate()
        {
            string result = string.Empty;
            foreach (string fragment in LanguageFragments()) result += fragment + '\n';
            return result.TrimEnd();
        }

        private IEnumerable<string> LanguageFragments()
        {
            foreach (Expressions.Expression expression in ast.CompilationUnit)
            {
                yield return language.GenerateNode(expression);
            }
            yield break;
        }
    }
}
