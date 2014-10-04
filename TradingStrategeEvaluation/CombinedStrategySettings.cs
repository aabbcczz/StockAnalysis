using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;

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

            XmlSerializer serializer = new XmlSerializer(typeof(CombinedStrategySettings));

            using (StreamReader reader = new StreamReader(file))
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

            XmlSerializer serializer = new XmlSerializer(typeof(CombinedStrategySettings));

            using (StreamWriter writer = new StreamWriter(file))
            {
                serializer.Serialize(writer, this);
            }
        }

        public CombinedStrategySettings GetActiveSettings()
        {
            CombinedStrategySettings settings = new CombinedStrategySettings();
            settings.ComponentSettings = ComponentSettings.Where(s => s.Enabled).ToArray();

            return settings;
        }

        public static CombinedStrategySettings GenerateExampleSettings()
        {
            CombinedStrategySettings settings = new CombinedStrategySettings();

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
