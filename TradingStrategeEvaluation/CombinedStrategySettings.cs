using System;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using TradingStrategy;

namespace TradingStrategyEvaluation
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
            var settings = new CombinedStrategySettings();
            settings.ComponentSettings = ComponentSettings.Where(s => s.Enabled).ToArray();

            return settings;
        }

        public static CombinedStrategySettings GenerateExampleSettings()
        {
            var settings = new CombinedStrategySettings();

            var allComponents = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes()
                    .Where(type => type.IsClass 
                        && !type.IsAbstract 
                        && typeof(ITradingStrategyComponent).IsAssignableFrom(type)
                        && !typeof(ITradingStrategy).IsAssignableFrom(type)
                        && !type.IsInterface));

            settings.ComponentSettings = allComponents
                .Select(c => TradingStrategyComponentSettings.GenerateExampleSettings(
                    (ITradingStrategyComponent)Activator.CreateInstance(c)))
                .ToArray();

            return settings;
        }
    }
}
