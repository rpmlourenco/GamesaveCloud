namespace GamesaveCloudManager
{
    partial class SyncForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SyncForm));
            label1 = new Label();
            label2 = new Label();
            comboBoxProvider = new ComboBox();
            comboBoxDirection = new ComboBox();
            textBox1 = new TextBox();
            buttonStart = new Button();
            checkBoxAsync = new CheckBox();
            buttonDeleteLocal = new Button();
            buttonDeleteCloud = new Button();
            buttonClearLog = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(12, 49);
            label1.Name = "label1";
            label1.Size = new Size(73, 20);
            label1.TabIndex = 0;
            label1.Text = "Direction";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(12, 15);
            label2.Name = "label2";
            label2.Size = new Size(68, 20);
            label2.TabIndex = 1;
            label2.Text = "Provider";
            // 
            // comboBoxProvider
            // 
            comboBoxProvider.FormattingEnabled = true;
            comboBoxProvider.Items.AddRange(new object[] { "OneDrive", "GoogleDrive" });
            comboBoxProvider.Location = new Point(91, 12);
            comboBoxProvider.Name = "comboBoxProvider";
            comboBoxProvider.Size = new Size(151, 28);
            comboBoxProvider.TabIndex = 2;
            // 
            // comboBoxDirection
            // 
            comboBoxDirection.FormattingEnabled = true;
            comboBoxDirection.Items.AddRange(new object[] { "Auto", "FromCloud", "ToCloud" });
            comboBoxDirection.Location = new Point(91, 46);
            comboBoxDirection.Name = "comboBoxDirection";
            comboBoxDirection.Size = new Size(151, 28);
            comboBoxDirection.TabIndex = 3;
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.White;
            textBox1.Location = new Point(91, 115);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.ScrollBars = ScrollBars.Both;
            textBox1.Size = new Size(697, 397);
            textBox1.TabIndex = 4;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // buttonStart
            // 
            buttonStart.Location = new Point(91, 80);
            buttonStart.Name = "buttonStart";
            buttonStart.Size = new Size(151, 29);
            buttonStart.TabIndex = 19;
            buttonStart.Text = "Start";
            buttonStart.UseVisualStyleBackColor = true;
            buttonStart.Click += buttonStart_Click;
            // 
            // checkBoxAsync
            // 
            checkBoxAsync.AutoSize = true;
            checkBoxAsync.Checked = true;
            checkBoxAsync.CheckState = CheckState.Checked;
            checkBoxAsync.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            checkBoxAsync.Location = new Point(261, 83);
            checkBoxAsync.Name = "checkBoxAsync";
            checkBoxAsync.Size = new Size(73, 24);
            checkBoxAsync.TabIndex = 20;
            checkBoxAsync.Text = "Async";
            checkBoxAsync.UseVisualStyleBackColor = true;
            // 
            // buttonDeleteLocal
            // 
            buttonDeleteLocal.Location = new Point(446, 80);
            buttonDeleteLocal.Name = "buttonDeleteLocal";
            buttonDeleteLocal.Size = new Size(110, 29);
            buttonDeleteLocal.TabIndex = 21;
            buttonDeleteLocal.Text = "Delete Local";
            buttonDeleteLocal.UseVisualStyleBackColor = true;
            buttonDeleteLocal.Click += buttonDeleteLocal_Click;
            // 
            // buttonDeleteCloud
            // 
            buttonDeleteCloud.Location = new Point(562, 80);
            buttonDeleteCloud.Name = "buttonDeleteCloud";
            buttonDeleteCloud.Size = new Size(110, 29);
            buttonDeleteCloud.TabIndex = 21;
            buttonDeleteCloud.Text = "Delete Cloud";
            buttonDeleteCloud.UseVisualStyleBackColor = true;
            buttonDeleteCloud.Click += buttonDeleteCloud_Click;
            // 
            // buttonClearLog
            // 
            buttonClearLog.Location = new Point(678, 80);
            buttonClearLog.Name = "buttonClearLog";
            buttonClearLog.Size = new Size(110, 29);
            buttonClearLog.TabIndex = 22;
            buttonClearLog.Text = "Clear Log";
            buttonClearLog.UseVisualStyleBackColor = true;
            buttonClearLog.Click += buttonClearLog_Click;
            // 
            // SyncForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 524);
            Controls.Add(buttonClearLog);
            Controls.Add(buttonDeleteCloud);
            Controls.Add(buttonDeleteLocal);
            Controls.Add(checkBoxAsync);
            Controls.Add(buttonStart);
            Controls.Add(textBox1);
            Controls.Add(comboBoxDirection);
            Controls.Add(comboBoxProvider);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "SyncForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "SyncForm";
            FormClosing += SyncForm_FormClosing;
            Load += SyncForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private ComboBox comboBoxProvider;
        private ComboBox comboBoxDirection;
        private TextBox textBox1;
        private Button buttonStart;
        private CheckBox checkBoxAsync;
        private Button buttonDeleteLocal;
        private Button buttonDeleteCloud;
        private Button buttonClearLog;
    }
}