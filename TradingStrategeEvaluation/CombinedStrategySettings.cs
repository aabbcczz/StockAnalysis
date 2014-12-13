using System;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using TradingStrategy;
using TradingStrategy.Strategy;

namespace TradingStrategyEvaluation
{
    [Serializable]
    public sealed class CombinedStrategySettings
    {
        public int MaxNumberOfActiveStocks { get; set; }

        public int MaxNumberOfActiveStocksPerBlock { get; set; }

        public bool RandomSelectTransactionWhenThereIsNoEnoughCapital { get; set; }

        public TradingStrategyComponentSettings[] ComponentSettings { get; set; }

        public static CombinedStrategySettings LoadFromFile(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException();
            }

            CombinedStrategySettings settings;

            var serializer = new XmlSerializer(typeof(CombinedStrategySettings));

            using (var reader = new StreamReader(file))
            {
                settings = (CombinedStrategySettings)serializer.Deserialize(reader);
            }

            if (settings.ComponentSettings == null
                || settings.ComponentSettings.Length == 0)

            {
                throw new InvalidDataException("no component settings is loaded");
            }

            return settings;
        }

        public void SaveToFile(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException();
            }

            var serializer = new XmlSerializer(typeof(CombinedStrategySettings));

            using (var writer = new StreamWriter(file))
            {
                serializer.Serialize(writer, this);
            }
        }

        public CombinedStrategySettings GetActiveSettings()
        {
            var settings = new CombinedStrategySettings
            {
                MaxNumberOfActiveStocks = this.MaxNumberOfActiveStocks,
                MaxNumberOfActiveStocksPerBlock = this.MaxNumberOfActiveStocksPerBlock,
                RandomSelectTransactionWhenThereIsNoEnoughCapital = this.RandomSelectTransactionWhenThereIsNoEnoughCapital,
                ComponentSettings = ComponentSettings.Where(s => s.Enabled).ToArray()
            };

            return settings;
        }

        public static CombinedStrategySettings GenerateExampleSettings()
        {
            CombinedStrategy.ForceLoad();

            var settings = new CombinedStrategySettings();

            var allComponents = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes()
                    .Where(type => type.IsClass 
                        && !type.IsAbstract 
                        && typeof(ITradingStrategyComponent).IsAssignableFrom(type)
                        && !typeof(ITradingStrategy).IsAssignableFrom(type)
                        && !type.IsInterface));

            settings.ComponentSettings = allComponents
                .OrderBy(t => t, new TradingStrategyComponentComparer())
                .Select(c => TradingStrategyComponentSettings.GenerateExampleSettings(
                    (ITradingStrategyComponent)Activator.CreateInstance(c)))
                .ToArray();

            settings.MaxNumberOfActiveStocks = 1000;
            settings.MaxNumberOfActiveStocksPerBlock = 1000;
            settings.RandomSelectTransactionWhenThereIsNoEnoughCapital = false;

            return settings;
        }

        private class TradingStrategyComponentComparer 
            : System.Collections.Generic.IComparer<Type>
        {
            public int Compare(Type x, Type y)
            {
                int xOrder = GetOrderNumber(x);
                int yOrder = GetOrderNumber(y);

                if (xOrder == yOrder)
                {
                    return x.FullName.CompareTo(y.FullName);
                }
                else
                {
                    return xOrder.CompareTo(yOrder);
                }
            }

            private int GetOrderNumber(Type x)
            {
                if (typeof(IMarketEnteringComponent).IsAssignableFrom(x))
                {
                    return 0;
                }

                if (typeof(IMarketExitingComponent).IsAssignableFrom(x))
                {
                    return 1;
                }

                if (typeof(IStopLossComponent).IsAssignableFrom(x))
                {
                    return 2;
                }

                if (typeof(IPositionSizingComponent).IsAssignableFrom(x))
                {
                    return 3;
                }

                if (typeof(IPositionAdjustingComponent).IsAssignableFrom(x))
                {
                    return 4;
                }

                return 100;
            }
        }
    }
}
