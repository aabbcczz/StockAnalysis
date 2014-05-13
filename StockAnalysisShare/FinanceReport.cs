using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalysis.Share
{
    public class FinanceReport
    {
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string Annotations { get; set; }

        public IEnumerable<FinanceReportTable> Tables
        {
            get { return _tables; }
        }

        private List<FinanceReportTable> _tables = new List<FinanceReportTable>();

        public void AddTable(FinanceReportTable table)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }

            _tables.Add(table);
        }

        public void Normalize(DataDictionary dictionary)
        {
            foreach (var table in _tables)
            {
                table.Normalize(dictionary);
            }
        }

        public void ExpandAndMerge(string tableName, IList<string> orderedRowNames, IList<string> orderedColumnText, IList<DateTime> orderedColumnDate)
        {
            // find out tables with given name
            var tables = _tables.Where(t => t.Name == tableName).ToArray();

            if (tables.Count() == 0)
            {
                return;
            }

            // remove the tables from report's table list
            foreach (var table in tables)
            { 
                _tables.Remove(table);
            }

            // expand corresponding tables
            var expandedTables = tables.Select(t => t.Expand(orderedRowNames, orderedColumnText, orderedColumnDate));

            // merge expanded tables
            var firstTable = expandedTables.First();
            foreach (var table in expandedTables.Skip(1))
            {
                firstTable.Merge(table);
            }

            // add back merged table to report
            AddTable(firstTable);
        }
    }
}
