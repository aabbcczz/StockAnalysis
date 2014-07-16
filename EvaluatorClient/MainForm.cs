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

using TradingStrategy;
using StockAnalysis.Share;

namespace EvaluatorClient
{
    public partial class MainForm : Form
    {
        StockNameTable _stockNameTable = null;

        public MainForm()
        {
            InitializeComponent();
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
                buyCommissionTextBox.Text = string.Format("{0:0.0000}", settings.BuyingCommission.Tariff * 100);
                sellCommissionTextBox.Text = string.Format("{0:0.0000}", settings.SellingCommission.Tariff * 100);
                
            }
            else
            {
                chargeByAmountRadioButton.Checked = false;
                chargeByVolumeRadioButton.Checked = true;
                tariffTextBox.Text = string.Format("{0:0.0000}", settings.BuyingCommission.Tariff);
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

            if (!int.TryParse(spreadTextBox.Text, out value) || value < 0.0)
            {
                // Cancel the event and select the text to be corrected by the user.
                e.Cancel = true;
                spreadTextBox.Select(0, spreadTextBox.Text.Length);

                // Set the ErrorProvider error with the text to display.  
                this.errorProvider1.SetError(spreadTextBox, "滑点需要为非负整数");
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

            if (selectedObjectListView.SelectedItems.Count == 0)
            {
                return;
            }

            TradingSettings settings = BuildTradingSettings(true);
            if (settings == null)
            {
                return;
            }

            try
            {
                List<string> files = new List<string>(selectedObjectListView.SelectedItems.Count);

                foreach (ListViewItem item in selectedObjectListView.SelectedItems)
                {
                    string code = item.Text;

                    string fileName = stockDataFileNamePattern.Replace(Constants.StockCodePattern, code);
                    fileName = Path.Combine(stockDataFileFolder, fileName);

                    files.Add(fileName);
                }

                ChinaStockDataProvider provider 
                    = new ChinaStockDataProvider(
                        _stockNameTable, 
                        files.ToArray(), 
                        startDateTimePicker.Value.Date, 
                        endDateTimePicker.Value.Date, 
                        int.Parse(warmupTextBox.Text));

                TradingStrategyEvaluator evaluator = new TradingStrategyEvaluator(0, null, "", provider, settings);


            }
            catch (Exception ex)
            {
                ShowError("Evaluation failed", ex);
            }

            evaluationProgressBar.Visible = false;
        }
    }
}
