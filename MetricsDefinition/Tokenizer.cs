using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    sealed class Tokenizer
    {
        static Regex RegexIdentifier = new Regex(@"[_a-zA-Z][_a-zA-Z0-9]*", RegexOptions.Compiled);
        static Regex RegexNumber = new Regex(@"\d+(\.\d+)?", RegexOptions.Compiled);

        private string _expression;
        private int _position = 0;

        public int CurrentPostion { get { return _position; } }

        public Tokenizer(string expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException();
            }

            _expression = expression;
            
        }

        public bool GetNextToken(out Token token, out string errorMessage)
        {
            errorMessage = string.Empty;

            token = null;

            while (_position < _expression.Length)
            {
                char ch = _expression[_position];
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
                            errorMessage = string.Format("Unrecognized character '{0}'", ch);
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
            int startPosition = _position;

            Match match = RegexIdentifier.Match(_expression, startPosition);
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
            int startPosition = _position;

            Match match = RegexNumber.Match(_expression, startPosition);
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
