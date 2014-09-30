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
    public sealed class ComponentStrategySettings
    {
        public TradingStrategyComponentSettings[] ComponentSettings { get; set; }

        public static ComponentStrategySettings LoadFromFile(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException();
            }

            ComponentStrategySettings settings;

            XmlSerializer serializer = new XmlSerializer(typeof(ComponentStrategySettings));

            using (StreamReader reader = new StreamReader(file))
            {
                settings = (ComponentStrategySettings)serializer.Deserialize(reader);
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

            XmlSerializer serializer = new XmlSerializer(typeof(ComponentStrategySettings));

            using (StreamWriter writer = new StreamWriter(file))
            {
                serializer.Serialize(writer, this);
            }
        }

        public static ComponentStrategySettings GenerateExampleSettings()
        {
            ComponentStrategySettings settings = new ComponentStrategySettings();

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
