namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    
    /// <summary>
    /// This class implements component configuration which allows each component in FastRank
    /// to have its own namespace and configuration, so that it does not need to define everything
    /// by command line parameter in Application.cs. It also provides a useful feature: 
    /// unrecognized paramter will be ignored, and will not stop the program from starting up,
    /// so the new AETHER module's configuration string can be copied to the old AETHER module
    /// and user don't need to worry about forward compatiability problem.
    /// 
    /// A component configuration string is in following format:
    /// [componentname:p1=v1;p2=v2;...;px=vx]
    /// multiple component configurations can be concatenated together when specified in 
    /// command line: [componentname1:p1=v1;p2=v2;...;px=vx][componentname2:p1=v1;p2=v2;...;px=vx]
    /// 
    /// The parameter name p1,...,px should be unique for the same component specified by componentname,
    /// But the parameter names across components could be the same because the scope of parameter
    /// is limited in component only.
    /// </summary>
    public static class ComponentConfiguration
    {
        /// <summary>
        /// All registered component configuration objects that indexed by component name
        /// </summary>
        private static Dictionary<string, ComponentConfigurationBase> registeredComponents
            = new Dictionary<string, ComponentConfigurationBase>();

        /// <summary>
        /// All component configurations that have been parsed to key-value pairs. 
        /// The key-value pairs are associated with component name.
        /// </summary>
        private static List<Tuple<string, Dictionary<string, string>>> parsedConfigurations = null;

        private static object syncRoot = new object();

        /// <summary>
        /// Register a component that need to get its configuration.
        /// </summary>
        /// <param name="configuration">component's configuration object used for receiving
        /// configuration</param>
        public static void RegisterComponent(ComponentConfigurationBase configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            string name = configuration.GetComponentName();
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException("component name can't be empty or null");
            }

            // component name is case insensitive, so we convert name to lower-case 
            // for comparason here.
            name = name.ToLowerInvariant();

            lock (ComponentConfiguration.syncRoot)
            {
                // a component can't be registered twice and can't have the same name
                // with other components.
                if (registeredComponents.ContainsKey(name))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Component {0} has been registered before, it can't be registered again",
                            name));
                }

                registeredComponents.Add(name, configuration);

                // because different components are defined in different assembly, so a component
                // might be registered after all configuration strings have been parsed, we need
                // to check if there is parsed configuration for the component.
                if (parsedConfigurations != null)
                {
                    var componentConfigurations = parsedConfigurations.Where(p => p.Item1 == name).ToList();
                    componentConfigurations.ForEach(
                        (Tuple<string, Dictionary<string, string>> tuple) =>
                        {
                            configuration.SetParameters(tuple.Item2);
                            configuration.CheckParameters();
                        });
                }
            }
        }

        /// <summary>
        /// Initialize component configuration collection by configuration strings.
        /// </summary>
        /// <param name="configurations">configuration strings. the format of configuration
        /// string is like:
        /// [componentName1:p1=v1;p2=v2;...;pn=vn]...[componentNameX:px=vx;...;pz=vz]
        /// </param>
        public static void Initialize(string[] configurations)
        {
            if (configurations == null)
            {
                throw new ArgumentNullException("configurations");
            }

            lock (ComponentConfiguration.syncRoot)
            {
                parsedConfigurations = configurations
                    .SelectMany(config => ParseConfiguration(config))
                    .ToList();

                // because different components are defined in different assembly, so a component
                // might be registered before all configuration strings have been parsed, we need
                // to initialize those components after having parsed the configuration strings.
                parsedConfigurations.ForEach(
                    (Tuple<string, Dictionary<string, string>> tuple) =>
                    {
                        if (registeredComponents.ContainsKey(tuple.Item1))
                        {
                            registeredComponents[tuple.Item1].SetParameters(tuple.Item2);
                            registeredComponents[tuple.Item1].CheckParameters();
                        }
                    });
            }
        }

        /// <summary>
        /// Clears this instance.
        /// This is designed for helping unit test.
        /// </summary>
        internal static void Clear()
        {
            registeredComponents.Clear();
            if (parsedConfigurations != null)
            {
                parsedConfigurations.Clear();
            }
        }

        /// <summary>
        /// Unregisters the component.
        /// This is designed for helping unit test.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        internal static void UnregisterComponent(ComponentConfigurationBase configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            string name = configuration.GetComponentName();
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException("component name can't be empty or null");
            }

            // component name is case insensitive, so we convert name to lower-case 
            // for comparason here.
            name = name.ToLowerInvariant();

            lock (ComponentConfiguration.syncRoot)
            {
                registeredComponents.Remove(name);
                if (parsedConfigurations != null)
                {
                    parsedConfigurations.RemoveAll(tuple => tuple.Item1 == name);
                }
            }
        }

        private static IEnumerable<Tuple<string, Dictionary<string, string>>> 
            ParseConfiguration(string configuration)
        {
            foreach (var section in ParseComponentSection(configuration))
            {
                int colonIndex = section.IndexOf(':');
                if (colonIndex < 0)
                {
                    throw new FormatException("':' is missed in component configuration");
                }

                string componentName = section.Substring(0, colonIndex);
                if (string.IsNullOrEmpty(componentName))
                {
                    throw new FormatException("component name is empty");
                }

                string parameters = section.Substring(colonIndex + 1).Trim();
                if (string.IsNullOrEmpty(parameters))
                {
                    // empty configuration is ignored.
                    continue;
                }

                Dictionary<string, string> parameterValues = new Dictionary<string, string>();

                // parse parameters
                foreach (var kvp in ParseParameters(parameters))
                {
                    if (parameterValues.ContainsKey(kvp.Key))
                    {
                        throw new FormatException("duplicated parameter name " + kvp.Key);
                    }

                    parameterValues.Add(kvp.Key, kvp.Value);
                }

                yield return Tuple.Create(componentName, parameterValues);
            }
        }

        private static IEnumerable<string> ParseComponentSection(string configuration)
        {
            if (string.IsNullOrEmpty(configuration))
            {
                yield break;
            }

            int startIndex = 0;
            while (startIndex < configuration.Length)
            {
                // skip leading spaces.
                while (char.IsWhiteSpace(configuration[startIndex]))
                {
                    ++startIndex;
                }

                if (startIndex >= configuration.Length)
                {
                    break;
                }

                if (configuration[startIndex] != '[')
                {
                    throw new FormatException("component configuration should be enbraced by []");
                }

                ++startIndex;

                int endIndex = configuration.IndexOf(']', startIndex);
                if (endIndex < 0)
                {
                    throw new FormatException("component configuration should be enbraced by []");
                }

                string componentSection = configuration.Substring(startIndex, endIndex - startIndex).Trim();

                startIndex = endIndex + 1;

                if (!string.IsNullOrEmpty(componentSection))
                {
                    yield return componentSection;
                }
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> ParseParameters(string parameters)
        {
            if (string.IsNullOrEmpty(parameters))
            {
                yield break;
            }

            string[] tokens = parameters.Split(';');

            foreach (string token in tokens)
            {
                string parameter = token.Trim();
                if (string.IsNullOrEmpty(parameter))
                {
                    continue;
                }

                string[] parts = parameter.Split('=');
                if (parts.Length != 2)
                {
                    throw new FormatException("parameter definition should contains exact one '='");
                }

                string key = parts[0].Trim();
                if (string.IsNullOrEmpty(key))
                {
                    throw new FormatException("parameter name could not be empty");
                }

                string value = parts[1].Trim();

                yield return new KeyValuePair<string, string>(key, value);
            }
        }

        /// <summary>
        /// Base class of all component configuration class
        /// </summary>
        public abstract class ComponentConfigurationBase
        {
            /// <summary>
            /// Get the name of component
            /// </summary>
            /// <returns></returns>
            public abstract string GetComponentName();

            /// <summary>
            /// Set parameters' value
            /// </summary>
            /// <param name="parameters">the [parameter name, parameter value] pairs
            /// parsed from command line</param>
            public abstract void SetParameters(IDictionary<string, string> parameters);

            /// <summary>
            /// Check if the parameters' values that have been set are valid.
            /// This function is called each time after new parameter values are set,
            /// and it provides an opportunity for component configuration to do some
            /// additional things, such as print out current parameter values.
            /// </summary>
            public abstract void CheckParameters();
        }
    }
}
