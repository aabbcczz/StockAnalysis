namespace DTViewer
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.CalloutAnnotation calloutAnnotation1 = new System.Windows.Forms.DataVisualization.Charting.CalloutAnnotation();
            System.Windows.Forms.DataVisualization.Charting.CalloutAnnotation calloutAnnotation2 = new System.Windows.Forms.DataVisualization.Charting.CalloutAnnotation();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.labelCode = new System.Windows.Forms.Label();
            this.buttonDecreaseShownBars = new System.Windows.Forms.Button();
            this.buttonIncreaseShowedBars = new System.Windows.Forms.Button();
            this.chartData = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadDataSettingsFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadClosedPositionFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.dataGridViewCodes = new System.Windows.Forms.DataGridView();
            this.ColumnCodesCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnCodesName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewClosedPosition = new System.Windows.Forms.DataGridView();
            this.ColumnPositionCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnPositionName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnPositionBuyTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnPositionBuyPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnPositionSellTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnPositionSellPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnPositionVolume = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnPositionGain = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnPositionR = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.textBoxDetails = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartData)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCodes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewClosedPosition)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.labelCode);
            this.splitContainer1.Panel1.Controls.Add(this.buttonDecreaseShownBars);
            this.splitContainer1.Panel1.Controls.Add(this.buttonIncreaseShowedBars);
            this.splitContainer1.Panel1.Controls.Add(this.chartData);
            this.splitContainer1.Panel1.Controls.Add(this.menuStrip1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1155, 665);
            this.splitContainer1.SplitterDistance = 465;
            this.splitContainer1.TabIndex = 0;
            // 
            // labelCode
            // 
            this.labelCode.AutoSize = true;
            this.labelCode.Location = new System.Drawing.Point(24, 161);
            this.labelCode.Name = "labelCode";
            this.labelCode.Size = new System.Drawing.Size(0, 13);
            this.labelCode.TabIndex = 6;
            // 
            // buttonDecreaseShownBars
            // 
            this.buttonDecreaseShownBars.Location = new System.Drawing.Point(24, 107);
            this.buttonDecreaseShownBars.Name = "buttonDecreaseShownBars";
            this.buttonDecreaseShownBars.Size = new System.Drawing.Size(17, 23);
            this.buttonDecreaseShownBars.TabIndex = 5;
            this.buttonDecreaseShownBars.Text = "-";
            this.buttonDecreaseShownBars.UseVisualStyleBackColor = true;
            this.buttonDecreaseShownBars.Click += new System.EventHandler(this.buttonDecreaseShownBars_Click);
            // 
            // buttonIncreaseShowedBars
            // 
            this.buttonIncreaseShowedBars.Location = new System.Drawing.Point(24, 78);
            this.buttonIncreaseShowedBars.Name = "buttonIncreaseShowedBars";
            this.buttonIncreaseShowedBars.Size = new System.Drawing.Size(17, 23);
            this.buttonIncreaseShowedBars.TabIndex = 4;
            this.buttonIncreaseShowedBars.Text = "+";
            this.buttonIncreaseShowedBars.UseVisualStyleBackColor = true;
            this.buttonIncreaseShowedBars.Click += new System.EventHandler(this.buttonIncreaseShowedBars_Click);
            // 
            // chartData
            // 
            calloutAnnotation1.AnchorAlignment = System.Drawing.ContentAlignment.TopCenter;
            calloutAnnotation1.AnchorOffsetX = 0D;
            calloutAnnotation1.AnchorOffsetY = 5D;
            calloutAnnotation1.AxisXName = "ChartArea1\\rX";
            calloutAnnotation1.ClipToChartArea = "ChartArea1";
            calloutAnnotation1.ForeColor = System.Drawing.Color.DarkRed;
            calloutAnnotation1.IsSizeAlwaysRelative = false;
            calloutAnnotation1.LineColor = System.Drawing.Color.Maroon;
            calloutAnnotation1.Name = "CalloutAnnotation1";
            calloutAnnotation1.Text = "Buy";
            calloutAnnotation1.YAxisName = "ChartArea1\\rY";
            calloutAnnotation2.AnchorAlignment = System.Drawing.ContentAlignment.BottomCenter;
            calloutAnnotation2.AnchorOffsetX = 0D;
            calloutAnnotation2.AnchorOffsetY = 5D;
            calloutAnnotation2.AxisXName = "ChartArea1\\rX";
            calloutAnnotation2.ClipToChartArea = "ChartArea1";
            calloutAnnotation2.ForeColor = System.Drawing.Color.Maroon;
            calloutAnnotation2.IsSizeAlwaysRelative = false;
            calloutAnnotation2.LineColor = System.Drawing.Color.DarkRed;
            calloutAnnotation2.Name = "CalloutAnnotation2";
            calloutAnnotation2.Text = "Sell";
            calloutAnnotation2.YAxisName = "ChartArea1\\rY";
            this.chartData.Annotations.Add(calloutAnnotation1);
            this.chartData.Annotations.Add(calloutAnnotation2);
            chartArea1.AxisX.MinorGrid.Enabled = true;
            chartArea1.AxisX.MinorTickMark.Enabled = true;
            chartArea1.AxisX.ScrollBar.BackColor = System.Drawing.Color.White;
            chartArea1.AxisX.ScrollBar.ButtonColor = System.Drawing.Color.Silver;
            chartArea1.AxisX.ScrollBar.ButtonStyle = System.Windows.Forms.DataVisualization.Charting.ScrollBarButtonStyles.SmallScroll;
            chartArea1.AxisY.MinorGrid.Enabled = true;
            chartArea1.AxisY.MinorTickMark.Enabled = true;
            chartArea1.AxisY2.MinorGrid.Enabled = true;
            chartArea1.AxisY2.MinorTickMark.Enabled = true;
            chartArea1.BackColor = System.Drawing.Color.Black;
            chartArea1.Name = "ChartArea1";
            this.chartData.ChartAreas.Add(chartArea1);
            this.chartData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartData.Location = new System.Drawing.Point(0, 24);
            this.chartData.Name = "chartData";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Candlestick;
            series1.CustomProperties = "PriceDownColor=Lime, PriceUpColor=Red, MaxPixelPointWidth=4";
            series1.IsXValueIndexed = true;
            series1.Name = "stockSeries";
            series1.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            series1.YValuesPerPoint = 4;
            series2.ChartArea = "ChartArea1";
            series2.IsXValueIndexed = true;
            series2.Name = "volumeSeries";
            series2.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            this.chartData.Series.Add(series1);
            this.chartData.Series.Add(series2);
            this.chartData.Size = new System.Drawing.Size(1155, 441);
            this.chartData.TabIndex = 2;
            this.chartData.Text = "chart1";
            this.chartData.GetToolTipText += new System.EventHandler<System.Windows.Forms.DataVisualization.Charting.ToolTipEventArgs>(this.chartData_GetToolTipText);
            this.chartData.AxisViewChanged += new System.EventHandler<System.Windows.Forms.DataVisualization.Charting.ViewEventArgs>(this.chartData_AxisViewChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1155, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadDataSettingsFileToolStripMenuItem,
            this.loadClosedPositionFileToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // loadDataSettingsFileToolStripMenuItem
            // 
            this.loadDataSettingsFileToolStripMenuItem.Name = "loadDataSettingsFileToolStripMenuItem";
            this.loadDataSettingsFileToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.loadDataSettingsFileToolStripMenuItem.Text = "Load &Data settings file";
            this.loadDataSettingsFileToolStripMenuItem.Click += new System.EventHandler(this.loadDataSettingsFileToolStripMenuItem_Click);
            // 
            // loadClosedPositionFileToolStripMenuItem
            // 
            this.loadClosedPositionFileToolStripMenuItem.Name = "loadClosedPositionFileToolStripMenuItem";
            this.loadClosedPositionFileToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.loadClosedPositionFileToolStripMenuItem.Text = "Load &Closed position file";
            this.loadClosedPositionFileToolStripMenuItem.Click += new System.EventHandler(this.loadClosedPositionFileToolStripMenuItem_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.textBoxDetails);
            this.splitContainer2.Size = new System.Drawing.Size(1155, 196);
            this.splitContainer2.SplitterDistance = 830;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.dataGridViewCodes);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.dataGridViewClosedPosition);
            this.splitContainer3.Size = new System.Drawing.Size(830, 196);
            this.splitContainer3.SplitterDistance = 257;
            this.splitContainer3.TabIndex = 1;
            // 
            // dataGridViewCodes
            // 
            this.dataGridViewCodes.AllowUserToResizeRows = false;
            this.dataGridViewCodes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewCodes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnCodesCode,
            this.ColumnCodesName});
            this.dataGridViewCodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewCodes.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewCodes.MultiSelect = false;
            this.dataGridViewCodes.Name = "dataGridViewCodes";
            this.dataGridViewCodes.ReadOnly = true;
            this.dataGridViewCodes.RowHeadersVisible = false;
            this.dataGridViewCodes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewCodes.Size = new System.Drawing.Size(257, 196);
            this.dataGridViewCodes.TabIndex = 0;
            this.dataGridViewCodes.SelectionChanged += new System.EventHandler(this.dataGridViewCodes_SelectionChanged);
            // 
            // ColumnCodesCode
            // 
            this.ColumnCodesCode.HeaderText = "Code";
            this.ColumnCodesCode.Name = "ColumnCodesCode";
            this.ColumnCodesCode.ReadOnly = true;
            this.ColumnCodesCode.Width = 60;
            // 
            // ColumnCodesName
            // 
            this.ColumnCodesName.HeaderText = "Name";
            this.ColumnCodesName.Name = "ColumnCodesName";
            this.ColumnCodesName.ReadOnly = true;
            this.ColumnCodesName.Width = 150;
            // 
            // dataGridViewClosedPosition
            // 
            this.dataGridViewClosedPosition.AllowUserToResizeRows = false;
            this.dataGridViewClosedPosition.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewClosedPosition.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnPositionCode,
            this.ColumnPositionName,
            this.ColumnPositionBuyTime,
            this.ColumnPositionBuyPrice,
            this.ColumnPositionSellTime,
            this.ColumnPositionSellPrice,
            this.ColumnPositionVolume,
            this.ColumnPositionGain,
            this.ColumnPositionR});
            this.dataGridViewClosedPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewClosedPosition.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewClosedPosition.MultiSelect = false;
            this.dataGridViewClosedPosition.Name = "dataGridViewClosedPosition";
            this.dataGridViewClosedPosition.ReadOnly = true;
            this.dataGridViewClosedPosition.RowHeadersVisible = false;
            this.dataGridViewClosedPosition.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewClosedPosition.Size = new System.Drawing.Size(569, 196);
            this.dataGridViewClosedPosition.TabIndex = 1;
            this.dataGridViewClosedPosition.SelectionChanged += new System.EventHandler(this.dataGridViewClosedPosition_SelectionChanged);
            // 
            // ColumnPositionCode
            // 
            this.ColumnPositionCode.HeaderText = "Code";
            this.ColumnPositionCode.Name = "ColumnPositionCode";
            this.ColumnPositionCode.ReadOnly = true;
            this.ColumnPositionCode.Width = 60;
            // 
            // ColumnPositionName
            // 
            this.ColumnPositionName.HeaderText = "Name";
            this.ColumnPositionName.Name = "ColumnPositionName";
            this.ColumnPositionName.ReadOnly = true;
            // 
            // ColumnPositionBuyTime
            // 
            dataGridViewCellStyle1.Format = "d";
            dataGridViewCellStyle1.NullValue = null;
            this.ColumnPositionBuyTime.DefaultCellStyle = dataGridViewCellStyle1;
            this.ColumnPositionBuyTime.HeaderText = "Buy Time";
            this.ColumnPositionBuyTime.Name = "ColumnPositionBuyTime";
            this.ColumnPositionBuyTime.ReadOnly = true;
            // 
            // ColumnPositionBuyPrice
            // 
            dataGridViewCellStyle2.Format = "N3";
            dataGridViewCellStyle2.NullValue = null;
            this.ColumnPositionBuyPrice.DefaultCellStyle = dataGridViewCellStyle2;
            this.ColumnPositionBuyPrice.HeaderText = "Buy Price";
            this.ColumnPositionBuyPrice.Name = "ColumnPositionBuyPrice";
            this.ColumnPositionBuyPrice.ReadOnly = true;
            // 
            // ColumnPositionSellTime
            // 
            dataGridViewCellStyle3.Format = "d";
            dataGridViewCellStyle3.NullValue = null;
            this.ColumnPositionSellTime.DefaultCellStyle = dataGridViewCellStyle3;
            this.ColumnPositionSellTime.HeaderText = "Sell Time";
            this.ColumnPositionSellTime.Name = "ColumnPositionSellTime";
            this.ColumnPositionSellTime.ReadOnly = true;
            // 
            // ColumnPositionSellPrice
            // 
            dataGridViewCellStyle4.Format = "N3";
            dataGridViewCellStyle4.NullValue = null;
            this.ColumnPositionSellPrice.DefaultCellStyle = dataGridViewCellStyle4;
            this.ColumnPositionSellPrice.HeaderText = "Sell Price";
            this.ColumnPositionSellPrice.Name = "ColumnPositionSellPrice";
            this.ColumnPositionSellPrice.ReadOnly = true;
            // 
            // ColumnPositionVolume
            // 
            dataGridViewCellStyle5.Format = "N0";
            dataGridViewCellStyle5.NullValue = null;
            this.ColumnPositionVolume.DefaultCellStyle = dataGridViewCellStyle5;
            this.ColumnPositionVolume.HeaderText = "Volume";
            this.ColumnPositionVolume.Name = "ColumnPositionVolume";
            this.ColumnPositionVolume.ReadOnly = true;
            // 
            // ColumnPositionGain
            // 
            dataGridViewCellStyle6.Format = "N3";
            dataGridViewCellStyle6.NullValue = null;
            this.ColumnPositionGain.DefaultCellStyle = dataGridViewCellStyle6;
            this.ColumnPositionGain.HeaderText = "Gain";
            this.ColumnPositionGain.Name = "ColumnPositionGain";
            this.ColumnPositionGain.ReadOnly = true;
            // 
            // ColumnPositionR
            // 
            dataGridViewCellStyle7.Format = "N3";
            dataGridViewCellStyle7.NullValue = null;
            this.ColumnPositionR.DefaultCellStyle = dataGridViewCellStyle7;
            this.ColumnPositionR.HeaderText = "R";
            this.ColumnPositionR.Name = "ColumnPositionR";
            this.ColumnPositionR.ReadOnly = true;
            // 
            // textBoxDetails
            // 
            this.textBoxDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxDetails.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxDetails.Location = new System.Drawing.Point(0, 0);
            this.textBoxDetails.Multiline = true;
            this.textBoxDetails.Name = "textBoxDetails";
            this.textBoxDetails.ReadOnly = true;
            this.textBoxDetails.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxDetails.Size = new System.Drawing.Size(321, 196);
            this.textBoxDetails.TabIndex = 0;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Data settings file|*.xml|Closed position file|*.csv|All files|*.*";
            this.openFileDialog1.RestoreDirectory = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1155, 665);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Data and Transaction Viewer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartData)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCodes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewClosedPosition)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartData;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadDataSettingsFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadClosedPositionFileToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TextBox textBoxDetails;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.DataGridView dataGridViewCodes;
        private System.Windows.Forms.DataGridView dataGridViewClosedPosition;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnCodesCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnCodesName;
        private System.Windows.Forms.Button buttonDecreaseShownBars;
        private System.Windows.Forms.Button buttonIncreaseShowedBars;
        private System.Windows.Forms.Label labelCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPositionCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPositionName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPositionBuyTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPositionBuyPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPositionSellTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPositionSellPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPositionVolume;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPositionGain;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPositionR;

    }
}

