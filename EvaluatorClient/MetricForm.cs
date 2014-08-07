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
            UpdateTransactionInformation(metric);
            UpdateMetricDetails(metric);
            UpdateChart(metric, provider);

            _metric = metric;
        }

        private void UpdateChart(TradeMetric metric, ITradingDataProvider provider)
        {

        }

        private void UpdateTransactionInformation(TradeMetric metric)
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
        }

        private void UpdateMetricDetails(TradeMetric metric)
        {
            if (metric == null)
            {
                // clear all data
                detailsTextBox.Clear();
            }
            else
            {
                if (!object.ReferenceEquals(_metric, metric))
                {
                    List<string> lines = new List<string>();
                    lines.Add(string.Format("代码      ： {0}", metric.Code));
                    lines.Add(string.Format("名称      ： {0}", metric.Name));
                    lines.Add(string.Format("起始日期    ： {0:yyyy-MM-dd}", metric.StartDate));
                    lines.Add(string.Format("终止日期    ： {0:yyyy-MM-dd}", metric.EndDate));
                    lines.Add(string.Format("总交易天数   ： {0}", metric.TotalTradingDays));
                    lines.Add(string.Format("期初权益    ： {0:0.00}", metric.InitialEquity));
                    lines.Add(string.Format("期末权益    ： {0:0.00}", metric.FinalEquity));
                    lines.Add(string.Format("总周期数    ： {0}", metric.TotalPeriods));
                    lines.Add(string.Format("盈利周期数   ： {0}", metric.ProfitPeriods));
                    lines.Add(string.Format("亏损周期数   ： {0}", metric.LossPeriods));
                    lines.Add(string.Format("盈亏周期比   ： {0:0.00}", metric.ProfitLossPeriodRatio));
                    lines.Add(string.Format("总盈利     ： {0:0.00}", metric.TotalProfit));
                    lines.Add(string.Format("总亏损     ： {0:0.00}", metric.TotalLoss));
                    lines.Add(string.Format("总手续费    ： {0:0.00}", metric.TotalCommission));
                    lines.Add(string.Format("净利润     ： {0:0.00}", metric.NetProfit));
                    lines.Add(string.Format("收益率     ： {0:0.00}%", metric.ProfitRatio * 100.0));
                    lines.Add(string.Format("年化收益率   ： {0:0.00}%", metric.AnnualProfitRatio * 100.0));
                    lines.Add(string.Format("总交易次数   ： {0}", metric.TotalTradingTimes));
                    lines.Add(string.Format("盈利交易次数  ： {0}", metric.ProfitTradingTimes));
                    lines.Add(string.Format("亏损交易次数  ： {0}", metric.LossTradingTimes));
                    lines.Add(string.Format("盈利系数    ： {0:0.00}", metric.ProfitCoefficient));
                    lines.Add(string.Format("胜率      ： {0:0.00}%", metric.ProfitTimesRatio * 100.0));
                    lines.Add(string.Format("亏损比率    ： {0:0.00}%", metric.LossTimesRatio * 100.0));
                    lines.Add(string.Format("盈亏次数比   ： {0:0.00}", metric.ProfitLossTimesRatio));
                    lines.Add(string.Format("总交易量    ： {0}", metric.TotalVolume));
                    lines.Add(string.Format("盈利交易量   ： {0}", metric.ProfitVolume));
                    lines.Add(string.Format("亏损交易量   ： {0}", metric.LossVolume));
                    lines.Add(string.Format("平均每股盈利  ： {0:0.00}", metric.AverageProfit));
                    lines.Add(string.Format("平均每股亏损  ： {0:0.00}", metric.AverageLoss));
                    lines.Add(string.Format("最大回撤    ： {0:0.00}", metric.MaxDrawDown));
                    lines.Add(string.Format("最大回撤比率  ： {0:0.00}%", metric.MaxDrawDownRatio * 100.0));
                    lines.Add(string.Format("最大回撤起始时间： {0:yyyy-MM-dd}", metric.MaxDrawDownStartTime));
                    lines.Add(string.Format("最大回撤终止时间： {0:yyyy-MM-dd}", metric.MaxDrawDownEndTime));
                    lines.Add(string.Format("最大回撤期初权益： {0:0.00}", metric.MaxDrawDownStartEquity));
                    lines.Add(string.Format("最大回撤期末权益： {0:0.00}", metric.MaxDrawDownEndEquity));
                    lines.Add(string.Format("年化收益风险比率： {0:0.00}", metric.MAR));
                    lines.Add(string.Format("单次最大盈利  ： {0:0.00}", metric.MaxProfitInOneTransaction));
                    lines.Add(string.Format("单次最大亏损  ： {0:0.00}", metric.MaxLossInOneTransaction));
                    lines.Add(string.Format("扣除单次最大盈利后收益率： {0:0.00}%", metric.ProfitRatioWithoutMaxProfit * 100.0));
                    lines.Add(string.Format("扣除单次最大亏损后收益率： {0:0.00}%", metric.ProfitRatioWithoutMaxLoss * 100.0));
                    lines.Add(string.Format("期初价格    ： {0:0.00}", metric.StartPrice));
                    lines.Add(string.Format("期末价格    ： {0:0.00}", metric.EndPrice));
                    lines.Add(string.Format("区间涨幅    ： {0:0.00}%", metric.Rise * 100.0));

                    detailsTextBox.Lines = lines.ToArray();
                }
            }
        }

        private void MetricForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;
        }
    }
}
