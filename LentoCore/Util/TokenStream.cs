using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Lexer;

namespace LentoCore.Util
{
    public class TokenStream : IEnumerable<Token>
    {
        private readonly List<Token> _tokens;
        private bool _endOfStream;
        private bool _endOfStreamSetLocked;
        public TokenStream()
        {
            _tokens = new List<Token>();
            Position = 0;
            _endOfStreamSetLocked = false;
        }
        
        public int Position;
        /// <summary>
        /// Is true when no tokens are left in the stream and the end of stream has been signaled by the TokenStream writer by calling WriteEndOfStream.
        /// </summary>
        public bool EndOfStream => _endOfStream && !CanRead;
        /// <summary>
        /// The amount of tokens that are left in stream available to be read.
        /// </summary>
        public long LeftInStream => _tokens.Count - Position;
        public bool CanRead => LeftInStream > 0;
        public bool CanWrite => _tokens != null;
        public bool CanSeek(int offset) => Position + offset < _tokens.Count;

        /// <summary>
        /// Consume the next token in the stream and increment Position.
        /// Use CanRead to safety check before invoking this method.
        /// </summary>
        /// <returns>The next token</returns>
        public Token Read() => _tokens[Position++];
        /// <summary>
        /// Write a new token into the stream.
        /// Use CanWrite to safety check before invoking this method.
        /// </summary>
        /// <param name="token">The new token</param>
        public void Write(Token token) => _tokens.Add(token);
        /// <summary>
        /// Signal the end of stream.
        /// </summary>
        public void WriteEndOfStream()
        {
            if (!_endOfStreamSetLocked)
            {
                _endOfStream = true;
                _endOfStreamSetLocked = true;
            }
            else throw new AccessViolationException("End of stream has already been set!");
        }
        /// <summary>
        /// Peek at a token at a specific offset in the stream relative to the current Position.
        /// Use CanSeek to safety check before invoking this method.
        /// </summary>
        /// <param name="offset">Seek offset</param>
        /// <returns>The token at that offset</returns>
        public Token Seek(int offset) => _tokens[Position + offset];
        /// <summary>
        /// Peek at the next token in the stream without consuming it.
        /// </summary>
        /// <returns>The next token</returns>
        public Token Peek() => Seek(0);
        /// <summary>
        /// Enumerate through all tokens without affecting the current Position until the end of the stream.
        /// This can be done anytime any amount of time.
        /// </summary>
        /// <returns>An enumerator that iterates through all tokens in the stream</returns>
        public IEnumerator<Token> GetEnumerator() => _tokens.GetEnumerator();
        /// <summary>
        /// Enumerate through all tokens without affecting the current Position until the end of the stream.
        /// This can be done anytime any amount of time.
        /// </summary>
        /// <returns>An enumerator that iterates through all tokens in the stream</returns>
        IEnumerator IEnumerable.GetEnumerator() => _tokens.GetEnumerator();
    }
}
