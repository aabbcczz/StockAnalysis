﻿namespace ImportTable
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Data.SqlClient;
    using CommandLine;
    using Properties;
    using StockAnalysis.Common.Utility;

    static class Program
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

        private static void Run(Options options)
        {
            options.Print(Console.Out);

            var tableName = Path.GetFileNameWithoutExtension(options.CsvFile);
            var csv = CsvTable.Load(options.CsvFile, Encoding.UTF8, options.Separator);

            using (var connection = new SqlConnection(Settings.Default.stockConnectionString))
            {
                connection.Open();

                if (options.DropTable)
                {
                    // drop table
                    var cmdstring = BuildDropTableSql(tableName);
                    var cmd = new SqlCommand(cmdstring, connection);
                    cmd.ExecuteNonQuery();

                    // create table
                    cmdstring = BuildCreateTableSql(tableName, csv.Header);
                    cmd = new SqlCommand(cmdstring, connection);
                    cmd.ExecuteNonQuery();

                    // create index if necessary
                    cmdstring = BuildCreateIndexSql(tableName, "Index1", csv.Header);
                    if (!string.IsNullOrEmpty(cmdstring))
                    {
                        cmd = new SqlCommand(cmdstring, connection);
                        cmd.ExecuteNonQuery();
                    }
                }

                // insert values
                for (var i = 0; i < csv.RowCount; ++i)
                {
                    var cmdstring = BuildInsertRowSql(tableName, csv[i]);
                    var cmd = new SqlCommand(cmdstring, connection);
                    
                    cmd.ExecuteNonQuery();

                    if (i % 100 == 0)
                    {
                        Console.Write(".");
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("Done.");
        }

        private static string BuildInsertRowSql(string tableName, string[] row)
        {
            // INSERT INTO [dbo].[table] ( "a", "b", "c" )
            var builder = new StringBuilder();

            builder.AppendFormat("INSERT INTO [dbo].[{0}] VALUES (", tableName);

            for (var i = 0; i < row.Length; ++i)
            {
                if (i > 0)
                {
                    builder.Append(",");
                }

                builder.AppendFormat("N'{0}'", row[i]);
            }

            builder.Append(")");

            return builder.ToString();
        }

        private static string BuildDropTableSql(string tableName)
        {
            // IF EXISTS ( SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '[table_name]')DROP TABLE [table_name]
            return string.Format("IF EXISTS ( SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{0}' ) DROP TABLE [dbo].[{0}]", tableName);
        }

        private static string BuildCreateIndexSql(string tableName, string indexName, string[] columns)
        {
            var primaryKeys = GetPrimaryKeys(columns);

            if (primaryKeys != null && primaryKeys.Length > 1)
            {
                //CREATE INDEX [CodeIndex] ON [dbo].[Table] ([Code])

                return string.Format("CREATE INDEX [{0}] ON [dbo].[{1}] ([{2}])", indexName, tableName, primaryKeys[0]);
            }

            return string.Empty;
        }

        private static string BuildCreateTableSql(string tableName, string[] columns)
        {
            //CREATE TABLE [dbo].[Table]
            //(
            //    [symbol] NVARCHAR(50) NOT NULL , 
            //    [column] NCHAR(50) NOT NULL, 
            //    PRIMARY KEY ([symbol], [column])
            //)

            var builder = new StringBuilder();

            builder.AppendFormat("CREATE TABLE [dbo].[{0}] (", tableName);

            for (var i = 0; i < columns.Length; ++i)
            {
                if (i != 0)
                {
                    builder.Append(",");
                }

                builder.AppendFormat("[{0}] NVARCHAR(50)", columns[i]);

                builder.Append(IsColumnNullable(columns[i]) ? "NULL" : "NOT NULL");
            }

            var primaryKeys = GetPrimaryKeys(columns);
            if (primaryKeys != null && primaryKeys.Length > 0)
            {
                builder.Append(", PRIMARY KEY (");

                for (var j = 0; j < primaryKeys.Length; ++j)
                {
                    if (j > 0)
                    {
                        builder.Append(",");
                    }

                    builder.AppendFormat("[{0}]", primaryKeys[j]);
                }

                builder.Append(")");
            }

            builder.Append(")");

            return builder.ToString();
        }

        private static bool IsColumnNullable(string column)
        {
            if (column.ToLower() == "symbol"
                || column.ToLower() == "periodorcolumn")
            {
                return false;
            }

            return true;
        }

        private static string[] GetPrimaryKeys(string[] columns)
        {
            var primaryKeys = new List<string>();

            columns = columns.Select(s => s.ToLower()).ToArray();

            if (Array.IndexOf(columns, "symbol") >= 0)
            {
                primaryKeys.Add("symbol");
            }
            
            if (Array.IndexOf(columns, "periodorcolumn") >= 0)
            {
                primaryKeys.Add("periodorcolumn");
            }

            return primaryKeys.ToArray();
        }
    }
}
