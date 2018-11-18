namespace StockAnalysis.TradingStrategy.Strategy
{
    using System.Collections.Generic;
    using System.Linq;
    using Base;

    public sealed class GapDownBouncePositionAdjusting : GeneralPositionAdjustingBase
    {
        public override string Name
        {
            get { return "跳空反弹头寸调整策略"; }
        }

        public override string Description
        {
            get { return "当首日收小幅阴线时按收盘价买入"; }
        }

        [Parameter(3.0, "最大收盘下跌百分比，当下跌比例超过时不允许买入")]
        public double MaxDropPercentage { get; set; }

        public override IEnumerable<Instruction> AdjustPositions()
        {
            var symbols = Context.GetAllPositionSymbols().ToArray();
            var instructions = new List<Instruction>();

            foreach (var symbol in symbols)
            {
                var positions = Context.GetPositionDetails(symbol);
                if (positions.Count() != 1)
                {
                    continue;
                }

                var position = positions.First();
                if (position.LastedPeriodCount != 0)
                {
                    continue;
                }

                var tradingObject = Context.GetTradingObject(symbol);
                var todayBar = Context.GetBarOfTradingObjectForCurrentPeriod(tradingObject);
                var dropPercentage = (todayBar.OpenPrice - todayBar.ClosePrice) / todayBar.OpenPrice * 100.0;

                if (dropPercentage > 0 &&  dropPercentage <= MaxDropPercentage)
                {
                    var instruction = new OpenInstruction(CurrentPeriod, tradingObject, new TradingPrice(TradingPricePeriod.CurrentPeriod, TradingPriceOption.ClosePrice, 0.0))
                    {
                        Comments = string.Format("Adjust: first day drop percentage {0:0.000}%", dropPercentage),
                        Volume = position.Volume,
                        StopLossGapForBuying = 0.0,
                        StopLossPriceForBuying = position.StopLossPrice
                    };

                    instructions.Add(instruction);
                }
            }

            return instructions;
        }
    }
}
