using System;
using System.Text.RegularExpressions;

namespace MetricsDefinition
{
    sealed class Tokenizer
    {
        static readonly Regex RegexIdentifier = new Regex(@"[_a-zA-Z\%][_a-zA-Z\%0-9]*", RegexOptions.Compiled);
        static readonly Regex RegexNumber = new Regex(@"\d+(\.\d+)?", RegexOptions.Compiled);

        private readonly string _expression;
        private int _position;

        public string LastErrorMessage { get; private set; }

        public int CurrentPostion { get { return _position; } }

        private void Reset()
        {
            LastErrorMessage = string.Empty;
        }

        public Tokenizer(string expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException();
            }

            _expression = expression;
            
        }

        public bool GetNextToken(out Token token)
        {
            Reset();

            token = null;

            while (_position < _expression.Length)
            {
                var ch = _expression[_position];
                if (char.IsWhiteSpace(ch))
                {
                    ++_position;
                    continue;
                }

                switch (ch)
                {
                    case '(' : 
                        token = new Token(TokenType.LeftParenthese, _position, _position, "(");
                        ++_position;
                        break;
                    case ')':
                        token = new Token(TokenType.RightParenthese, _position, _position, ")");
                        ++_position;
                        break;
                    case '[':
                        token = new Token(TokenType.LeftBracket, _position, _position, "[");
                        ++_position;
                        break;
                    case ']':
                        token = new Token(TokenType.RightBracket, _position, _position, "]");
                        ++_position;
                        break;
                    case ',':
                        token = new Token(TokenType.Comma, _position, _position, ",");
                        ++_position;
                        break;
                    case '.':
                        token = new Token(TokenType.Dot, _position, _position, ".");
                        ++_position;
                        break;
                    default:
                        if (char.IsLetter(ch) || ch == '_')
                        {
                            token = ParseIdentifier();
                        }
                        else if (char.IsDigit(ch))
                        {
                            token = ParseNumber();
                        }
                        else
                        {
                            LastErrorMessage = string.Format("Unrecognized character '{0}'", ch);
                            return false;
                        }
                        break;
                }

                break;
            }

            return true;
        }

        private Token ParseIdentifier()
        {
            var startPosition = _position;

            var match = RegexIdentifier.Match(_expression, startPosition);
            if (!match.Success || match.Index != _position)
            {
                throw new InvalidOperationException(
                    string.Format("Match identifier starts from {0} in expression \"{1}\" failed", _position, _expression));
            }

            _position += match.Length;

            return new Token(
                TokenType.Identifier,
                startPosition,
                _position - 1,
                match.Value);
        }

        private Token ParseNumber()
        {
            var startPosition = _position;

            var match = RegexNumber.Match(_expression, startPosition);
            if (!match.Success || match.Index != _position)
            {
                throw new InvalidOperationException(
                    string.Format("Match number starts from {0} in expression \"{1}\" failed", _position, _expression));
            }

            _position += match.Length;

            return new Token(
                TokenType.Number,
                startPosition,
                _position - 1,
                match.Value);
        }
    }
}
