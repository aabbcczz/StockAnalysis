using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TradingStrategy;
using TradingStrategy.Strategy;
using TradingStrategy.Base;

namespace TradingStrategyEvaluation
{
    public class CombinedStrategyAssembler
    {
        private class ParameterValueSelector
        {
            public int ComponentIndex { get; set; }
            public ParameterAttribute Attribute { get; set; }
            public object[] Values { get; set; }
            public int ValueIndex { get; set; }
        }

        private readonly TradingStrategyComponentSettings[] _componentSettings;

        private readonly List<ParameterValueSelector> _parameterValueSelectors
            = new List<ParameterValueSelector>();

        private bool _endPermutation;
        private readonly bool _allowRemovingInstructionRandomly;

        public long NumberOfParmeterValueCombinations { get; private set; }

        public CombinedStrategyAssembler(CombinedStrategySettings settings, bool allowRemovingInstructionRandomly)
        {
            if (settings == null)
            {
                throw new ArgumentNullException();
            }

            _componentSettings = settings.ComponentSettings.Where(s => s.Enabled).ToArray();

            if (_componentSettings.Length == 0)
            {
                throw new ArgumentException("No trading strategy component is enabled in settings");
            }

            _allowRemovingInstructionRandomly = allowRemovingInstructionRandomly;

            // verify if component settings can be used for creating new combined strategy
            try
            {
                NewStrategy();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("settings can't be used to create a valid combined strategy", ex);
            }

            // verify if components' parameter settings are correct
            var components = CreateComponents().ToArray();
            Debug.Assert(components.Length == _componentSettings.Length);

            for (var i = 0; i < _componentSettings.Length; ++i)
            {
                var attributes = ParameterHelper.GetParameterAttributes(components[i]).ToArray();
                var allParameterSettings = _componentSettings[i].ComponentParameterSettings;

                if (allParameterSettings == null || allParameterSettings.Length == 0)
                {
                    continue;
                }

                // detect duplicate names
                var duplicateNames = allParameterSettings
                    .Select(p => p.Name)
                    .GroupBy(s => s)
                    .Where(g => g.Count() > 1)
                    .ToArray();

                if (duplicateNames.Any())
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "duplicate parameter name {0} is found in component {0} settings", 
                            duplicateNames.First().Key,
                            components[i].GetType().FullName));
                }

                // verify if name and valueType matches the component's parameter
                foreach (var parameterSettings in allParameterSettings)
                {
                    // varify if there is name not defined in class.
                    var attribute = attributes.Where(a => a.Name == parameterSettings.Name).ToArray();
                    
                    if (!attribute.Any())
                    {
                        throw new InvalidOperationException(
                            string.Format(
                                "there is no parameter named {0} in component {1}",
                                parameterSettings.Name,
                                components[i].GetType().FullName));
                    }

                    // verify if ValueType is a correct type
                    var valueType = Type.GetType(parameterSettings.ValueType, false);
                    if (valueType == null)
                    {
                        throw new InvalidOperationException(
                            string.Format("{0} is not a valid type", parameterSettings.ValueType));
                    }

                    // verify if ValueType match the true type in class
                    if (attribute.First().ParameterType != valueType)
                    {
                        throw new InvalidOperationException(
                            string.Format(
                                "value type of parameter {0}: {1} does not match the definition of component {2}",
                                parameterSettings.Name,
                                parameterSettings.ValueType,
                                components[i].GetType().FullName));
                    }

                    // verify the values specified in parameter setting is correct
                    object[] values;
                    try
                    {
                        values = parameterSettings.GetParsedValues().ToArray();
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            string.Format(
                                "Failed to parse value for component {0}, parameter {1}",
                                components[i].GetType().FullName,
                                parameterSettings.Name),
                            ex);
                    }

                    // associate attribute with all possible values
                    if (values.Length > 0)
                    {
                        _parameterValueSelectors.Add(
                            new ParameterValueSelector
                            {
                                ComponentIndex = i,
                                Attribute = attribute.First(),
                                Values = values,
                                ValueIndex = 0
                            });
                    }
                }
            }

            // calculate the number of parameter value combinations
            NumberOfParmeterValueCombinations = 1L;
            if (_parameterValueSelectors.Count > 0)
            {
                foreach (var selector in _parameterValueSelectors)
                {
                    checked
                    {
                        NumberOfParmeterValueCombinations *= selector.Values.Length;
                    }
                }
            }
        }

        private ITradingStrategyComponent[] CreateComponents()
        {
            foreach (var settings in _componentSettings)
            {
                var classType = Type.GetType(settings.ClassType, false);
                if (classType == null)
                {
                    throw new InvalidOperationException(
                        string.Format("{0} is not valid type", settings.ClassType));
                }

                if (Attribute.IsDefined(classType, typeof(DeprecatedStrategyAttribute)))
                {
                    throw new InvalidOperationException(
                        string.Format("{0} is deprecated", settings.ClassType));
                }
            }

            var components = _componentSettings
                .Select(s => Type.GetType(s.ClassType))
                .Select(t => (ITradingStrategyComponent)Activator.CreateInstance(t))
                .ToArray();

            return components;
        }

        public CombinedStrategy NewStrategy()
        {
            var strategy = new CombinedStrategy(CreateComponents(), _allowRemovingInstructionRandomly);

            return strategy;
        }

        /// <summary>
        /// Get next set of parameter values
        /// </summary>
        /// <returns>
        /// null: no more values
        /// empty dictionary: all default values
        /// non-empty dictionary: new set of values
        /// </returns>
        public IDictionary<Tuple<int, ParameterAttribute>, object> GetNextSetOfParameterValues()
        {
            if (_endPermutation)
            {
                // no more permutation
                return null;
            }

            if (_parameterValueSelectors.Count == 0)
            {
                _endPermutation = true;
                return new Dictionary<Tuple<int, ParameterAttribute>, object>();
            }

            // get current set of value firstly.
            var result = _parameterValueSelectors
                .ToDictionary(s => Tuple.Create(s.ComponentIndex, s.Attribute), s => s.Values[s.ValueIndex]);

            // move to next set of value
            var index = 0;

            while (index < _parameterValueSelectors.Count)
            {
                ++_parameterValueSelectors[index].ValueIndex;

                if (_parameterValueSelectors[index].ValueIndex >= _parameterValueSelectors[index].Values.Length)
                {
                    _parameterValueSelectors[index].ValueIndex = 0;
                    ++index;
                }
                else
                {
                    break;
                }
            }

            if (index >= _parameterValueSelectors.Count)
            {
                _endPermutation = true;
            }

            return result;
        }
    }
}
