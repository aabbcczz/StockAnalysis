using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

using TradingStrategy;
using TradingStrategyEvaluation;
using StockAnalysis.Share;
using CsvHelper;

namespace DTViewer
{
    public partial class MainForm : Form
    {
        private const string ViewerSettingsFileName = "ViewerSettings.xml";

        private ChinaStockDataSettings _stockDataSettings = null;

        private ViewerSettings _viewerSettings = null;

        private Position[] _closedPositions = null;

        private Dictionary<Position, string[]> _positionDetails = new Dictionary<Position, string[]>();

        private StockNameTable _stockNameTable = null;

        private StockHistoryData _currentShownData = null;

        public MainForm()
        {
            InitializeComponent();

            dataGridViewCodes.AutoGenerateColumns = false;
            ColumnCodesCode.DataPropertyName = "Code";
            ColumnCodesName.DataPropertyName = "Name";

            dataGridViewClosedPosition.AutoGenerateColumns = false;
            ColumnPositionCode.DataPropertyName = "Code";
            ColumnPositionBuyTime.DataPropertyName = "BuyTime";
            ColumnPositionBuyPrice.DataPropertyName = "BuyPrice";
            ColumnPositionSellTime.DataPropertyName = "SellTime";
            ColumnPositionSellPrice.DataPropertyName = "SellPrice";

            // initialize data accessor (cache)
            ChinaStockDataAccessor.Initialize();
        }

        private void ShowError(string error, Exception e = null)
        {
            string message = e != null 
                ? error + Environment.NewLine + e.ToString() 
                : error;

            MessageBox.Show(
                message, 
                "Error", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Error);
        }

        private void loadDataSettingsFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // load data settings file
            openFileDialog1.FilterIndex = 1;
            if (!string.IsNullOrEmpty(_viewerSettings.LastDataSettingsFileName))
            {
                openFileDialog1.FileName = _viewerSettings.LastDataSettingsFileName;
            }

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    LoadDataSettingsFile(openFileDialog1.FileName);

                    // save the last data settings file name
                    _viewerSettings.LastDataSettingsFileName = openFileDialog1.FileName;
                }
                catch (Exception ex)
                {
                    ShowError("Failed to load data settings file", ex);
                }
            }
        }

        private void LoadDataSettingsFile(string fileName)
        {
            _stockDataSettings = ChinaStockDataSettings.LoadFromFile(fileName);

            _stockNameTable = new StockNameTable(_stockDataSettings.StockNameTableFile);

            // fill the codes and names to grid view
            var stockProperties = _stockNameTable.StockNames
                .Select(sn => new StockProperty()
                        {
                            Code = sn.Code,
                            Name = string.Join("|", sn.Names)
                        })
                .OrderBy(sp => sp.Code)
                .ToArray();

            dataGridViewCodes.DataSource = new SortableBindingList<StockProperty>(stockProperties);

            // re-initialize data accessor (cache)
            ChinaStockDataAccessor.Initialize();
        }

        private void LoadClosedPositionFile(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName, Encoding.UTF8))
            {
                using (CsvReader csvReader = new CsvReader(reader))
                {
                    _closedPositions = csvReader.GetRecords(typeof(Position))
                        .Select(o => (Position)o)
                        .ToArray();

                    _positionDetails.Clear();
                }
            }

            // fill the positions to grid view
            dataGridViewClosedPosition.DataSource = new SortableBindingList<Position>(_closedPositions);
        }

        private void loadClosedPositionFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // load data settings file
            openFileDialog1.FilterIndex = 2;
            if (!string.IsNullOrEmpty(_viewerSettings.LastClosedPositionFileName))
            {
                openFileDialog1.FileName = _viewerSettings.LastClosedPositionFileName;
            }

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    LoadClosedPositionFile(openFileDialog1.FileName);

                    // save the last closed position file name
                    _viewerSettings.LastClosedPositionFileName = openFileDialog1.FileName;
                }
                catch (Exception ex)
                {
                    ShowError("Failed to load closed position file", ex);
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                _viewerSettings = ViewerSettings.LoadFromFile(ViewerSettingsFileName);
            }
            catch (Exception ex)
            {
                _viewerSettings = new ViewerSettings();
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                _viewerSettings.SaveToFile(ViewerSettingsFileName);
            }
            catch (Exception ex)
            {
                // ignore the error;
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            try
            {
                if (_viewerSettings != null)
                {
                    if (!string.IsNullOrEmpty(_viewerSettings.LastDataSettingsFileName))
                    {
                        LoadDataSettingsFile(_viewerSettings.LastDataSettingsFileName);
                    }

                    // don't load position automatically
                    //if (!string.IsNullOrEmpty(_viewerSettings.LastClosedPositionFileName))
                    //{
                    //    LoadClosedPositionFile(_viewerSettings.LastClosedPositionFileName);
                    //}
                }
            }
            catch (Exception ex)
            {
                // ignore error;
            }
        }

        private void dataGridViewClosedPosition_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewClosedPosition.SelectedRows != null
                && dataGridViewClosedPosition.SelectedRows.Count > 0)
            {
                Position position = (Position)dataGridViewClosedPosition.SelectedRows[0].DataBoundItem;

                if (position == null)
                {
                    return;
                }

                string[] lines;
                if (!_positionDetails.TryGetValue(position, out lines))
                {
                    lines = new string[]
                    {
                        string.Format("Buy Action: {0}", position.BuyAction),
                        string.Format("Sell Action: {0}", position.SellAction),
                        string.Format("Buy Commission: {0:0.000}", position.BuyCommission),
                        string.Format("Sell Commission: {0:0.000}", position.SellCommission),
                        string.Format("Initial Risk: {0:0.000}", position.InitialRisk),
                        string.Format("Stoploss Price: {0:0.000}", position.StopLossPrice),
                        position.Comments
                    };

                    _positionDetails.Add(position, lines);
                }

                textBoxDetails.Lines = lines;

                // show stock data
                ShowStockData(position.Code, position.BuyTime, position.SellTime);
            }
        }

        private void ShowStockData(string code, DateTime startTime, DateTime endTime)
        {
            string file = _stockDataSettings.BuildActualDataFilePathAndName(code);

            StockHistoryData data;
            try
            {
                data = ChinaStockDataAccessor.Load(file, _stockNameTable);
            }
            catch (Exception ex)
            {
                ShowError("Load data file failed", ex);
                return;
            }

            var stockSeries = chartData.Series["stockSeries"];
            var chartArea = chartData.ChartAreas["ChartArea1"];

            var bars = data.DataOrderedByTime;

            if (bars == null || bars.Count() == 0)
            {
                stockSeries.Points.Clear();
                return;
            }

            if (data != _currentShownData)
            {
                stockSeries.Points.Clear();

                var labels = chartArea.AxisX.CustomLabels;

                for (int i = 0; i < bars.Length; ++i)
                {
                    var bar = bars[i];
                    stockSeries.Points.AddXY(
                        bar.Time, 
                        bar.HighestPrice,
                        bar.LowestPrice,
                        bar.OpenPrice,
                        bar.ClosePrice);
                }
            }

            if (startTime < bars[0].Time)
            {
                startTime = bars[0].Time;
            }
            
            if (endTime > bars[bars.Length - 1].Time)
            {
                endTime = bars[bars.Length - 1].Time;
            }

            chartArea.AxisX.ScaleView = new System.Windows.Forms.DataVisualization.Charting.AxisScaleView()
            {
                Position = 0,
                Size = 200
            };

            AdjustChartView();
        }

        private void ShowStockData(string code)
        {
            ShowStockData(code, DateTime.MinValue, DateTime.MaxValue);
        }

        private void dataGridViewCodes_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewCodes.SelectedRows != null 
                && dataGridViewCodes.SelectedRows.Count > 0)
            {
                string code = ((StockProperty)dataGridViewCodes.SelectedRows[0].DataBoundItem).Code;

                ShowStockData(code);
            }
        }

        private void AdjustChartView()
        {
            var stockSeries = chartData.Series["stockSeries"];
            var chartArea = chartData.ChartAreas["ChartArea1"];

            if (double.IsNaN(chartArea.AxisX.ScaleView.Position))
            {
                return;
            }

            int min = (int)chartArea.AxisX.ScaleView.Position;
            int max = (int)(chartArea.AxisX.ScaleView.Position + chartArea.AxisX.ScaleView.Size);

            if (max > stockSeries.Points.Count)
            {
                max = stockSeries.Points.Count;
            }

            var points = stockSeries.Points.Skip(min).Take(max - min);

            var minValue = points.Min(x => x.YValues[1]);
            var maxValue = points.Max(x => x.YValues[0]);

            chartArea.AxisY.Minimum = minValue;
            chartArea.AxisY.Maximum = maxValue;
        }

        private void chartData_AxisViewChanged(object sender, System.Windows.Forms.DataVisualization.Charting.ViewEventArgs e)
        {
            var chartArea = chartData.ChartAreas["ChartArea1"];

            if (e.Axis == e.ChartArea.AxisX)
            {
                AdjustChartView();
            }
        }
    }
}
