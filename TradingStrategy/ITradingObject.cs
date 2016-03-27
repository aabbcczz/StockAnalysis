namespace TradingStrategy
{
    public interface ITradingObject
    {
        // For performance consideration, the data provider will assign unique index for
        // each trading object, starts from 0, to allow other parts of program use array
        // to store/index the objects.
        // it will be the same in the same run and might change across different runs.
        int Index { get; }

        // code, like 'SH600002', should be unique
        string Code { get; }

        // name, like '中国平安'
        string Name { get; }

        bool IsTradable { get; }

        // specific object associated with this trading object. the value depends on the type of
        // trading object. for example, for china stock trading object, this field will be associated
        // with a StockName object.
        object Object { get; }

        int VolumePerHand { get; }

        // volume per unit for buying
        int VolumePerBuyingUnit { get; }

        // volume per unit for selling
        int VolumePerSellingUnit { get; }

        // minimum price unit
        double MinPriceUnit { get; }

        // limit up ratio
        double LimitUpRatio { get;  }

        // limit down ratio
        double LimitDownRatio{ get; }
    }
}
