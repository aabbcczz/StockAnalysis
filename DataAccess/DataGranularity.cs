namespace StockAnalysis.DataAccess
{
    /// <summary>
    /// define the granularity of Bar in second
    /// </summary>
    public enum DataGranularity : uint
    {
        D1s = 1,
        D5s = 5,
        D10s = 10,
        D15s = 15,
        D30s = 30,
        D1min = 60,
        D3min = 180,
        D5min = 300,
        D10min = 600,
        D15min = 900,
        D30min = 1800,
        D1h = 3600,
        D2h = 7200,
        D3h = 10800,
        D4h = 14400,
        D1d = 86400,
        D1w = 604800,
        D1mon = 2592000,
        D1season = 7776000,
        D1y = 31104000
    }
}
