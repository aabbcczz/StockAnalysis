namespace StockAnalysis.MetricsDefinition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public sealed class MetricExpressionParser
    {
        private readonly Queue<Token> _tokens = new Queue<Token>();
        private readonly TokenType[] _parameterTokenTypes = new TokenType[]
        {
            TokenType.Number, 
            TokenType.String
        };

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

        public string LastErrorMessage { get; private set;}

        private void Reset()
        {
            _tokens.Clear();
            LastErrorMessage = string.Empty;
        }

        public MetricExpression Parse(string expression)
        {
            // reset status and internal data structures.
            Reset();

            // parse all tokens out and put it in queue.
            var tokenizer = new Tokenizer(expression);

            Token token;
            

            do
            {
                if (!tokenizer.GetNextToken(out token))
                {
                    LastErrorMessage = "Parse token failed: " + tokenizer.LastErrorMessage;
                    return null;
                }

                if (token != null)
                {
                    _tokens.Enqueue(token);
                }
            } while (token != null);


            // use recursive descending parsing
            var metric = Parse();

            if (metric != null)
            {
                token = PeekNextToken();
                if (token != null)
                {
                    LastErrorMessage = string.Format("Unexpected token {0} left after parsing at {1}", token.Type, token.StartPosition);
                    return null;
                }
            }

            return metric;
        }

        private MetricExpression Parse()
        {
            // parse the first part, such as MA[20]
            var metric = ParseMetric();
            if (metric == null)
            {
                return null;
            }

            // parse the call operation part, such as (MA[20])
            MetricExpression callee = null;

            var token = PeekNextToken();
            if (token != null && token.Type == TokenType.LeftParenthese)
            {
                GetNextToken();

                callee = Parse();

                if (callee == null || !Expect(TokenType.RightParenthese, out token))
                {
                    return null;
                }
            }

            // parse the selection part, such as .DIF
            var fieldIndex = -1;
            token = PeekNextToken();
            if (token != null && token.Type == TokenType.Dot)
            {
                GetNextToken();

                if (!Expect(TokenType.Identifier, out token))
                {
                    return null;
                }

                var field = token.Value;

                // verify if the selection name is part of metric definition
                var metricType = metric.Metric.GetType();
                var attribute = metricType.GetCustomAttribute<MetricAttribute>();

                if (!attribute.NameToFieldIndexMap.ContainsKey(field))
                {
                    LastErrorMessage = string.Format("{0} is not a valid subfield of metric {1}", field, metricType.Name);
                    return null;
                }

                fieldIndex = attribute.NameToFieldIndexMap[field];
            }

            MetricExpression retValue = metric;

            if (callee != null)
            {
                retValue = new CallOperator(metric, callee);
            }

            if (fieldIndex >= 0)
            {
                retValue = new SelectionOperator(retValue, fieldIndex);
            }

            return retValue;
        }

        private StandaloneMetric ParseMetric()
        {
            Token token;

            // Get name
            if (!Expect(TokenType.Identifier, out token))
            {
                return null;
            }

            var name = token.Value;
            var parameters = new string[0];

            var nextToken = PeekNextToken();

            if (nextToken != null && nextToken.Type == TokenType.LeftBracket)
            {
                GetNextToken();

                parameters = ParseParameters();

                if (parameters == null)
                {
                    return null;
                }

                if (!Expect(TokenType.RightBracket, out token))
                {
                    return null;
                }
            }

            // check if name is valid metric
            if (!MetricEvaluationContext.NameToMetricMap.ContainsKey(name))
            {
                LastErrorMessage = string.Format("Undefined metric name {0}", name);
                return null;
            }

            var metricType = MetricEvaluationContext.NameToMetricMap[name];
            StandaloneMetric metric = null;

            try
            {
                var constructors = metricType.FindMembers(
                    MemberTypes.Constructor, 
                    BindingFlags.Public | BindingFlags.Instance, 
                    null, 
                    null);
                foreach (var constructor in constructors)
                {
                    var parameterTypes = ((ConstructorInfo)constructor).GetParameters().Select(pi => pi.ParameterType).ToArray();
                    if (parameterTypes.Length != parameters.Length)
                    {
                        continue;
                    }

                    // try to convert parameters to the expected type
                    var objects = new object[parameterTypes.Length];

                    try
                    {
                        for (var i = 0; i < parameterTypes.Length; ++i)
                        {
                            objects[i] = Convert.ChangeType(parameters[i], parameterTypes[i]);
                        }
                    }
                    catch
                    {
                        continue;
                    }

                    // now try to create instance with converted parameters
                    metric = new StandaloneMetric((SerialMetric)Activator.CreateInstance(metricType, objects));
                    break;
                }

                if (metric == null)
                {
                    LastErrorMessage = string.Format(
                        "Can't find proper constructor for metric {0} that can be initialized by parameters {1}",
                        metricType.Name,
                        string.Join(",", parameters));

                    return null;
                }
            }
            catch (Exception ex)
            {
                LastErrorMessage = string.Format(
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
        /// <returns>
        /// null: failed.
        /// empty array : no parameter
        /// otherwise : parameters
        /// </returns>
        private string[] ParseParameters()
        {
            var parameters = new List<string>();

            do
            {
                Token token = PeekNextToken();
                if (token == null)
                {
                    LastErrorMessage = "Expect ']'";
                    return null;
                }

                if (token.Type == TokenType.RightBracket)
                {
                    break;
                }

                if (!Expect(_parameterTokenTypes, out token))
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

        private bool Expect(TokenType[] expectedTypes, out Token token)
        {
            token = PeekNextToken();

            if (token == null)
            {
                LastErrorMessage = string.Format("Expect {0}, but there is no more token", string.Join("|", expectedTypes));
                return false;
            }

            token = GetNextToken();
            if (!expectedTypes.Contains(token.Type))
            {
                LastErrorMessage = string.Format(
                    "Expect {0} at position {1}, but get {2}",
                    string.Join("|", expectedTypes),
                    token.StartPosition,
                    token.Type.ToString());

                return false;
            }

            return true;
        }

        private bool Expect(TokenType expectedType, out Token token)
        {
            token = PeekNextToken();

            if (token == null)
            {
                LastErrorMessage = string.Format("Expect {0}, but there is no more token", expectedType);
                return false;
            }

            token = GetNextToken();
            if (token.Type != expectedType)
            {
                LastErrorMessage = string.Format(
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
