using System;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using StockAnalysis.TradingStrategy;
using StockAnalysis.TradingStrategy.Strategy;
using StockAnalysis.TradingStrategy.Base;

namespace StockAnalysis.TradingStrategy.Evaluation
{
    [Serializable]
    public sealed class CombinedStrategySettings
    {
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
                        && !type.IsInterface
                        && !Attribute.IsDefined(type, typeof(DeprecatedStrategyAttribute))));


            settings.ComponentSettings = allComponents
                .OrderBy(t => t, new TradingStrategyComponentComparer())
                .Select(c => TradingStrategyComponentSettings.GenerateExampleSettings(
                    (ITradingStrategyComponent)Activator.CreateInstance(c)))
                .ToArray();

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
                if (typeof(GlobalSettingsComponent).IsAssignableFrom(x))
                {
                    return 0;
                }

                if (typeof(IMarketEnteringComponent).IsAssignableFrom(x))
                {
                    return 10;
                }

                if (typeof(IMarketExitingComponent).IsAssignableFrom(x))
                {
                    return 20;
                }

                if (typeof(IStopLossComponent).IsAssignableFrom(x))
                {
                    return 30;
                }

                if (typeof(IPositionSizingComponent).IsAssignableFrom(x))
                {
                    return 40;
                }

                if (typeof(IPositionAdjustingComponent).IsAssignableFrom(x))
                {
                    return 50;
                }

                if (typeof(IBuyPriceFilteringComponent).IsAssignableFrom(x))
                {
                    return 60;
                }

                return 100;
            }
        }
    }
}
