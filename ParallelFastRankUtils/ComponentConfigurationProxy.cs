namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using FastRank;

    /// <summary>
    /// Proxy class to access the static instance of each component configuration.
    /// </summary>
    /// <typeparam name="ConfigurationType">type of component configuration to be accessed</typeparam>
    public class ComponentConfigurationProxy<ConfigurationType> 
        : ComponentConfiguration.ComponentConfigurationBase
        where ConfigurationType : ComponentConfiguration.ComponentConfigurationBase, new()
    {
        private static ComponentConfigurationProxy<ConfigurationType> instance;
        private ConfigurationType _configuration;
        
        static ComponentConfigurationProxy()
        {
            instance = new ComponentConfigurationProxy<ConfigurationType>();
            ComponentConfiguration.RegisterComponent(instance);
        }

        private ComponentConfigurationProxy()
        {
            _configuration = new ConfigurationType();
        }

        /// <summary>
        /// the static instance of configuration class
        /// </summary>
        public static ConfigurationType Configuration
        {
            get { return instance._configuration; }
        }

        public override string GetComponentName()
        {
            return _configuration.GetComponentName();
        }

        public override void SetParameters(IDictionary<string, string> parameters)
        {
            _configuration.SetParameters(parameters);
        }

        public override void CheckParameters()
        {
            _configuration.CheckParameters();
        }

        /// <summary>
        /// Renews the configuration.
        /// This is designed for helping unit test
        /// </summary>
        internal static void RenewConfiguration()
        {
            ComponentConfiguration.UnregisterComponent(instance);
            instance = new ComponentConfigurationProxy<ConfigurationType>();
            ComponentConfiguration.RegisterComponent(instance);
        }
    }
}
