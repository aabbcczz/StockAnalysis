using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MetricsDefinition
{
    internal sealed class MetricExpressionParser
    {
        Queue<Token> _tokens = new Queue<Token>();

        private Token GetNextToken()
        {
            Token token = null;

            if (_tokens.Count > 0)
            {
                token = _tokens.Dequeue();
            }

            return token;
        }

        private Token PeekNextToken()
        {
            Token token = null;

            if (_tokens.Count > 0)
            {
                token = _tokens.Peek();
            }

            return token;
        }

        public MetricExpression Parse(string expression, out string errorMessage)
        {
            _tokens.Clear();

            // parse all tokens out and put it in queue.
            Tokenizer _tokenizer = new Tokenizer(expression);

            Token token = null;
            errorMessage = string.Empty;

            do
            {
                if (!_tokenizer.GetNextToken(out token, out errorMessage))
                {
                    errorMessage = "Parse token failed: " + errorMessage;
                    return null;
                }

                if (token != null)
                {
                    _tokens.Enqueue(token);
                }
            } while (token != null);


            // use recursive descending parsing
            MetricExpression metric = Parse(out errorMessage);

            if (metric != null)
            {
                token = PeekNextToken();
                if (token != null)
                {
                    errorMessage = string.Format("Unexpected token {0} left after parsing at {1}", token.Type, token.StartPosition);
                    return null;
                }
            }

            return metric;
        }

        private MetricExpression Parse(out string errorMessage)
        {
            errorMessage = string.Empty;

            StandaloneMetric metric = ParseMetric(out errorMessage);
            if (metric == null)
            {
                return null;
            }

            Token token = PeekNextToken();

            if (token != null && token.Type == TokenType.LeftParenthese)
            {
                MetricExpression expression = Parse(out errorMessage);

                if (expression != null)
                {
                    if (!Expect(TokenType.RightParenthese, out token, out errorMessage))
                    {
                        return null;
                    }

                    CallOperator call = new CallOperator(metric, expression);
                    return call;
                }

                return null;
            }
            else
            {
                return metric;
            }
        }

        private StandaloneMetric ParseMetric(out string errorMessage)
        {
            errorMessage = string.Empty;

            Token token;

            // Get name
            if (!Expect(TokenType.Identifier, out token, out errorMessage))
            {
                return null;
            }

            string name = token.Value;
            string[] parameters = null;

            Token nextToken = PeekNextToken();

            if (nextToken != null || nextToken.Type == TokenType.LeftBracket)
            {
                GetNextToken();

                parameters = ParseParameters(out errorMessage);

                if (parameters == null)
                {
                    return null;
                }

                if (!Expect(TokenType.RightBracket, out token, out errorMessage))
                {
                    return null;
                }
            }

            // check if name is valid metric
            if (!MetricEvaluator.NameToMetricMap.ContainsKey(name))
            {
                errorMessage = string.Format("Undefined metric name {0}", name);
                return null;
            }

            Type metricType = MetricEvaluator.NameToMetricMap[name];
            StandaloneMetric metric = null;

            try
            {
                var constructors = metricType.FindMembers(MemberTypes.Constructor, BindingFlags.Public, null, null);
                foreach (var constructor in constructors)
                {
                    Type[] parameterTypes = ((MethodInfo)constructor).GetParameters().Select(pi => pi.ParameterType).ToArray();
                    if (parameterTypes.Length != parameters.Length)
                    {
                        continue;
                    }

                    // try to convert parameters to the expected type
                    object[] objects = new object[parameterTypes.Length];

                    try
                    {
                        for (int i = 0; i < parameterTypes.Length; ++i)
                        {
                            objects[i] = Convert.ChangeType(parameters[i], parameterTypes[i]);
                        }
                    }
                    catch
                    {
                        continue;
                    }

                    // now try to create instance with converted parameters
                    metric = new StandaloneMetric((IMetric)Activator.CreateInstance(metricType, objects));
                    break;
                }

                if (metric == null)
                {
                    errorMessage = string.Format(
                        "Can't find proper constructor for metric {0} that can be initialized by parameters {1}",
                        metricType.Name,
                        string.Join(",", parameters));

                    return null;
                }
            }
            catch (Exception ex)
            {
                errorMessage = string.Format(
                    "Create metric object {0} with parameter {1} failed. Exception {2}",
                    metricType.Name,
                    string.Join(",", parameters),
                    ex.ToString());

                return null;
            }

            return metric;
        }

        /// <summary>
        /// Parase parameters
        /// </summary>
        /// <param name="errorMessage">error message</param>
        /// <returns>
        /// null: failed.
        /// empty array : no parameter
        /// otherwise : parameters
        /// </returns>
        private string[] ParseParameters(out string errorMessage)
        {
            errorMessage = string.Empty;

            Token token = null;
            List<string> parameters = new List<string>();

            do
            {
                token = PeekNextToken();
                if (token == null)
                {
                    errorMessage = "Expect ']'";
                    return null;
                }

                if (token.Type == TokenType.RightBracket)
                {
                    break;
                }

                if (!Expect(TokenType.Number, out token, out errorMessage))
                {
                    return null;
                }

                parameters.Add(token.Value);

                token = PeekNextToken();
                if (token != null)
                {
                    if (token.Type == TokenType.Comma)
                    { 
                        GetNextToken();
                    }
                }
            } while (true);

            return parameters.ToArray();
        }

        private bool Expect(TokenType expectedType, out Token token, out string errorMessage)
        {
            errorMessage = string.Empty;
            token = PeekNextToken();

            if (token == null)
            {
                errorMessage = string.Format("Expect {0}, but there is no more token", expectedType);
                return false;
            }

            token = GetNextToken();
            if (token.Type != expectedType)
            {
                errorMessage = string.Format(
                    "Expect {0} at position {1}, but get {2}",
                    expectedType,
                    token.StartPosition,
                    token.Type.ToString());

                return false;
            }

            return true;
        }
    }
}
