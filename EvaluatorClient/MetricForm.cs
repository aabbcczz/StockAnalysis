using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using TradingStrategy;

namespace EvaluatorClient
{
    public partial class MetricForm : Form
    {
        TradeMetric _metric = null;

        public MetricForm()
        {
            InitializeComponent();

            transactionDataGridView.AutoGenerateColumns = false;

            timeTransactionDataGridColumn.DataPropertyName = "Time";
            codeTransactionDataGridColumn.DataPropertyName = "Code";
            actionTransactionDataGridColumn.DataPropertyName = "Action";
            priceTransactionDataGridColumn.DataPropertyName = "Price";
            volumeTransactionDataGridColumn.DataPropertyName = "Volume";
        }

        public void SetMetric(TradeMetric metric, ITradingDataProvider provider)
        {
            if (metric == null)
            {
                // clear all data
                transactionDataGridView.DataSource = null;
            }
            else
            {
                if (!object.ReferenceEquals(_metric, metric))
                {
                    transactionDataGridView.DataSource = metric.OrderedTransactionSequence.Select(t => new TransactionSlim(t)).ToList();
                }
            }

            _metric = metric;
        }

        private void MetricForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;
        }
    }
}
