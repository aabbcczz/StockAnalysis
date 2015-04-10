using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using TradingStrategy.GroupMetrics;
using MetricsDefinition.Metrics;
using TradingStrategy.Base;
using CsvHelper;

namespace TradingStrategy.Strategy
{
    public sealed class BlockPriceIndexFilterMarketEntering : GeneralMarketEnteringBase
    {
        public sealed class BlockUpRatesFromLowestForCode
        {
            public string Code { get; set; }
            public Dictionary<string, double> BlockUpRatesFromLowest { get; set; }
        };

        public sealed class BlockConfig
        {
            public string Block { get; set; }
            public double MinimumUpRate { get; set; }
            public double MaximumUpRate { get; set; }
        }

        private BlockMetricsManager _blockMetricManager;
        private Dictionary<string, RateOfChange> _blockToPriceIndexChangeRateMap;
        private Dictionary<string, Lowest> _blockToLowestMap;
        private Dictionary<string, BlockConfig> _blockConfigMap;

        public override string Name
        {
            get { return "板块价格指数入市过滤器"; }
        }

        public override string Description
        {
            get { return @"当股票所在板块中至少有一个板块的价格指数在给定周期（WindowSize）内变化率超过MininumRateOfChange, 
并且股票板块从最低点上升比例符合配置文件中的规定时则允许入市"; }
        }

        [Parameter(20, "指数升降判定周期")]
        public int WindowSize { get; set; }

        [Parameter(0.0, "指数变化比例最小值，按百分比计算")]
        public double MininumRateOfChange { get; set; }

        [Parameter("BlockUpRateConfig.txt", "板块指数从最低点上升比例配置文件")]
        public string BlockUpRateConfigFile { get; set; }

        [Parameter(0, "板块配置选择参数，0代表选择所有板块，其他数值x>0代表选择配置文件中的第x个板块")]
        public int BlockSelector { get; set; }

        protected override void ValidateParameterValues()
        {
            base.ValidateParameterValues();

            if (WindowSize <= 0)
            {
                throw new ArgumentOutOfRangeException("WindowSize must be greater than 0");
            }

            if (string.IsNullOrWhiteSpace(BlockUpRateConfigFile))
            {
                throw new ArgumentNullException("BlockUpRateConfigFile can't be empty");
            }

            if (BlockSelector < 0)
            {
                throw new ArgumentOutOfRangeException("BlockSelector must not be smaller than 0");
            }
        }

        public override void Initialize(IEvaluationContext context, IDictionary<ParameterAttribute, object> parameterValues)
        {
            base.Initialize(context, parameterValues);

            if (context.RelationshipManager == null)
            {
                return;
            }

            // load block configurations
            var blockConfigs = LoadBlockConfiguration(BlockUpRateConfigFile);

            if (BlockSelector > 0)
            {
                blockConfigs = blockConfigs.Skip(BlockSelector - 1).Take(1);
            }

            _blockConfigMap = blockConfigs.ToDictionary(b => b.Block);

            // initialize block metrics manager.
            _blockMetricManager = new BlockMetricsManager(
                context,
                (IEnumerable<ITradingObject> objects) => { return new GroupAverage(objects, "BAR.CP"); });

            _blockToPriceIndexChangeRateMap = context.RelationshipManager.Blocks.ToDictionary(b => b, b => new RateOfChange(WindowSize));
            _blockToLowestMap = context.RelationshipManager.Blocks.ToDictionary(b => b, b => new Lowest(WindowSize));

            _blockMetricManager.AfterUpdatedMetrics += UpdatePriceIndex;
        }

        private IEnumerable<BlockConfig> LoadBlockConfiguration(string configFile)
        {
            using(var reader = new StreamReader(configFile, Encoding.UTF8))
            {
                using (var csvReader = new CsvReader(reader))
                {
                    return csvReader.GetRecords<BlockConfig>().ToList();
                }
            }
        }

        private double GetBlockUpRateFromLowest(string block)
        {
            var indexValue = _blockMetricManager.GetMetricForBlock(block).MetricValues[0];
            var indexLowestValue = _blockToLowestMap[block].Value;

            var upRateFromLowest = (indexValue - indexLowestValue) / indexLowestValue * 100.0;

            return upRateFromLowest;
        }

        public override bool CanEnter(ITradingObject tradingObject, out string comments, out object obj)
        {
            comments = string.Empty;
            obj = null;

            if (Context.RelationshipManager == null)
            {
                return true;
            }

            var blocks = Context.RelationshipManager.GetBlocksForStock(tradingObject.Code);

            // if the stock's blocks has no intersect with blocks in config, we ignore the stock.
            var intersectedBlocks = _blockConfigMap.Keys.Intersect(blocks);
            if (!intersectedBlocks.Any())
            {
                return false;
            }

            foreach (var block in intersectedBlocks)
            {
                BlockConfig blockConfig = _blockConfigMap[block];

                var upRateFromLowest = GetBlockUpRateFromLowest(block);

                if (upRateFromLowest < blockConfig.MinimumUpRate
                    || upRateFromLowest > blockConfig.MaximumUpRate)
                {
                    return false;
                }
            }

            foreach (var block in blocks)
            {
                var indexRateOfChange = _blockToPriceIndexChangeRateMap[block].Value;

                if (indexRateOfChange > MininumRateOfChange)
                {
                    comments = string.Format(
                        "Block {0} price index change rate {1:0.000}", 
                        block, 
                        indexRateOfChange);

                    obj = new BlockUpRatesFromLowestForCode()
                    {
                        Code = tradingObject.Code,
                        BlockUpRatesFromLowest = blocks.ToDictionary(b => b, b => GetBlockUpRateFromLowest(b))
                    };

                    return true;
                }
            }

            return false;
        }

        private void UpdatePriceIndex()
        {
            foreach (var block in _blockToPriceIndexChangeRateMap.Keys)
            {
                var value = _blockMetricManager.GetMetricForBlock(block).MetricValues[0];

                _blockToPriceIndexChangeRateMap[block].Update(value);
                _blockToLowestMap[block].Update(value);
            }
        }
    }
}
