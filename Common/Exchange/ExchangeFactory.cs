namespace StockAnalysis.Common.Exchange
{
    // All time zone id in system.
    //Afghanistan Standard Time
    //Alaskan Standard Time
    //Alaskan Standard Time\Dynamic DST
    //Arab Standard Time
    //Arabian Standard Time
    //Arabic Standard Time
    //Arabic Standard Time\Dynamic DST
    //Argentina Standard Time
    //Argentina Standard Time\Dynamic DST
    //Atlantic Standard Time
    //Atlantic Standard Time\Dynamic DST
    //AUS Central Standard Time
    //AUS Eastern Standard Time
    //AUS Eastern Standard Time\Dynamic DST
    //Azerbaijan Standard Time
    //Azores Standard Time
    //Azores Standard Time\Dynamic DST
    //Bahia Standard Time
    //Bahia Standard Time\Dynamic DST
    //Bangladesh Standard Time
    //Bangladesh Standard Time\Dynamic DST
    //Canada Central Standard Time
    //Cape Verde Standard Time
    //Caucasus Standard Time
    //Caucasus Standard Time\Dynamic DST
    //Cen.Australia Standard Time
    //Cen.Australia Standard Time\Dynamic DST
    //Central America Standard Time
    //Central Asia Standard Time
    //Central Brazilian Standard Time
    //Central Brazilian Standard Time\Dynamic DST
    //Central Europe Standard Time
    //Central European Standard Time
    //Central Pacific Standard Time
    //Central Standard Time
    //Central Standard Time\Dynamic DST
    //Central Standard Time (Mexico)
    //China Standard Time
    //Dateline Standard Time
    //E.Africa Standard Time
    //E.Australia Standard Time
    //E.Europe Standard Time
    //E. South America Standard Time
    //E.South America Standard Time\Dynamic DST
    //Eastern Standard Time
    //Eastern Standard Time\Dynamic DST
    //Egypt Standard Time
    //Egypt Standard Time\Dynamic DST
    //Ekaterinburg Standard Time
    //Ekaterinburg Standard Time\Dynamic DST
    //Fiji Standard Time
    //Fiji Standard Time\Dynamic DST
    //FLE Standard Time
    //Georgian Standard Time
    //GMT Standard Time
    //Greenland Standard Time
    //Greenland Standard Time\Dynamic DST
    //Greenwich Standard Time
    //GTB Standard Time
    //Hawaiian Standard Time
    //India Standard Time
    //Iran Standard Time
    //Iran Standard Time\Dynamic DST
    //Israel Standard Time
    //Israel Standard Time\Dynamic DST
    //Jordan Standard Time
    //Jordan Standard Time\Dynamic DST
    //Kaliningrad Standard Time
    //Kaliningrad Standard Time\Dynamic DST
    //Kamchatka Standard Time
    //Korea Standard Time
    //Libya Standard Time
    //Libya Standard Time\Dynamic DST
    //Magadan Standard Time
    //Magadan Standard Time\Dynamic DST
    //Mauritius Standard Time
    //Mauritius Standard Time\Dynamic DST
    //Mid-Atlantic Standard Time
    //Middle East Standard Time
    //Middle East Standard Time\Dynamic DST
    //Montevideo Standard Time
    //Montevideo Standard Time\Dynamic DST
    //Morocco Standard Time
    //Morocco Standard Time\Dynamic DST
    //Mountain Standard Time
    //Mountain Standard Time\Dynamic DST
    //Mountain Standard Time (Mexico)
    //Myanmar Standard Time
    //N.Central Asia Standard Time
    //N. Central Asia Standard Time\Dynamic DST
    //Namibia Standard Time
    //Namibia Standard Time\Dynamic DST
    //Nepal Standard Time
    //New Zealand Standard Time
    //New Zealand Standard Time\Dynamic DST
    //Newfoundland Standard Time
    //Newfoundland Standard Time\Dynamic DST
    //North Asia East Standard Time
    //North Asia East Standard Time\Dynamic DST
    //North Asia Standard Time
    //North Asia Standard Time\Dynamic DST
    //Pacific SA Standard Time
    //Pacific SA Standard Time\Dynamic DST
    //Pacific Standard Time
    //Pacific Standard Time\Dynamic DST
    //Pacific Standard Time (Mexico)
    //Pakistan Standard Time
    //Pakistan Standard Time\Dynamic DST
    //Paraguay Standard Time
    //Paraguay Standard Time\Dynamic DST
    //Romance Standard Time
    //Russian Standard Time
    //Russian Standard Time\Dynamic DST
    //SA Eastern Standard Time
    //SA Pacific Standard Time
    //SA Western Standard Time
    //Samoa Standard Time
    //Samoa Standard Time\Dynamic DST
    //SE Asia Standard Time
    //Singapore Standard Time
    //South Africa Standard Time
    //Sri Lanka Standard Time
    //Syria Standard Time
    //Syria Standard Time\Dynamic DST
    //Taipei Standard Time
    //Tasmania Standard Time
    //Tasmania Standard Time\Dynamic DST
    //Tokyo Standard Time
    //Tonga Standard Time
    //Turkey Standard Time
    //Turkey Standard Time\Dynamic DST
    //Ulaanbaatar Standard Time
    //US Eastern Standard Time
    //US Eastern Standard Time\Dynamic DST
    //US Mountain Standard Time
    //UTC
    //UTC+12
    //UTC-02
    //UTC-11
    //Venezuela Standard Time
    //Venezuela Standard Time\Dynamic DST
    //Vladivostok Standard Time
    //Vladivostok Standard Time\Dynamic DST
    //W.Australia Standard Time
    //W.Australia Standard Time\Dynamic DST
    //W.Central Africa Standard Time
    //W.Europe Standard Time
    //West Asia Standard Time
    //West Pacific Standard Time
    //Yakutsk Standard Time
    //Yakutsk Standard Time\Dynamic DST

    using System.Collections.Generic;

    public static class ExchangeFactory
    {
        private readonly static Dictionary<ExchangeId, IExchange> Exchanges = new Dictionary<ExchangeId, IExchange>
            {
                { ExchangeId.ShanghaiSecurityExchange, new ShanghaiSecurityExchange() },
                { ExchangeId.ShenzhenSecurityExchange, new ShenzhenSecurityExchange() },
                { ExchangeId.NewYorkSecurityExchange, new NewYorkSecurityExchange() },
                { ExchangeId.NasdaqSecurityExchange, new NasdaqSecurityExchange() },
                { ExchangeId.ChinaFinancialFuturesExchange, new ChinaFinancialFuturesExchange() },
                { ExchangeId.DalianCommodityExchange, new DalianCommodityExchange() },
                { ExchangeId.ShanghaiFutureExchange, new ShanghaiFutureExchange() },
                { ExchangeId.ShanghaiInternationalEnergyExchange, new ShanghaiInternationalEnergyExchange() },
                { ExchangeId.ZhengzhouCommodityExchange, new ZhengzhouCommodityExchange() },
            };


        public static IExchange GetExchangeById(ExchangeId id)
        {
            return Exchanges[id];
        }

        public static IExchange GetExchangeByName(string abbreviation)
        {
            foreach (var exchange in Exchanges.Values)
            {
                if (string.Compare(exchange.CapitalizedAbbreviation, abbreviation, true) == 0)
                {
                    return exchange;
                }
            }

            return null;
        }

        public static IExchange GetExchangeBySymbolPrefix(string prefix)
        {
            foreach (var exchange in Exchanges.Values)
            {
                if (string.Compare(exchange.CapitalizedSymbolPrefix, prefix, true) == 0)
                {
                    return exchange;
                }
            }

            return null;
        }
    }
}
