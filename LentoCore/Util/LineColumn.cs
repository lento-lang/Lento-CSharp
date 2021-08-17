using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LentoCore.Util
{
    public class LineColumn
    {
        private readonly int _lineStart = 1;
        private readonly int _columnStart = 1;
        private readonly Dictionary<int, int> _lineLengths;
        public LineColumn()
        {
            _lineLengths = new Dictionary<int, int>();
            Clear();
        }

        public LineColumn Clone()
        {
            return new LineColumn()
            {
                Index = Index,
                Line = Line,
                Column = Column
            };
        }

        public LineColumn CloneAndSubtract(int offset)
        {
            LineColumn result = new LineColumn()
            {
                Index = Index - offset,
                Line = Line,
                Column = Column
            };
            // TODO: Make into expression using modulus
            for (int i = 0; i < offset; i++)
            {
                result.Column--;
                if (result.Column == _columnStart - 1)
                {
                    result.Line--;
                    result.Column = _lineLengths[result.Line];
                }
            }

            return result;
        }

        public int Index { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }

        public void NextColumn()
        {
            Index++;
            Column++;
        }

        public void NextLine()
        {
            _lineLengths.Add(Line, Column);
            Column = _columnStart;
            Line++;
        }

        public void Clear()
        {
            Index = 0;
            Line = _lineStart;
            Column = _columnStart;
            _lineLengths.Clear();
        }

        public override string ToString() => $"line {Line}, column {Column}";
    }
}
