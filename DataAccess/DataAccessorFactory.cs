namespace StockAnalysis.DataAccess
{
    using System;

    public static class DataAccessorFactory
    {
        public static IDataAccessor CreateFileStorageAccessor(string rootPath)
        {
            if (string.IsNullOrEmpty(rootPath))
            {
                throw new ArgumentNullException();
            }

            return new FileStorageAccessor(rootPath);
        } 
    }
}
