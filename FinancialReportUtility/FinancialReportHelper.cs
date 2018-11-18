namespace StockAnalysis.FinancialReportUtility
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    public static class FinancialReportHelper
    {
        private static readonly Dictionary<string, decimal> Units = new Dictionary<string, decimal>
        {
            { "元", 1.0M },
            { "万元", 10000.0M },
            { "十万元", 100000.0M },
            { "百万元", 1000000.0M },
            { "千万元", 10000000.0M },
            { "亿元", 100000000.0M },
            { "万", 10000.0M },
            { "十万", 100000.0M },
            { "百万", 1000000.0M },
            { "千万", 10000000.0M },
            { "亿", 100000000.0M },
            { "%", 1.0M },
            { "％", 1.0M }
        };

        private static readonly Regex UnitRegex = new Regex(@"(.*)(\(|（)(.+)(）|\))$", RegexOptions.Compiled);

        /// <summary>
        /// Parse a definition string to get the unit (if any) and cleaned definition contains no unit.
        /// </summary>
        /// <param name="definition">the definiton string</param>
        /// <param name="defaultUnit">the default unit if no unit is specified in the definition</param>
        /// <param name="cleanedDefinition">[OUT] cleand definition that contains no unit</param>
        /// <param name="unit">[out] unit specified in the definition or defaultUnit if there is no unit in definition</param>
        /// <returns>true if there is unit in the definition, otherwise false is returned</returns>
        /// <example>财务指标（万元） is parsed to unit 10000.0 (万) and cleaned definition 财务指标</example>
        public static bool ParseDefinitionAndUnit(string definition, decimal defaultUnit, out string cleanedDefinition, out decimal unit)
        {
            cleanedDefinition = string.Empty;
            unit = defaultUnit;

            if (string.IsNullOrWhiteSpace(definition))
            {
                return false;
            }

            definition = definition.Trim();

            var match = UnitRegex.Match(definition);

            if (match.Success)
            {
                var unitString = match.Groups[3].Value;

                if (Units.TryGetValue(unitString, out unit))
                {
                    cleanedDefinition = match.Groups[1].Value;

                    return true;
                }
            }

            // special condition for columns which has only % or ％ at the end.
            if (definition.EndsWith("%") || definition.EndsWith("％"))
            {
                cleanedDefinition = definition.Substring(0, definition.Length - 1);
                unit = 1.0M;

                return true;
            }

            unit = defaultUnit;
            cleanedDefinition = definition;

            return false;
        }
    }
}
