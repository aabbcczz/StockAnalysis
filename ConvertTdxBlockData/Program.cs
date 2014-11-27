using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using CommandLine;
using StockAnalysis.Share;

namespace ConvertTdxBlockData
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            var parser = new Parser(with => with.HelpWriter = Console.Error);

            if (parser.ParseArgumentsStrict(args, options, () => Environment.Exit(-2)))
            {
                options.BoundaryCheck();
                options.Print(Console.Out);

                Run(options);
            }
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
