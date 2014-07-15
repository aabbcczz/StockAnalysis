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
        TradingSettings _tradingSettings;
        ChinaStockDataProvider _provider;

        public MainForm()
        {
            InitializeComponent();
        }

        private void conditionsTabControl_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage == resultPage)
            {
                exportButton.Visible = resultListView.Items.Count > 0;
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

            ApplyTradingSettings(LoadTradingSettings());
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
                    MessageBox.Show(
                        string.Format("Failed to load file {0}. Exception:\n{1}", file, ex),
                        "Load trading settings",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
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
                    MessageBox.Show(
                        string.Format("Exception:\n{0}", ex),
                        "Save trading settings",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
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
                MessageBox.Show(
                    string.Format("Failed to save file {0}. Exception:\n{1}", file, ex),
                    "Save trading settings",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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

        private void buyCommissionTextBox_Validated(object sender, EventArgs e)
        {
            this.errorProvider1.SetError(buyCommissionTextBox, "");
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

        private void sellCommissionTextBox_Validated(object sender, EventArgs e)
        {
            this.errorProvider1.SetError(sellCommissionTextBox, "");
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

        private void tariffTextBox_Validated(object sender, EventArgs e)
        {
            this.errorProvider1.SetError(tariffTextBox, "");
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

        private void spreadTextBox_Validated(object sender, EventArgs e)
        {
            this.errorProvider1.SetError(spreadTextBox, "");
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
    }
}
