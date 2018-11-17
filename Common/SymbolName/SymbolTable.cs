namespace StockAnalysis.Common.SymbolName
{
    using System;
    using System.IO;
    using System.Text;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    using Exchange;
    using Utility;

    /// <summary>
    /// Symbol table class
    /// </summary>
    /// <remarks>
    /// format: regularexpression <tab> exchange_abbreviation <tab> country_code
    /// example: a\d* <tab> CZCE <tab> CN
    /// additional <tab> character will be ignored
    /// </remarks>
    public sealed class SymbolTable
    {
        private const string SymbolTableFileName = "SymbolTable.txt";

        private static SymbolTable Instance = null;
        private static object syncObj = new object();

        private Dictionary<Regex, IExchange> _rules = new Dictionary<Regex, IExchange>();

        private SymbolTable()
        {
        }

        private void LoadRules(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException();
            }

            var lines = File.ReadAllLines(file, Encoding.UTF8);

            var validLines = lines.Select(l => l.Trim())  // trim
                .Where(l => !string.IsNullOrEmpty(l)) // ignore empty lines
                .Where(l => !l.StartsWith("#")) // ignore comments
                ;

            foreach (var line in validLines)
            {
                // split to fields by \t and remove additional \t
                var fields = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (fields.Count() != 2)
                {
                    AppLogger.Default.ErrorFormat("Invalid line\n{0}", line);
                    continue;
                }

                // fields: regex, exchange name, country
                string regexString = fields[0];
                string exchangeAbbr = fields[1];

                // verify if exchange exists
                IExchange exchange = ExchangeFactory.GetExchangeByName(exchangeAbbr);
                if (exchange == null)
                {
                    AppLogger.Default.ErrorFormat("Invalid exchange name {0}", exchangeAbbr);
                    continue;
                }

                // add to rules
                Regex regex = new Regex(regexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                _rules.Add(regex, exchange);
            }
        }

        public IExchange FindExchangeForRawSymbol(string symbol, string exchangePrefixFilter = null, Country countryFilter = null)
        {
            var kvps = _rules.Where(kvp => kvp.Key.IsMatch(symbol)).ToList();
            if (!string.IsNullOrEmpty(exchangePrefixFilter))
            {
                IExchange exchange = ExchangeFactory.GetExchangeBySymbolPrefix(exchangePrefixFilter);
                if (exchange == null)
                {
                    return null;
                }

                kvps = kvps.Where(kvp => kvp.Value.CapitalizedAbbreviation == exchange.CapitalizedAbbreviation).ToList();
            }

            if (countryFilter != null)
            {
                kvps = kvps.Where(kvp => Country.IsSameCountry(kvp.Value.Country, countryFilter)).ToList();
            }

            if (kvps.Count() == 0)
            {
                return null;
            }
            else if (kvps.Count() == 1)
            {
                return kvps.First().Value;
            }
            else
            {
                throw new InvalidOperationException($"There are more than one exchange for symbol {symbol}, please consider add country filter");
            }
        }

        public static SymbolTable GetInstance()
        {
            if (Instance == null)
            {
                lock (syncObj)
                {
                    if (Instance == null)
                    {
                        Instance = new SymbolTable();
                        Instance.LoadRules(SymbolTableFileName);
                    }
                }
            }

            return Instance;
        }
    }
}
