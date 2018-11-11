namespace StockAnalysis.Share
{
    public interface ISymbolNormalizer
    {
        /// <summary>
        /// Try to noramlize a symbol to SecuritySymbol object
        /// </summary>
        /// <param name="symbol">symbol to be normalized</param>
        /// <param name="securitySymbol">[out] the normalized security symbol</param>
        /// <returns>true if <paramref name="symbol"/> can be normalized or has been normalized by this class. otherwise false is returned</returns>
        bool TryNormalizeSymbol(string symbol, out SecuritySymbol securitySymbol);
    }
}
