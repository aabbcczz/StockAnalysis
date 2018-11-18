namespace ConvertTdxBlockData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CommandLine;
    using StockAnalysis.Common.ChineseMarket;
    using StockAnalysis.TdxHelper;
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser(with => { with.HelpWriter = Console.Error; });

            var parseResult = parser.ParseArguments<Options>(args);

            if (parseResult.Errors.Any())
            {
                var helpText = CommandLine.Text.HelpText.AutoBuild(parseResult);
                Console.WriteLine("{0}", helpText);

                Environment.Exit(-2);
            }

            var options = parseResult.Value;

            options.BoundaryCheck();
            options.Print(Console.Out);

            Run(options);
        }

        static void Run(Options options)
        {
            List<StockBlockRelationship> relationships = new List<StockBlockRelationship>();

            // load block definitions
            var stockBlockManager = new StockBlockManager();
            var blockConfigReader = new TdxStockBlockConfigReader(options.BlockConfigFile);
            stockBlockManager.AddStockBlocks(blockConfigReader.Blocks);

            // try to load stock block relationships
            if (!string.IsNullOrEmpty(options.HangYeFile))
            {
                relationships.AddRange(
                    new TdxHangYeBlockDataReader(options.HangYeFile, stockBlockManager).Relationships);
             }

            if (!string.IsNullOrEmpty(options.GaiNianFile))
            {
                relationships.AddRange(
                    new TdxBinaryBlockDataReader(options.GaiNianFile).Relationships);
            }

            if (!string.IsNullOrEmpty(options.FengGeFile))
            {
                relationships.AddRange(
                    new TdxBinaryBlockDataReader(options.FengGeFile).Relationships);
            }

            if (!string.IsNullOrEmpty(options.ZhiShuFile))
            {
                relationships.AddRange(
                    new TdxBinaryBlockDataReader(options.ZhiShuFile).Relationships);
            }

            // expand relationships
            if (relationships.Count > 0)
            {
                relationships = stockBlockManager.ExpandRelationships(relationships).ToList();
            }

            // output
            StockBlockRelationship.SaveToFile(options.OutputFile, relationships);
        }
    }
}
