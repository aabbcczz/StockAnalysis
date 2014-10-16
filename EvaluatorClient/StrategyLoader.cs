using System;
using System.Collections.Generic;
using System.Linq;
using TradingStrategy;

namespace EvaluatorClient
{
    internal static class StrategyLoader
    {
        private static List<ITradingStrategy> _strategies = new List<ITradingStrategy>();

        private static bool _initialized;

        public static IEnumerable<ITradingStrategy> Strategies
        {
            get { return _strategies; }
        }

        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            var strategyTypes = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                strategyTypes.AddRange(assembly.GetTypes()
                    .Where(type => type.IsClass 
                        && typeof(ITradingStrategy).IsAssignableFrom(type)
                        && !type.IsAbstract
                        && !type.IsInterface));
            }

            _strategies = new List<ITradingStrategy>(strategyTypes.Count);
            foreach (var type in strategyTypes)
            {
                try
                {
                    var strategy = (ITradingStrategy)Activator.CreateInstance(type);

                    _strategies.Add(strategy);
                }
                catch
                {
                    // ignore exception.
                }
            }

            _initialized = true;
        }
    }
}
