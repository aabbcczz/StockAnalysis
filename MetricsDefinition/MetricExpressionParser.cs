using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    internal sealed class MetricExpressionParser
    {
        Tokenizer _tokenizer;

        Token _lastToken = null;

        bool _noMoreToken = false;

        public MetricExpressionParser(string expression)
        {
            _tokenizer = new Tokenizer(expression);
        }

        private bool GetNextToken(out Token token, out string errorMessage)
        {
            token = null;

            if (_noMoreToken)
            {
                token = _lastToken;
                _lastToken = null;
                return true;
            }
            else
            {
                if (_lastToken == null)
                {
                    if (!_tokenizer.GetNextToken(out _lastToken, out errorMessage))
                    {
                        errorMessage = "Parse token failed: " + errorMessage;
                        return false;
                    }

                    if (_lastToken == null)
                    {
                        _noMoreToken = true;
                    }

                    token = _lastToken;
                    return true;
                }
                else
                {

                }
            }

        }
        public MetricExpression Parse(out string errorMessage)
        {
            errorMessage = string.Empty;

            Token token;

            while (true)
            {
                if (!_tokenizer.GetNextToken(out token, out errorMessage))
                {
                    errorMessage = "Parse token failed: " + errorMessage;
                    return null;
                }

                if (token == null)
                {
                    // no more token
                    break;
                }
            }

            return null;
        }

        private bool Expect(TokenType expectedType, out Token token, out string errorMessage)
        {
            if (!_tokenizer.GetNextToken(out token, out errorMessage))
            {
                errorMessage = "Parse token failed: " + errorMessage;
                return false;
            }

            if (token.Type != expectedType)
            {
                errorMessage = string.Format(
                    "Expect token {0}, but get token {1} at position {2}", 
                    expectedType, 
                    token.Type, 
                    token.StartPosition);

                return false;
            }
        }
    }
}
