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
            this.conditionsTabControl = new System.Windows.Forms.TabControl();
            this.settingsPage = new System.Windows.Forms.TabPage();
            this.selectionPage = new System.Windows.Forms.TabPage();
            this.resultPage = new System.Windows.Forms.TabPage();
            this.evaluateButton = new System.Windows.Forms.Button();
            this.conditionsTabControl.SuspendLayout();
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
            this.conditionsTabControl.Size = new System.Drawing.Size(1131, 564);
            this.conditionsTabControl.TabIndex = 0;
            // 
            // settingsPage
            // 
            this.settingsPage.Location = new System.Drawing.Point(4, 22);
            this.settingsPage.Name = "settingsPage";
            this.settingsPage.Padding = new System.Windows.Forms.Padding(3);
            this.settingsPage.Size = new System.Drawing.Size(1123, 538);
            this.settingsPage.TabIndex = 0;
            this.settingsPage.Text = "Settings";
            this.settingsPage.UseVisualStyleBackColor = true;
            // 
            // selectionPage
            // 
            this.selectionPage.Location = new System.Drawing.Point(4, 22);
            this.selectionPage.Name = "selectionPage";
            this.selectionPage.Padding = new System.Windows.Forms.Padding(3);
            this.selectionPage.Size = new System.Drawing.Size(1123, 538);
            this.selectionPage.TabIndex = 1;
            this.selectionPage.Text = "Selection";
            this.selectionPage.UseVisualStyleBackColor = true;
            // 
            // resultPage
            // 
            this.resultPage.Location = new System.Drawing.Point(4, 22);
            this.resultPage.Name = "resultPage";
            this.resultPage.Padding = new System.Windows.Forms.Padding(3);
            this.resultPage.Size = new System.Drawing.Size(1123, 538);
            this.resultPage.TabIndex = 2;
            this.resultPage.Text = "Result";
            this.resultPage.UseVisualStyleBackColor = true;
            // 
            // evaluateButton
            // 
            this.evaluateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.evaluateButton.Location = new System.Drawing.Point(1065, 581);
            this.evaluateButton.Name = "evaluateButton";
            this.evaluateButton.Size = new System.Drawing.Size(75, 32);
            this.evaluateButton.TabIndex = 1;
            this.evaluateButton.Text = "Evaluate";
            this.evaluateButton.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1156, 616);
            this.Controls.Add(this.evaluateButton);
            this.Controls.Add(this.conditionsTabControl);
            this.Name = "MainForm";
            this.Text = "Evaluator";
            this.conditionsTabControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl conditionsTabControl;
        private System.Windows.Forms.TabPage settingsPage;
        private System.Windows.Forms.TabPage selectionPage;
        private System.Windows.Forms.TabPage resultPage;
        private System.Windows.Forms.Button evaluateButton;
    }
}

