using TradingStrategyEvaluation;

namespace EvaluatorClient
{
    internal sealed class TradingPriceOptionBinding
    {
        public string Text { get; private set; }
        public TradingPriceOption Option { get; private set; }

        public static TradingPriceOptionBinding[] CreateBindings()
        {
            return new[]
            {
                new TradingPriceOptionBinding("本周期开盘价", TradingPriceOption.CurrentOpenPrice),
                new TradingPriceOptionBinding("本周期收盘价", TradingPriceOption.CurrentClosePrice),
                new TradingPriceOptionBinding("本周期中间价", TradingPriceOption.CurrentMiddlePrice),
                new TradingPriceOptionBinding("本周期最高价", TradingPriceOption.CurrentHighestPrice),
                new TradingPriceOptionBinding("本周期最低价", TradingPriceOption.CurrentLowestPrice),
                new TradingPriceOptionBinding("次周期开盘价", TradingPriceOption.NextOpenPrice),
                new TradingPriceOptionBinding("次周期收盘价", TradingPriceOption.NextClosePrice),
                new TradingPriceOptionBinding("次周期中间价", TradingPriceOption.NextMiddlePrice),
                new TradingPriceOptionBinding("次周期最高价", TradingPriceOption.NextHighestPrice),
                new TradingPriceOptionBinding("次周期最低价", TradingPriceOption.NextLowestPrice)
            };
        }

        public TradingPriceOptionBinding(string text, TradingPriceOption option)
        {
            Text = text;
            Option = option;
        }
    }
}
