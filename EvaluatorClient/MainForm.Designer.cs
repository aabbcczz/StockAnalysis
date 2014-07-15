namespace EvaluatorClient
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewItem listViewItem14 = new System.Windows.Forms.ListViewItem(new string[] {
            "000000",
            "000000",
            "1",
            "1",
            "100",
            "3",
            "100",
            "20",
            "10",
            "30",
            "30"}, -1);
            this.conditionsTabControl = new System.Windows.Forms.TabControl();
            this.settingsPage = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.closeLongOptionComboBox = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.spreadTextBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.openLongOptionComboBox = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tariffTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.sellCommissionTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.buyCommissionTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chargeByVolumeRadioButton = new System.Windows.Forms.RadioButton();
            this.chargeByAmountRadioButton = new System.Windows.Forms.RadioButton();
            this.selectionPage = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.clearObjectButton = new System.Windows.Forms.Button();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.availableObjectListView = new System.Windows.Forms.ListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.removeObjectButton = new System.Windows.Forms.Button();
            this.addObjectButton = new System.Windows.Forms.Button();
            this.selectedObjectListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label15 = new System.Windows.Forms.Label();
            this.warmupTextBox = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.endDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.label12 = new System.Windows.Forms.Label();
            this.startDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.label11 = new System.Windows.Forms.Label();
            this.resultPage = new System.Windows.Forms.TabPage();
            this.resultListView = new System.Windows.Forms.ListView();
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader14 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader15 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.evaluateButton = new System.Windows.Forms.Button();
            this.exportButton = new System.Windows.Forms.Button();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.conditionsTabControl.SuspendLayout();
            this.settingsPage.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.selectionPage.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.resultPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // conditionsTabControl
            // 
            this.conditionsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.conditionsTabControl.Controls.Add(this.settingsPage);
            this.conditionsTabControl.Controls.Add(this.selectionPage);
            this.conditionsTabControl.Controls.Add(this.resultPage);
            this.conditionsTabControl.Location = new System.Drawing.Point(13, 13);
            this.conditionsTabControl.Name = "conditionsTabControl";
            this.conditionsTabControl.SelectedIndex = 0;
            this.conditionsTabControl.Size = new System.Drawing.Size(934, 567);
            this.conditionsTabControl.TabIndex = 0;
            this.conditionsTabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.conditionsTabControl_Selected);
            // 
            // settingsPage
            // 
            this.settingsPage.Controls.Add(this.groupBox2);
            this.settingsPage.Controls.Add(this.groupBox1);
            this.settingsPage.Location = new System.Drawing.Point(4, 22);
            this.settingsPage.Name = "settingsPage";
            this.settingsPage.Padding = new System.Windows.Forms.Padding(3);
            this.settingsPage.Size = new System.Drawing.Size(926, 541);
            this.settingsPage.TabIndex = 0;
            this.settingsPage.Text = "交易设置";
            this.settingsPage.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.closeLongOptionComboBox);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.spreadTextBox);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.openLongOptionComboBox);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Location = new System.Drawing.Point(256, 16);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(206, 172);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "交易时机和价格";
            // 
            // closeLongOptionComboBox
            // 
            this.closeLongOptionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.closeLongOptionComboBox.FormattingEnabled = true;
            this.closeLongOptionComboBox.Location = new System.Drawing.Point(53, 53);
            this.closeLongOptionComboBox.MaxDropDownItems = 10;
            this.closeLongOptionComboBox.Name = "closeLongOptionComboBox";
            this.closeLongOptionComboBox.Size = new System.Drawing.Size(134, 21);
            this.closeLongOptionComboBox.TabIndex = 6;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(108, 113);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(79, 13);
            this.label10.TabIndex = 5;
            this.label10.Text = "最小变动单元";
            // 
            // spreadTextBox
            // 
            this.spreadTextBox.Location = new System.Drawing.Point(53, 110);
            this.spreadTextBox.Name = "spreadTextBox";
            this.spreadTextBox.Size = new System.Drawing.Size(49, 20);
            this.spreadTextBox.TabIndex = 5;
            this.spreadTextBox.Text = "0";
            this.spreadTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.spreadTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.spreadTextBox_Validating);
            this.spreadTextBox.Validated += new System.EventHandler(this.spreadTextBox_Validated);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(16, 113);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(31, 13);
            this.label9.TabIndex = 4;
            this.label9.Text = "滑点";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(16, 56);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(31, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "平仓";
            // 
            // openLongOptionComboBox
            // 
            this.openLongOptionComboBox.DisplayMember = "Option";
            this.openLongOptionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.openLongOptionComboBox.FormattingEnabled = true;
            this.openLongOptionComboBox.Location = new System.Drawing.Point(53, 21);
            this.openLongOptionComboBox.MaxDropDownItems = 10;
            this.openLongOptionComboBox.Name = "openLongOptionComboBox";
            this.openLongOptionComboBox.Size = new System.Drawing.Size(134, 21);
            this.openLongOptionComboBox.TabIndex = 1;
            this.openLongOptionComboBox.ValueMember = "Option";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 24);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(31, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "开仓";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.tariffTextBox);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.sellCommissionTextBox);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.buyCommissionTextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.chargeByVolumeRadioButton);
            this.groupBox1.Controls.Add(this.chargeByAmountRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(7, 16);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(227, 172);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "交易费率";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(152, 137);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "元 / 手";
            // 
            // tariffTextBox
            // 
            this.tariffTextBox.Enabled = false;
            this.tariffTextBox.Location = new System.Drawing.Point(91, 134);
            this.tariffTextBox.Name = "tariffTextBox";
            this.tariffTextBox.Size = new System.Drawing.Size(55, 20);
            this.tariffTextBox.TabIndex = 9;
            this.tariffTextBox.Text = "10";
            this.tariffTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.tariffTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.tariffTextBox_Validating);
            this.tariffTextBox.Validated += new System.EventHandler(this.tariffTextBox_Validated);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(30, 137);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "交易费率";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(152, 82);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "% 成交额";
            // 
            // sellCommissionTextBox
            // 
            this.sellCommissionTextBox.Location = new System.Drawing.Point(91, 79);
            this.sellCommissionTextBox.Name = "sellCommissionTextBox";
            this.sellCommissionTextBox.Size = new System.Drawing.Size(55, 20);
            this.sellCommissionTextBox.TabIndex = 6;
            this.sellCommissionTextBox.Text = "0.05";
            this.sellCommissionTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.sellCommissionTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.sellCommissionTextBox_Validating);
            this.sellCommissionTextBox.Validated += new System.EventHandler(this.sellCommissionTextBox_Validated);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(30, 79);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "卖出费率";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(152, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "% 成交额";
            // 
            // buyCommissionTextBox
            // 
            this.buyCommissionTextBox.Location = new System.Drawing.Point(91, 53);
            this.buyCommissionTextBox.Name = "buyCommissionTextBox";
            this.buyCommissionTextBox.Size = new System.Drawing.Size(55, 20);
            this.buyCommissionTextBox.TabIndex = 3;
            this.buyCommissionTextBox.Text = "0.05";
            this.buyCommissionTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.buyCommissionTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.buyCommissionTextBox_Validating);
            this.buyCommissionTextBox.Validated += new System.EventHandler(this.buyCommissionTextBox_Validated);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "买入费率";
            // 
            // chargeByVolumeRadioButton
            // 
            this.chargeByVolumeRadioButton.AutoSize = true;
            this.chargeByVolumeRadioButton.Location = new System.Drawing.Point(16, 111);
            this.chargeByVolumeRadioButton.Name = "chargeByVolumeRadioButton";
            this.chargeByVolumeRadioButton.Size = new System.Drawing.Size(121, 17);
            this.chargeByVolumeRadioButton.TabIndex = 1;
            this.chargeByVolumeRadioButton.Text = "按成交量收手续费";
            this.chargeByVolumeRadioButton.UseVisualStyleBackColor = true;
            this.chargeByVolumeRadioButton.CheckedChanged += new System.EventHandler(this.chargeByVolumeRadioButton_CheckedChanged);
            // 
            // chargeByAmountRadioButton
            // 
            this.chargeByAmountRadioButton.AutoSize = true;
            this.chargeByAmountRadioButton.Checked = true;
            this.chargeByAmountRadioButton.Location = new System.Drawing.Point(16, 20);
            this.chargeByAmountRadioButton.Name = "chargeByAmountRadioButton";
            this.chargeByAmountRadioButton.Size = new System.Drawing.Size(121, 17);
            this.chargeByAmountRadioButton.TabIndex = 0;
            this.chargeByAmountRadioButton.TabStop = true;
            this.chargeByAmountRadioButton.Text = "按成交额收手续费";
            this.chargeByAmountRadioButton.UseVisualStyleBackColor = true;
            this.chargeByAmountRadioButton.CheckedChanged += new System.EventHandler(this.chargeByAmountRadioButton_CheckedChanged);
            // 
            // selectionPage
            // 
            this.selectionPage.Controls.Add(this.groupBox4);
            this.selectionPage.Controls.Add(this.groupBox3);
            this.selectionPage.Location = new System.Drawing.Point(4, 22);
            this.selectionPage.Name = "selectionPage";
            this.selectionPage.Padding = new System.Windows.Forms.Padding(3);
            this.selectionPage.Size = new System.Drawing.Size(926, 541);
            this.selectionPage.TabIndex = 1;
            this.selectionPage.Text = "交易范围";
            this.selectionPage.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.clearObjectButton);
            this.groupBox4.Controls.Add(this.label17);
            this.groupBox4.Controls.Add(this.label16);
            this.groupBox4.Controls.Add(this.availableObjectListView);
            this.groupBox4.Controls.Add(this.removeObjectButton);
            this.groupBox4.Controls.Add(this.addObjectButton);
            this.groupBox4.Controls.Add(this.selectedObjectListView);
            this.groupBox4.Location = new System.Drawing.Point(7, 98);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(659, 437);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "交易对象";
            // 
            // clearObjectButton
            // 
            this.clearObjectButton.Location = new System.Drawing.Point(293, 215);
            this.clearObjectButton.Name = "clearObjectButton";
            this.clearObjectButton.Size = new System.Drawing.Size(75, 23);
            this.clearObjectButton.TabIndex = 7;
            this.clearObjectButton.Text = "清除所有";
            this.clearObjectButton.UseVisualStyleBackColor = true;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(376, 24);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(67, 13);
            this.label17.TabIndex = 6;
            this.label17.Text = "可选对象：";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(6, 24);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(67, 13);
            this.label16.TabIndex = 5;
            this.label16.Text = "已选对象：";
            // 
            // availableObjectListView
            // 
            this.availableObjectListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4});
            this.availableObjectListView.Location = new System.Drawing.Point(379, 49);
            this.availableObjectListView.Name = "availableObjectListView";
            this.availableObjectListView.Size = new System.Drawing.Size(273, 382);
            this.availableObjectListView.TabIndex = 4;
            this.availableObjectListView.UseCompatibleStateImageBehavior = false;
            this.availableObjectListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "代码";
            this.columnHeader3.Width = 116;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "名称";
            this.columnHeader4.Width = 132;
            // 
            // removeObjectButton
            // 
            this.removeObjectButton.Location = new System.Drawing.Point(293, 127);
            this.removeObjectButton.Name = "removeObjectButton";
            this.removeObjectButton.Size = new System.Drawing.Size(75, 23);
            this.removeObjectButton.TabIndex = 2;
            this.removeObjectButton.Text = "> >";
            this.removeObjectButton.UseVisualStyleBackColor = true;
            // 
            // addObjectButton
            // 
            this.addObjectButton.Location = new System.Drawing.Point(293, 77);
            this.addObjectButton.Name = "addObjectButton";
            this.addObjectButton.Size = new System.Drawing.Size(75, 23);
            this.addObjectButton.TabIndex = 1;
            this.addObjectButton.Text = "< <";
            this.addObjectButton.UseVisualStyleBackColor = true;
            // 
            // selectedObjectListView
            // 
            this.selectedObjectListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.selectedObjectListView.Location = new System.Drawing.Point(8, 49);
            this.selectedObjectListView.Name = "selectedObjectListView";
            this.selectedObjectListView.Size = new System.Drawing.Size(273, 382);
            this.selectedObjectListView.TabIndex = 0;
            this.selectedObjectListView.UseCompatibleStateImageBehavior = false;
            this.selectedObjectListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "代码";
            this.columnHeader1.Width = 116;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "名称";
            this.columnHeader2.Width = 132;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label15);
            this.groupBox3.Controls.Add(this.warmupTextBox);
            this.groupBox3.Controls.Add(this.label14);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Controls.Add(this.endDateTimePicker);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.startDateTimePicker);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Location = new System.Drawing.Point(7, 7);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(659, 85);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "评测时间段";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(503, 25);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(127, 13);
            this.label15.TabIndex = 3;
            this.label15.Text = "个周期数据用于初始化";
            // 
            // warmupTextBox
            // 
            this.warmupTextBox.Location = new System.Drawing.Point(444, 22);
            this.warmupTextBox.Name = "warmupTextBox";
            this.warmupTextBox.Size = new System.Drawing.Size(53, 20);
            this.warmupTextBox.TabIndex = 3;
            this.warmupTextBox.Text = "100";
            this.warmupTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(395, 25);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(43, 13);
            this.label14.TabIndex = 2;
            this.label14.Text = "向前取";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 54);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(175, 13);
            this.label13.TabIndex = 1;
            this.label13.Text = "评测结束后自动平仓并计入收益";
            // 
            // endDateTimePicker
            // 
            this.endDateTimePicker.Location = new System.Drawing.Point(242, 20);
            this.endDateTimePicker.Name = "endDateTimePicker";
            this.endDateTimePicker.Size = new System.Drawing.Size(124, 20);
            this.endDateTimePicker.TabIndex = 2;
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(209, 25);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(27, 13);
            this.label12.TabIndex = 2;
            this.label12.Text = "-";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // startDateTimePicker
            // 
            this.startDateTimePicker.Location = new System.Drawing.Point(79, 20);
            this.startDateTimePicker.Name = "startDateTimePicker";
            this.startDateTimePicker.Size = new System.Drawing.Size(124, 20);
            this.startDateTimePicker.TabIndex = 1;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 25);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(67, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "评测时段：";
            // 
            // resultPage
            // 
            this.resultPage.Controls.Add(this.resultListView);
            this.resultPage.Location = new System.Drawing.Point(4, 22);
            this.resultPage.Name = "resultPage";
            this.resultPage.Padding = new System.Windows.Forms.Padding(3);
            this.resultPage.Size = new System.Drawing.Size(926, 541);
            this.resultPage.TabIndex = 2;
            this.resultPage.Text = "评测结果";
            this.resultPage.UseVisualStyleBackColor = true;
            // 
            // resultListView
            // 
            this.resultListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader7,
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader12,
            this.columnHeader13,
            this.columnHeader14,
            this.columnHeader15});
            this.resultListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem14});
            this.resultListView.Location = new System.Drawing.Point(6, 6);
            this.resultListView.MultiSelect = false;
            this.resultListView.Name = "resultListView";
            this.resultListView.Size = new System.Drawing.Size(914, 529);
            this.resultListView.TabIndex = 0;
            this.resultListView.UseCompatibleStateImageBehavior = false;
            this.resultListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "代码";
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "名称";
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "盈利次数";
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "总次数";
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "胜率 %";
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "手续费（元）";
            this.columnHeader10.Width = 86;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "净利润（元）";
            this.columnHeader11.Width = 85;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "收益率 %";
            // 
            // columnHeader13
            // 
            this.columnHeader13.Text = "年化收益率 %";
            this.columnHeader13.Width = 90;
            // 
            // columnHeader14
            // 
            this.columnHeader14.Text = "最大回撤值";
            this.columnHeader14.Width = 79;
            // 
            // columnHeader15
            // 
            this.columnHeader15.Text = "最大回测比 %";
            this.columnHeader15.Width = 96;
            // 
            // evaluateButton
            // 
            this.evaluateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.evaluateButton.Location = new System.Drawing.Point(872, 586);
            this.evaluateButton.Name = "evaluateButton";
            this.evaluateButton.Size = new System.Drawing.Size(75, 32);
            this.evaluateButton.TabIndex = 1;
            this.evaluateButton.Text = "开始评测";
            this.evaluateButton.UseVisualStyleBackColor = true;
            // 
            // exportButton
            // 
            this.exportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exportButton.Location = new System.Drawing.Point(791, 586);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(75, 32);
            this.exportButton.TabIndex = 2;
            this.exportButton.Text = "导出结果";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Visible = false;
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(959, 629);
            this.Controls.Add(this.exportButton);
            this.Controls.Add(this.evaluateButton);
            this.Controls.Add(this.conditionsTabControl);
            this.Name = "MainForm";
            this.Text = "Evaluator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.conditionsTabControl.ResumeLayout(false);
            this.settingsPage.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.selectionPage.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.resultPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl conditionsTabControl;
        private System.Windows.Forms.TabPage settingsPage;
        private System.Windows.Forms.TabPage selectionPage;
        private System.Windows.Forms.TabPage resultPage;
        private System.Windows.Forms.Button evaluateButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tariffTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox sellCommissionTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox buyCommissionTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton chargeByVolumeRadioButton;
        private System.Windows.Forms.RadioButton chargeByAmountRadioButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox closeLongOptionComboBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox spreadTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox openLongOptionComboBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.DateTimePicker startDateTimePicker;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox warmupTextBox;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.DateTimePicker endDateTimePicker;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ListView selectedObjectListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Button removeObjectButton;
        private System.Windows.Forms.Button addObjectButton;
        private System.Windows.Forms.ListView availableObjectListView;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ListView resultListView;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.ColumnHeader columnHeader14;
        private System.Windows.Forms.ColumnHeader columnHeader15;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.Button clearObjectButton;
        private System.Windows.Forms.ErrorProvider errorProvider1;
    }
}

