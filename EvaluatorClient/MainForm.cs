using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;

using TradingStrategy;
using StockAnalysis.Share;

namespace EvaluatorClient
{
    public partial class MainForm : Form
    {
        StockNameTable _stockNameTable = null;

        ITradingDataProvider _activeDataProvider = null;

        MetricForm _metricForm;

        public MainForm()
        {
            InitializeComponent();

            // initialize resultDataGridView
            resultDataGridView.AutoGenerateColumns = false;

            codeResultDataGridViewColumn.DataPropertyName = "Code";
            nameResultDataGridViewColumn.DataPropertyName = "Name";
            profitTimesResultDataGridViewColumn.DataPropertyName = "ProfitTimes";
            totalTimesResultDataGridViewColumn.DataPropertyName = "TotalTimes";
            winRatioResultDataGridViewColumn.DataPropertyName = "WinRatio";
            commissionResultDataGridViewColumn.DataPropertyName = "Commission";
            netProfitResultDataGridViewColumn.DataPropertyName = "NetProfit";
            profitRatioResultDataGridViewColumn.DataPropertyName = "ProfitRatio";
            annualProfitRatioResultDataGridViewColumn.DataPropertyName = "AnnualProfitRatio";
            maxDrawDownResultDataGridViewColumn.DataPropertyName = "MaxDrawDown";
            maxDrawDownRatioResultDataGridViewColumn.DataPropertyName = "MaxDrawDownRatio";

            // initialize parameterDataGridView
            parameterDataGridView.AutoGenerateColumns = false;
            nameParameterDataGridViewColumn.DataPropertyName = "Name";
            descriptionParameterDataGridViewColumn.DataPropertyName = "Description";
            typeParameterDataGridViewColumn.DataPropertyName = "ParameterType";
            valueParameterDataGridViewColumn.DataPropertyName = "Value";

            // create metric form
            _metricForm = new MetricForm();
            _metricForm.Visible = false;
        }

        private void ShowError(string error, Exception exception = null)
        {
            string message = exception == null ? error : error + string.Format("\nException: {0}", exception);

            MessageBox.Show(
                message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private void conditionsTabControl_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage == resultPage)
            {
                exportButton.Visible = resultDataGridView.RowCount > 0;
            }
            else
            {
                exportButton.Visible = false;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // initialize strategy loader
            try
            {
                LoadStrategies();

                // initialize data source bindings
                openLongOptionComboBox.DataSource = TradingPriceOptionBinding.CreateBindings();
                openLongOptionComboBox.DisplayMember = "Text";
                openLongOptionComboBox.ValueMember = "Option";

                closeLongOptionComboBox.DataSource = TradingPriceOptionBinding.CreateBindings();
                closeLongOptionComboBox.DisplayMember = "Text";
                closeLongOptionComboBox.ValueMember = "Option";

                // load and apply trading settings from configuration file if any;
                ApplyTradingSettings(LoadTradingSettings());

                // initialize evaluation time span
                startDateTimePicker.Value = DateTime.Today.AddYears(-1);
                endDateTimePicker.Value = DateTime.Today;

                // load all possible stocks
                LoadStocks();
            }
            catch (Exception ex)
            {
                ShowError("Initialization failed, please fix the problem and restart the program", ex);
            }
        }

        private void LoadStrategies()
        {
            StrategyLoader.Initialize();

            foreach (var strategy in StrategyLoader.Strategies)
            {
                iTradingStrategyBindingSource.Add(strategy);
            }

            if (StrategyLoader.Strategies.Count() > 0)
            {
                strategyComboBox.SelectedIndex = 0;
                // force update
                strategyComboBox_SelectedIndexChanged(strategyComboBox, new EventArgs());
            }
        }

        private ITradingStrategy BuildStrategy(bool warning)
        {
            if (strategyComboBox.SelectedIndex < 0)
            {
                if (warning)
                {
                    ShowError("Please select strategy.");
                }

                return null;
            }

            try
            {
                ITradingStrategy strategy = (ITradingStrategy)strategyComboBox.SelectedItem;

                // create the copy of strategy
                ITradingStrategy copyStrategy = (ITradingStrategy)Activator.CreateInstance(strategy.GetType());

                return copyStrategy;
            }
            catch (Exception ex)
            {
                if (warning)
                {
                    ShowError("Build strategy failed", ex);
                }

                return null;
            }
        }

        private void LoadStocks()
        {
            _stockNameTable = null;

            string stockNameFile = Properties.Settings.Default.stockNameFile;

            try
            {
                _stockNameTable = new StockNameTable(stockNameFile);
            }
            catch (Exception ex)
            {
                ShowError(
                    string.Format("Failed to load file \"{0}\".", stockNameFile), 
                    ex);
            }

            if (_stockNameTable != null)
            {
                availableObjectListView.BeginUpdate();

                foreach (var stockName in _stockNameTable.StockNames)
                {
                    availableObjectListView.Items.Add(
                        new ListViewItem(
                            new string[] 
                            { 
                                stockName.Code, 
                                stockName.Names[0] 
                            }));
                }

                availableObjectListView.EndUpdate();
            }
        }

        private TradingSettings LoadTradingSettings()
        {
            string file = Properties.Settings.Default.tradingSettingsFile;
            TradingSettings settings = null;

            if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(TradingSettings));

                    using (StreamReader reader = new StreamReader(file))
                    {
                        settings = (TradingSettings)serializer.Deserialize(reader);
                    }

                    if (settings.BuyingCommission.Type != settings.SellingCommission.Type)
                    {
                        throw new InvalidDataException("Commission types of buying and selling are different");
                    }
                }
                catch (Exception ex)
                {
                    ShowError(
                        string.Format("Failed to load file {0}.", file),
                        ex);
                }
            }

            if (settings == null)
            {
                settings = new TradingSettings()
                {
                    BuyingCommission = new CommissionSettings()
                    {
                        Type = CommissionSettings.CommissionType.ByAmount,
                        Tariff = 0.0005
                    },

                    SellingCommission = new CommissionSettings()
                    {
                        Type = CommissionSettings.CommissionType.ByAmount,
                        Tariff = 0.0005
                    },

                    Spread = 0,

                    OpenLongPriceOption = TradingPriceOption.NextOpenPrice,

                    CloseLongPriceOption = TradingPriceOption.NextOpenPrice,
                };
            }

            return settings;
        }

        private void ApplyTradingSettings(TradingSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            // apply settings to UI
            if (settings.BuyingCommission.Type == CommissionSettings.CommissionType.ByAmount)
            {
                chargeByAmountRadioButton.Checked = true;
                chargeByVolumeRadioButton.Checked = false;
                tariffTextBox.Text = "0";
                buyCommissionTextBox.Text = string.Format("{0:0.00}", settings.BuyingCommission.Tariff * 100);
                sellCommissionTextBox.Text = string.Format("{0:0.00}", settings.SellingCommission.Tariff * 100);
                
            }
            else
            {
                chargeByAmountRadioButton.Checked = false;
                chargeByVolumeRadioButton.Checked = true;
                tariffTextBox.Text = string.Format("{0:0.00}", settings.BuyingCommission.Tariff);
                buyCommissionTextBox.Text = "0.05";
                sellCommissionTextBox.Text = "0.05";
            }

            spreadTextBox.Text = settings.Spread.ToString();

            openLongOptionComboBox.SelectedValue = settings.OpenLongPriceOption;
            closeLongOptionComboBox.SelectedValue = settings.CloseLongPriceOption;
        }

        private TradingSettings BuildTradingSettings(bool warning)
        {
            // build trading settings from UI
            TradingSettings settings = new TradingSettings();

            try
            {

                if (chargeByAmountRadioButton.Checked)
                {
                    settings.BuyingCommission = new CommissionSettings()
                    {
                        Type = CommissionSettings.CommissionType.ByAmount,
                        Tariff = double.Parse(buyCommissionTextBox.Text) / 100.0,
                    };

                    settings.SellingCommission = new CommissionSettings()
                    {
                        Type = CommissionSettings.CommissionType.ByAmount,
                        Tariff = double.Parse(sellCommissionTextBox.Text) / 100.0,
                    };
                }
                else
                {
                    double tariff = double.Parse(tariffTextBox.Text);

                    settings.BuyingCommission = new CommissionSettings()
                    {
                        Type = CommissionSettings.CommissionType.ByVolume,
                        Tariff = tariff,
                    };

                    settings.SellingCommission = new CommissionSettings()
                    {
                        Type = CommissionSettings.CommissionType.ByVolume,
                        Tariff = tariff,
                    };
                }

                settings.Spread = int.Parse(spreadTextBox.Text);

                settings.OpenLongPriceOption = (TradingPriceOption)openLongOptionComboBox.SelectedValue;
                settings.CloseLongPriceOption = (TradingPriceOption)closeLongOptionComboBox.SelectedValue;

            }
            catch (Exception ex)
            {
                if (warning)
                {
                    ShowError("", ex);
                }

                return null;
            }

            return settings;
        }

        private void SaveTradingSettings(TradingSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            string file = Properties.Settings.Default.tradingSettingsFile;

            if (string.IsNullOrWhiteSpace(file))
            {
                return;
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TradingSettings));

                using (StreamWriter writer = new StreamWriter(file))
                {
                    serializer.Serialize(writer, settings);
                }
            }
            catch (Exception ex)
            {
                ShowError(
                    string.Format("Failed to save file {0}.", file),
                    ex);
            }
        }

        private void buyCommissionTextBox_Validating(object sender, CancelEventArgs e)
        {
            double value;

            if (!double.TryParse(buyCommissionTextBox.Text, out value) || value < 0.0)
            {
                // Cancel the event and select the text to be corrected by the user.
                e.Cancel = true;
                buyCommissionTextBox.Select(0, buyCommissionTextBox.Text.Length);

                // Set the ErrorProvider error with the text to display.  
                this.errorProvider1.SetError(buyCommissionTextBox, "费率需要为非负数字");
            }
        }

        private void sellCommissionTextBox_Validating(object sender, CancelEventArgs e)
        {
            double value;

            if (!double.TryParse(sellCommissionTextBox.Text, out value) || value < 0.0)
            {
                // Cancel the event and select the text to be corrected by the user.
                e.Cancel = true;
                sellCommissionTextBox.Select(0, sellCommissionTextBox.Text.Length);

                // Set the ErrorProvider error with the text to display.  
                this.errorProvider1.SetError(sellCommissionTextBox, "费率需要为非负数字");
            }
        }

        private void tariffTextBox_Validating(object sender, CancelEventArgs e)
        {
            double value;

            if (!double.TryParse(tariffTextBox.Text, out value) || value < 0.0)
            {
                // Cancel the event and select the text to be corrected by the user.
                e.Cancel = true;
                tariffTextBox.Select(0, tariffTextBox.Text.Length);

                // Set the ErrorProvider error with the text to display.  
                this.errorProvider1.SetError(tariffTextBox, "费率需要为非负数字");
            }
        }

        private void spreadTextBox_Validating(object sender, CancelEventArgs e)
        {
            int value;

            if (!int.TryParse(spreadTextBox.Text, out value) || value < 0)
            {
                // Cancel the event and select the text to be corrected by the user.
                e.Cancel = true;
                spreadTextBox.Select(0, spreadTextBox.Text.Length);

                // Set the ErrorProvider error with the text to display.  
                this.errorProvider1.SetError(spreadTextBox, "滑点需要为非负整数");
            }
        }

        private void initialCapitalTextBox_Validating(object sender, CancelEventArgs e)
        {
            double value;

            if (!double.TryParse(initialCapitalTextBox.Text, out value) || value < 0.0)
            {
                // Cancel the event and select the text to be corrected by the user.
                e.Cancel = true;
                initialCapitalTextBox.Select(0, initialCapitalTextBox.Text.Length);

                // Set the ErrorProvider error with the text to display.  
                this.errorProvider1.SetError(initialCapitalTextBox, "初始资金需要为非负整数");
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var settings = BuildTradingSettings(true);

            if (settings != null)
            {
                SaveTradingSettings(settings);
            }

            e.Cancel = false;
        }

        private void chargeByAmountRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            buyCommissionTextBox.Enabled = chargeByAmountRadioButton.Checked;
            sellCommissionTextBox.Enabled = chargeByAmountRadioButton.Checked;
        }

        private void chargeByVolumeRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            tariffTextBox.Enabled = chargeByVolumeRadioButton.Checked;
        }

        private void warmupTextBox_Validating(object sender, CancelEventArgs e)
        {
            int value;

            if (!int.TryParse(warmupTextBox.Text, out value) || value < 0.0)
            {
                // Cancel the event and select the text to be corrected by the user.
                e.Cancel = true;
                warmupTextBox.Select(0, spreadTextBox.Text.Length);

                // Set the ErrorProvider error with the text to display.  
                this.errorProvider1.SetError(warmupTextBox, "周期数需要为非负整数");
            }
        }

        private void control_Validated(object sender, EventArgs e)
        {
            this.errorProvider1.SetError((Control)sender, "");
        }

        private void time_Validating(object sender, CancelEventArgs e)
        {
            if (startDateTimePicker.Value >= endDateTimePicker.Value)
            {
                e.Cancel = true;
                startDateTimePicker.Value = endDateTimePicker.Value.AddYears(-1);

                this.errorProvider1.SetError((Control)sender, "起始时间必须小于终止时间");
            }
        }

        private void availableObjectListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            addObjectButton.Enabled = availableObjectListView.SelectedItems.Count > 0;
        }

        private void selectedObjectListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            removeObjectButton.Enabled = selectedObjectListView.SelectedItems.Count > 0;
        }

        private void addObjectButton_Click(object sender, EventArgs e)
        {
            selectedObjectListView.BeginUpdate();
            availableObjectListView.BeginUpdate();

            foreach (ListViewItem item in availableObjectListView.SelectedItems)
            {
                availableObjectListView.Items.Remove(item);
                selectedObjectListView.Items.Add(item);
            }

            selectedObjectListView.EndUpdate();
            availableObjectListView.EndUpdate();

            selectedObjectListView.SelectedItems.Clear();
        }

        private void removeObjectButton_Click(object sender, EventArgs e)
        {
            selectedObjectListView.BeginUpdate();
            availableObjectListView.BeginUpdate();

            foreach (ListViewItem item in selectedObjectListView.SelectedItems)
            {
                selectedObjectListView.Items.Remove(item);
                availableObjectListView.Items.Add(item);
            }

            selectedObjectListView.EndUpdate();
            availableObjectListView.EndUpdate();

            availableObjectListView.SelectedItems.Clear();
        }

        private void listView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                ListView view = (ListView)sender;
                foreach (ListViewItem item in view.Items)
                {
                    item.Selected = true;
                }
            }
        }

        private void evaluateButton_Click(object sender, EventArgs e)
        {
            if (_stockNameTable == null)
            {
                ShowError("No stock name is loaded");
                return;
            }

            string stockDataFileFolder = Properties.Settings.Default.stockDataFileFolder;
            if (!Directory.Exists(stockDataFileFolder))
            {
                ShowError(string.Format("Stock data file folder \"{0}\" does not exist", stockDataFileFolder));
            }

            string stockDataFileNamePattern = Properties.Settings.Default.stockDataFileNamePattern;
            if (stockDataFileNamePattern.IndexOf(Constants.StockCodePattern) < 0)
            {
                ShowError(string.Format("Stock data file name pattern does not contain {0}", Constants.StockCodePattern));
            }

            if (selectedObjectListView.Items.Count == 0)
            {
                return;
            }

            TradingSettings settings = BuildTradingSettings(true);
            if (settings == null)
            {
                return;
            }

            ITradingStrategy strategy = BuildStrategy(true);
            if (strategy == null)
            {
                return;
            }

            try
            {
                double initialCapital = double.Parse(initialCapitalTextBox.Text);

                List<string> files = new List<string>(selectedObjectListView.Items.Count);

                foreach (ListViewItem item in selectedObjectListView.Items)
                {
                    string code = item.Text;

                    string fileName = stockDataFileNamePattern.Replace(Constants.StockCodePattern, code);
                    fileName = Path.Combine(stockDataFileFolder, fileName);

                    if (File.Exists(fileName))
                    {
                        files.Add(fileName);
                    }
                }

                if (files.Count == 0)
                {
                    ShowError("No data file exists, please check the configuration");
                    return;
                }

                // clear previous log and results
                logTextBox.Clear();
                resultDataGridView.DataSource = null;

                // clear data provider
                _activeDataProvider = null;

                // force GC for fun
                GC.Collect();

                DateTime startDate = startDateTimePicker.Value.Date;
                DateTime endDate = endDateTimePicker.Value.Date;

                // create data provider
                ChinaStockDataProvider provider 
                    = new ChinaStockDataProvider(
                        _stockNameTable, 
                        files.ToArray(), 
                        startDate, 
                        endDate, 
                        int.Parse(warmupTextBox.Text));

                // create new logger;
                MemoryLogger logger = new MemoryLogger();

                // create evaluator
                IDictionary<ParameterAttribute, object> parameterValues = CollectParameterValues();

                TradingStrategyEvaluator evaluator
                    = new TradingStrategyEvaluator(
                        initialCapital, 
                        strategy, 
                        parameterValues, 
                        provider, 
                        settings,
                        logger);

                // reset progress bar and register progress updater.
                evaluationProgressBar.Minimum = 0;
                evaluationProgressBar.Maximum = 100;
                evaluationProgressBar.Value = 0;
                evaluator.OnEvaluationProgress += UpdateEvaluationProgress;

                // evalute strategy
                evaluator.Evaluate();

                // update log
                logTextBox.Clear();
                logTextBox.Lines = logger.Logs.ToArray();

                // calculate metrics
                MetricCalculator calculator 
                    = new MetricCalculator(
                        _stockNameTable,
                        evaluator.History,
                        provider);

                var metrics = calculator.Calculate().Select(tm => new TradeMetricSlim(tm));

                // show results
                SortableBindingList<TradeMetricSlim> results = new SortableBindingList<TradeMetricSlim>(metrics);
                resultDataGridView.DataSource = results;

                // switch to result page
                this.tabControl1.SelectedTab = resultPage; 

                // set active data provider
                if (metrics.Count() > 0)
                {
                    _activeDataProvider = provider;
                }
            }
            catch (Exception ex)
            {
                ShowError("Evaluation failed", ex);
            }
        }

        private IDictionary<ParameterAttribute, object> CollectParameterValues()
        {
            Dictionary<ParameterAttribute, object> parameterValues = new Dictionary<ParameterAttribute, object>();

            var attributes = parameterDataGridView.DataSource as List<ParameterAttributeSlim>;
            foreach (var attribute in attributes)
            {
                object value = ParameterHelper.ConvertStringToValue(attribute.Attribute, attribute.Value);

                parameterValues.Add(attribute.Attribute, value);
            }

            return parameterValues;
        }

        private void UpdateEvaluationProgress(object sender, EvaluationProgressEventArgs e)
        {
            evaluationProgressBar.Value = (int)(e.EvaluationPercentage * 100.0);
        }

        private void strategyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (strategyComboBox.SelectedIndex >= 0)
            {
                ITradingStrategy strategy = (ITradingStrategy)strategyComboBox.SelectedItem;

                StringBuilder builder = new StringBuilder();

                builder.Append("全类型名：\n");
                builder.Append(strategy.GetType().FullName);
                builder.Append("\n\n");
                builder.Append("策略描述：\n");
                builder.Append(strategy.Description);

                descriptionTextBox.Lines = builder.ToString().Split(new char[] {'\n'});

                var parameters = ParameterHelper.GetParameterAttributes(strategy)
                    .Select(p => new ParameterAttributeSlim(p))
                    .ToList();

                parameterDataGridView.DataSource = parameters;
            }
            else
            {
                descriptionTextBox.Clear();
            }
        }

        private void resultDataGridView_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            TradeMetricSlim obj = resultDataGridView.Rows[e.RowIndex].DataBoundItem as TradeMetricSlim;
            if (obj != null)
            {
                _metricForm.SetMetric(obj.Metric, _activeDataProvider);
            }

            _metricForm.Visible = true;
        }

        private void parameterDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            e.Cancel = false;

            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            var column = parameterDataGridView.Columns[e.ColumnIndex];

            if (column != valueParameterDataGridViewColumn)
            {
                return;
            }

            ParameterAttributeSlim obj = parameterDataGridView.Rows[e.RowIndex].DataBoundItem as ParameterAttributeSlim;

            if (!ParameterHelper.IsValidValue(obj.Attribute, (string)e.FormattedValue))
            {
                e.Cancel = true;
            }
        }
    }
}
