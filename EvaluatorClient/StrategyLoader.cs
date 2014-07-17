using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using TradingStrategy;

namespace EvaluatorClient
{
    internal static class StrategyLoader
    {
        private static List<ITradingStrategy> _strategies = new List<ITradingStrategy>();

        private static bool Initialized = false;

        public static IEnumerable<ITradingStrategy> Strategies
        {
            get { return _strategies; }
        }

        public static void Initialize()
        {
            if (Initialized)
            {
                return;
            }

            List<Type> strategyTypes = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                strategyTypes.AddRange(assembly.GetTypes()
                    .Where(type => type.IsClass 
                        && typeof(ITradingStrategy).IsAssignableFrom(type)
                        && !type.IsAbstract
                        && !type.IsInterface));
            }

            _strategies = new List<ITradingStrategy>(strategyTypes.Count);
            foreach (Type type in strategyTypes)
            {
                ITradingStrategy strategy = (ITradingStrategy)Activator.CreateInstance(type);

                _strategies.Add(strategy);
            }

            Initialized = true;
        }
    }
}
