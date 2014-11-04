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
        private const int ProportionOfVolumeSpace = 6;
        private const int MinScaleViewSize = 100;
        private const int SurroundDataPointCount = 50;
        private const int StockSeriesIndex = 0;
        private const int VolumeSeriesIndex = 1;
        private const int ChartAreaIndex = 0;
        private const int StartAnnotationIndex = 0;
        private const int EndAnnotationIndex = 1;

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
            ColumnPositionVolume.DataPropertyName = "Volume";
            ColumnPositionGain.DataPropertyName = "Gain";
            ColumnPositionR.DataPropertyName = "R";

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

            // reset data accessor (cache)
            ChinaStockDataAccessor.Reset();
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
            var positionSlims = _closedPositions.Select(p => new PositionSlim(p)).ToArray();

            dataGridViewClosedPosition.DataSource = new SortableBindingList<PositionSlim>(positionSlims);
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
            catch
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
            catch
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
            catch
            {
                // ignore error;
            }
        }

        private void dataGridViewClosedPosition_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewClosedPosition.SelectedRows != null
                && dataGridViewClosedPosition.SelectedRows.Count > 0)
            {
                PositionSlim positionSlim = (PositionSlim)dataGridViewClosedPosition.SelectedRows[0].DataBoundItem;

                if (positionSlim == null)
                {
                    return;
                }

                textBoxDetails.Text = positionSlim.Annotation;

                // show stock data
                ShowStockData(positionSlim.Code, positionSlim.BuyTime, positionSlim.SellTime, true);
            }
        }

        private int GetIndexOfTimeInBars(Bar[] bars, DateTime time)
        {
            var compareObject = new Bar { Time = time };

            // index is the index of data whose Time >= time
            int index = Array.BinarySearch(bars, compareObject, new Bar.TimeComparer());
            if (index < 0)
            {
                // not found, ~index is the index of first data that is greater than value being searched.
                index = ~index;
            }

            if (index == bars.Length)
            {
                index = bars.Length - 1;
            }

            return index;
        }

        private void ShowStockData(string code, DateTime startTime, DateTime endTime, bool addAnnotation)
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

            // update label of code
            labelCode.Text = code;

            var stockSeries = chartData.Series[StockSeriesIndex];
            var volumeSeries = chartData.Series[VolumeSeriesIndex];
            var chartArea = chartData.ChartAreas[ChartAreaIndex];

            var bars = data.DataOrderedByTime;

            if (bars == null || bars.Count() == 0)
            {
                stockSeries.Points.Clear();
                volumeSeries.Points.Clear();
                return;
            }

            // add data to chart
            if (data != _currentShownData)
            {
                _currentShownData = data;

                stockSeries.Points.Clear();
                volumeSeries.Points.Clear();

                for (int i = 0; i < bars.Length; ++i)
                {
                    var bar = bars[i];
                    stockSeries.Points.AddXY(
                        bar.Time, 
                        bar.HighestPrice,
                        bar.LowestPrice,
                        bar.OpenPrice,
                        bar.ClosePrice);

                    volumeSeries.Points.AddXY(bar.Time, bar.Volume);
                }
            }

            // calculate the index of start time and end time in data
            int startIndex = GetIndexOfTimeInBars(bars, startTime);
            int endIndex = GetIndexOfTimeInBars(bars, endTime);

            if (startIndex > endIndex)
            {
                int temp = startIndex;
                startIndex = endIndex;
                endIndex = temp;
            }

            // create scale view to cover start time and end time
            int position = startIndex - SurroundDataPointCount;
            int size = endIndex - startIndex + SurroundDataPointCount * 2;

            position = Math.Max(0, position);
            size = Math.Min(Math.Max(MinScaleViewSize, size), bars.Length);

            if (size + position > bars.Length)
            {
                position = bars.Length - size;
            }

            chartArea.AxisX.ScaleView = new System.Windows.Forms.DataVisualization.Charting.AxisScaleView()
            {
                Position = position,
                Size = size
            };

            // adjust view to accomendate the scale
            AdjustChartView();

            // add annotation
            var startAnnotation = chartData.Annotations[StartAnnotationIndex];
            var endAnnotation = chartData.Annotations[EndAnnotationIndex];
            if (addAnnotation)
            {
                startAnnotation.AnchorX = startIndex + 1;
                endAnnotation.AnchorX = endIndex + 1;

                startAnnotation.Visible = true;
                endAnnotation.Visible = true;
            }
            else
            {
                startAnnotation.AnchorX = 0;
                endAnnotation.AnchorX = 0;

                startAnnotation.Visible = false;
                endAnnotation.Visible = false;
            }

            chartData.Invalidate();
            chartData.Update();
        }

        private void ShowStockData(string code)
        {
            ShowStockData(code, DateTime.MinValue, DateTime.MaxValue, false);
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
            var volumeSeries = chartData.Series[VolumeSeriesIndex];
            var stockSeries = chartData.Series[StockSeriesIndex];
            var chartArea = chartData.ChartAreas[ChartAreaIndex];

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

            var stockPoints = stockSeries.Points.Skip(min).Take(max - min);
            var volumePoints = volumeSeries.Points.Skip(min).Take(max - min);

            var minStockValue = stockPoints.Min(x => x.YValues[1]);
            var maxStockValue = stockPoints.Max(x => x.YValues[0]);

            // left space for volume
            minStockValue -= (maxStockValue - minStockValue) / (ProportionOfVolumeSpace - 1);

            chartArea.AxisY.Minimum = minStockValue;
            chartArea.AxisY.Maximum = maxStockValue;

            var maxVolumePoints = volumePoints.Max(x => x.YValues[0]);
            // keep volume on the bottom
            maxVolumePoints *= ProportionOfVolumeSpace;

            chartArea.AxisY2.Minimum = 0;
            chartArea.AxisY2.Maximum = maxVolumePoints;

        }

        private void chartData_AxisViewChanged(object sender, System.Windows.Forms.DataVisualization.Charting.ViewEventArgs e)
        {
            var chartArea = chartData.ChartAreas[ChartAreaIndex];

            if (e.Axis == e.ChartArea.AxisX)
            {
                AdjustChartView();
            }
        }

        private void buttonIncreaseShowedBars_Click(object sender, EventArgs e)
        {
            var chartArea = chartData.ChartAreas[ChartAreaIndex];
            var stockSeries = chartData.Series[StockSeriesIndex];

            if (chartArea.AxisX.ScaleView == null)
            {
                return;
            }

            int position = (int)chartArea.AxisX.ScaleView.Position;
            int size = (int)chartArea.AxisX.ScaleView.Size;

            size *= 2;

            int pointsCount = stockSeries.Points.Count;

            if (size >= pointsCount)
            {
                position = 0;
                size = pointsCount;
            }
            else
            {
                if (position + size >= pointsCount)
                {
                    position = pointsCount;
                }
            }

            chartArea.AxisX.ScaleView.Position = position;
            chartArea.AxisX.ScaleView.Size = size;

            AdjustChartView();
        }

        private void buttonDecreaseShownBars_Click(object sender, EventArgs e)
        {
            var chartArea = chartData.ChartAreas[ChartAreaIndex];
            var stockSeries = chartData.Series[StockSeriesIndex];

            if (chartArea.AxisX.ScaleView == null)
            {
                return;
            }

            int pointsCount = stockSeries.Points.Count;
            int position = (int)chartArea.AxisX.ScaleView.Position;
            int size = (int)chartArea.AxisX.ScaleView.Size;

            size /= 2;

            size = Math.Min(Math.Max(MinScaleViewSize, size), pointsCount);

            if (size == pointsCount)
            {
                position = 0;
            }

            chartArea.AxisX.ScaleView.Position = position;
            chartArea.AxisX.ScaleView.Size = size;

            AdjustChartView();
        }

        private double GetAxisXPositionFromMouse(int x)
        {
            var chartArea = chartData.ChartAreas[ChartAreaIndex];
            return chartArea.AxisX.PixelPositionToValue(x);
        }


        private void chartData_GetToolTipText(object sender, System.Windows.Forms.DataVisualization.Charting.ToolTipEventArgs e)
        {
            if (e.HitTestResult.ChartArea == chartData.ChartAreas[ChartAreaIndex])
            {
                double position = GetAxisXPositionFromMouse(e.X);
                int index = (int)(position - 0.5);

                if (index < 0 )
                {
                    index = 0;
                }

                var stockSeries = chartData.Series[StockSeriesIndex];

                if (position < 0 || position >= stockSeries.Points.Count)
                {
                    e.Text = string.Empty;
                }
                else
                {
                    var bar = _currentShownData.DataOrderedByTime[index];

                    e.Text = string.Format(
                        "Position:{6}\nTime:{0:yyyy-MM-dd}\nOpen:{1:0.000}\nClose:{2:0.000}\nHighest:{3:0.000}\nLowest:{4:0.000}\nVolume:{5}",
                        bar.Time,
                        bar.OpenPrice,
                        bar.ClosePrice,
                        bar.HighestPrice,
                        bar.LowestPrice,
                        bar.Volume,
                        position);
                }
            }
        }
    }
}
