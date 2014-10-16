using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using TradingStrategy;
using TradingStrategyEvaluation;

namespace EvaluatorClient
{
    public partial class MetricForm : Form
    {
        TradeMetric _metric;

        public MetricForm()
        {
            InitializeComponent();

            transactionDataGridView.AutoGenerateColumns = false;

            timeTransactionDataGridColumn.DataPropertyName = "Time";
            codeTransactionDataGridColumn.DataPropertyName = "Code";
            actionTransactionDataGridColumn.DataPropertyName = "Action";
            priceTransactionDataGridColumn.DataPropertyName = "Price";
            volumeTransactionDataGridColumn.DataPropertyName = "Volume";
            commentsTransactionDataGridColumn.DataPropertyName = "Comments";
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
            var equitySeries = metricChart.Series["equitySeries"];
            var priceSeries = metricChart.Series["priceSeries"];

            if (metric == null)
            {
                // clear all data
                equitySeries.Points.Clear();
                priceSeries.Points.Clear();
            }
            else
            {
                if (!ReferenceEquals(_metric, metric))
                {
                    equitySeries.Points.Clear();
                    foreach (var equityPoint in metric.OrderedEquitySequence)
                    {
                        equitySeries.Points.AddXY(equityPoint.Time, equityPoint.Equity);
                    }

                    priceSeries.Points.Clear();

                    if (metric.Code != TradeMetric.CodeForAll)
                    {
                        var index = provider.GetIndexOfTradingObject(metric.Code);
                        var bars = provider.GetAllBarsForTradingObject(index)
                            .Where(bar => bar.Time >= metric.StartDate && bar.Time <= metric.EndDate);

                        foreach (var bar in bars)
                        {
                            priceSeries.Points.AddXY(
                                bar.Time,
                                bar.HighestPrice,
                                bar.LowestPrice,
                                bar.OpenPrice,
                                bar.ClosePrice);
                        }
                    }
                }
            }
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
                if (!ReferenceEquals(_metric, metric))
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
                if (!ReferenceEquals(_metric, metric))
                {
                    var lines = new List<string>
                    {
                        string.Format("代码 ： {0}", metric.Code),
                        string.Format("名称 ： {0}", metric.Name),
                        string.Format("起始日期： {0:yyyy-MM-dd}", metric.StartDate),
                        string.Format("终止日期： {0:yyyy-MM-dd}", metric.EndDate),
                        string.Format("总交易天数： {0}", metric.TotalTradingDays),
                        string.Format("期初权益： {0:0.00}", metric.InitialEquity),
                        string.Format("期末权益： {0:0.00}", metric.FinalEquity),
                        string.Format("总周期数： {0}", metric.TotalPeriods),
                        string.Format("盈利周期数： {0}", metric.ProfitPeriods),
                        string.Format("亏损周期数： {0}", metric.LossPeriods),
                        string.Format("盈亏周期比： {0:0.00}", metric.ProfitLossPeriodRatio),
                        string.Format("总盈利： {0:0.00}", metric.TotalProfit),
                        string.Format("总亏损： {0:0.00}", metric.TotalLoss),
                        string.Format("总手续费： {0:0.00}", metric.TotalCommission),
                        string.Format("净利润： {0:0.00}", metric.NetProfit),
                        string.Format("收益率： {0:0.00}%", metric.ProfitRatio*100.0),
                        string.Format("年化收益率： {0:0.00}%", metric.AnnualProfitRatio*100.0),
                        string.Format("总交易次数： {0}", metric.TotalTradingTimes),
                        string.Format("盈利交易次数： {0}", metric.ProfitTradingTimes),
                        string.Format("亏损交易次数： {0}", metric.LossTradingTimes),
                        string.Format("盈利系数： {0:0.00}", metric.ProfitCoefficient),
                        string.Format("胜率 ： {0:0.00}%", metric.ProfitTimesRatio*100.0),
                        string.Format("亏损比率： {0:0.00}%", metric.LossTimesRatio*100.0),
                        string.Format("盈亏次数比： {0:0.00}", metric.ProfitLossTimesRatio),
                        string.Format("总交易量： {0}", metric.TotalVolume),
                        string.Format("盈利交易量： {0}", metric.ProfitVolume),
                        string.Format("亏损交易量： {0}", metric.LossVolume),
                        string.Format("平均每股盈利： {0:0.00}", metric.AverageProfitPerVolume),
                        string.Format("平均每股亏损： {0:0.00}", metric.AverageLossPerVolume),
                        string.Format("最大回撤： {0:0.00}", metric.MaxDrawDown),
                        string.Format("最大回撤比率： {0:0.00}%", metric.MaxDrawDownRatio*100.0),
                        string.Format("最大回撤起始时间： {0:yyyy-MM-dd}", metric.MaxDrawDownStartTime),
                        string.Format("最大回撤终止时间： {0:yyyy-MM-dd}", metric.MaxDrawDownEndTime),
                        string.Format("最大回撤期初权益： {0:0.00}", metric.MaxDrawDownStartEquity),
                        string.Format("最大回撤期末权益： {0:0.00}", metric.MaxDrawDownEndEquity),
                        string.Format("年化收益风险比率： {0:0.00}", metric.Mar),
                        string.Format("单次最大盈利： {0:0.00}", metric.MaxProfitInOneTransaction),
                        string.Format("单次最大亏损： {0:0.00}", metric.MaxLossInOneTransaction),
                        string.Format("扣除单次最大盈利后收益率： {0:0.00}%", metric.ProfitRatioWithoutMaxProfit*100.0),
                        string.Format("扣除单次最大亏损后收益率： {0:0.00}%", metric.ProfitRatioWithoutMaxLoss*100.0),
                        string.Format("期初价格： {0:0.00}", metric.StartPrice),
                        string.Format("期末价格： {0:0.00}", metric.EndPrice),
                        string.Format("区间涨幅： {0:0.00}%", metric.Rise*100.0)
                    };

                    detailsTextBox.Lines = lines.ToArray();
                }
            }
        }

        private void MetricForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Visible = false;
        }
    }
}
