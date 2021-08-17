using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Util
{
    public class TokenSpan
    {
        public LineColumn Start;
        public LineColumn End;
        public int Length => End.Index - Start.Index;

        public TokenSpan(LineColumn start, LineColumn end)
        {
            Start = start;
            End = end;
        }
    }
}
