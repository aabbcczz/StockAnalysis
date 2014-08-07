namespace EvaluatorClient
{
    partial class MetricForm
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint1 = new System.Windows.Forms.DataVisualization.Charting.DataPoint(5478D, "100,90,80,70");
            System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint2 = new System.Windows.Forms.DataVisualization.Charting.DataPoint(13148D, "110,120,100,90");
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.transactionDataGridView = new System.Windows.Forms.DataGridView();
            this.timeTransactionDataGridColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.codeTransactionDataGridColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.actionTransactionDataGridColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.priceTransactionDataGridColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.volumeTransactionDataGridColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.detailsTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.transactionDataGridView)).BeginInit();
            this.tabPage2.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.chart1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(848, 819);
            this.splitContainer1.SplitterDistance = 419;
            this.splitContainer1.TabIndex = 0;
            // 
            // chart1
            // 
            chartArea1.AxisX.IsStartedFromZero = false;
            chartArea1.AxisX.MajorGrid.Enabled = false;
            chartArea1.AxisX.ScaleBreakStyle.BreakLineStyle = System.Windows.Forms.DataVisualization.Charting.BreakLineStyle.Straight;
            chartArea1.AxisX.ScaleBreakStyle.Enabled = true;
            chartArea1.AxisX.ScaleBreakStyle.LineColor = System.Drawing.Color.Maroon;
            chartArea1.AxisX2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.False;
            chartArea1.AxisY.IsStartedFromZero = false;
            chartArea1.AxisY.MajorGrid.Enabled = false;
            chartArea1.AxisY2.IsStartedFromZero = false;
            chartArea1.AxisY2.MajorGrid.Enabled = false;
            chartArea1.BackColor = System.Drawing.Color.Black;
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart1.Location = new System.Drawing.Point(0, 0);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Name = "equitySeries";
            series1.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            series1.YValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Double;
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Candlestick;
            series2.CustomProperties = "PriceDownColor=Red, PriceUpColor=Lime";
            series2.Name = "priceSeries";
            series2.Points.Add(dataPoint1);
            series2.Points.Add(dataPoint2);
            series2.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            series2.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            series2.YValuesPerPoint = 4;
            this.chart1.Series.Add(series1);
            this.chart1.Series.Add(series2);
            this.chart1.Size = new System.Drawing.Size(848, 419);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(848, 396);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.transactionDataGridView);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(840, 370);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "交易事务";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // transactionDataGridView
            // 
            this.transactionDataGridView.AllowUserToAddRows = false;
            this.transactionDataGridView.AllowUserToDeleteRows = false;
            this.transactionDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.transactionDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.timeTransactionDataGridColumn,
            this.codeTransactionDataGridColumn,
            this.actionTransactionDataGridColumn,
            this.priceTransactionDataGridColumn,
            this.volumeTransactionDataGridColumn});
            this.transactionDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.transactionDataGridView.Location = new System.Drawing.Point(3, 3);
            this.transactionDataGridView.Name = "transactionDataGridView";
            this.transactionDataGridView.ReadOnly = true;
            this.transactionDataGridView.RowHeadersVisible = false;
            this.transactionDataGridView.RowHeadersWidth = 4;
            this.transactionDataGridView.Size = new System.Drawing.Size(834, 364);
            this.transactionDataGridView.TabIndex = 0;
            // 
            // timeTransactionDataGridColumn
            // 
            dataGridViewCellStyle1.Format = "d";
            dataGridViewCellStyle1.NullValue = null;
            this.timeTransactionDataGridColumn.DefaultCellStyle = dataGridViewCellStyle1;
            this.timeTransactionDataGridColumn.HeaderText = "时间";
            this.timeTransactionDataGridColumn.Name = "timeTransactionDataGridColumn";
            this.timeTransactionDataGridColumn.ReadOnly = true;
            // 
            // codeTransactionDataGridColumn
            // 
            this.codeTransactionDataGridColumn.HeaderText = "代码";
            this.codeTransactionDataGridColumn.Name = "codeTransactionDataGridColumn";
            this.codeTransactionDataGridColumn.ReadOnly = true;
            // 
            // actionTransactionDataGridColumn
            // 
            this.actionTransactionDataGridColumn.DataPropertyName = "Action";
            this.actionTransactionDataGridColumn.HeaderText = "操作";
            this.actionTransactionDataGridColumn.Name = "actionTransactionDataGridColumn";
            this.actionTransactionDataGridColumn.ReadOnly = true;
            // 
            // priceTransactionDataGridColumn
            // 
            this.priceTransactionDataGridColumn.DataPropertyName = "Price";
            dataGridViewCellStyle2.Format = "N2";
            dataGridViewCellStyle2.NullValue = null;
            this.priceTransactionDataGridColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this.priceTransactionDataGridColumn.HeaderText = "价格";
            this.priceTransactionDataGridColumn.Name = "priceTransactionDataGridColumn";
            this.priceTransactionDataGridColumn.ReadOnly = true;
            // 
            // volumeTransactionDataGridColumn
            // 
            this.volumeTransactionDataGridColumn.DataPropertyName = "Volume";
            dataGridViewCellStyle3.NullValue = null;
            this.volumeTransactionDataGridColumn.DefaultCellStyle = dataGridViewCellStyle3;
            this.volumeTransactionDataGridColumn.HeaderText = "数量";
            this.volumeTransactionDataGridColumn.Name = "volumeTransactionDataGridColumn";
            this.volumeTransactionDataGridColumn.ReadOnly = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.detailsTextBox);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(840, 370);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "详细";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // detailsTextBox
            // 
            this.detailsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailsTextBox.Font = new System.Drawing.Font("Arial Narrow", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.detailsTextBox.Location = new System.Drawing.Point(3, 3);
            this.detailsTextBox.Multiline = true;
            this.detailsTextBox.Name = "detailsTextBox";
            this.detailsTextBox.ReadOnly = true;
            this.detailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.detailsTextBox.Size = new System.Drawing.Size(834, 364);
            this.detailsTextBox.TabIndex = 0;
            this.detailsTextBox.Text = "收益率     ：\r\n年华收益率   ：";
            // 
            // MetricForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(848, 819);
            this.Controls.Add(this.splitContainer1);
            this.Name = "MetricForm";
            this.Text = "评测结果细节";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MetricForm_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.transactionDataGridView)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.DataGridView transactionDataGridView;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox detailsTextBox;
        private System.Windows.Forms.DataGridViewTextBoxColumn timeTransactionDataGridColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn codeTransactionDataGridColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn actionTransactionDataGridColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn priceTransactionDataGridColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn volumeTransactionDataGridColumn;

    }
}