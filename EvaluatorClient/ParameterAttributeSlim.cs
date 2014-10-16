using System;
using TradingStrategy;

namespace EvaluatorClient
{
    sealed class ParameterAttributeSlim
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        public string ParameterType { get; private set; }

        public string Value { get; set; }

        public ParameterAttribute Attribute { get; private set; }

        public ParameterAttributeSlim(ParameterAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException();
            }

            Attribute = attribute;
            Name = attribute.Name;
            Value = attribute.DefaultValue == null ? string.Empty : attribute.DefaultValue.ToString();
            Description = attribute.Description;
            ParameterType = attribute.ParameterType.Name;
        }
    }
}
