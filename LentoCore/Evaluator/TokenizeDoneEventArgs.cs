using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Util;

namespace LentoCore.Evaluator
{
    public class TokenizeDoneEventArgs : EventArgs
    {
        public TokenStream TokenStream { get; }

        public TokenizeDoneEventArgs(TokenStream tokenStream)
        {
            TokenStream = tokenStream;
        }
    }
}
