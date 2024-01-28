namespace GamesaveCloudManager
{
    partial class GameDetail
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameDetail));
            buttonSave = new Button();
            checkBoxActive = new CheckBox();
            dataGridViewPaths = new DataGridView();
            textBoxTitle = new TextBox();
            labelTitle = new Label();
            textBoxId = new TextBox();
            labelId = new Label();
            label1 = new Label();
            buttonDelete = new Button();
            buttonAdd = new Button();
            groupBoxPath = new GroupBox();
            label2 = new Label();
            textBoxFilterOut = new TextBox();
            buttonFolderOpen = new Button();
            buttonFolderBrowser = new Button();
            textBoxFilter = new TextBox();
            labelFilter = new Label();
            checkBoxRecursive = new CheckBox();
            checkBoxMachine = new CheckBox();
            textBoxPath = new TextBox();
            labelPath = new Label();
            textBoxPathId = new TextBox();
            labelPathId = new Label();
            buttonIGDBId = new Button();
            buttonIGDB = new Button();
            buttonInstallFolderOpen = new Button();
            buttonExecFileBrowser = new Button();
            textBoxExecPath = new TextBox();
            label3 = new Label();
            textBoxInstallPath = new TextBox();
            label4 = new Label();
            checkBoxAdmin = new CheckBox();
            comboBoxPlatform = new ComboBox();
            label5 = new Label();
            buttonInstallFolderBrowser = new Button();
            checkBox3DVision = new CheckBox();
            textBoxArguments = new TextBox();
            label6 = new Label();
            checkBoxStopMonitor = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)dataGridViewPaths).BeginInit();
            groupBoxPath.SuspendLayout();
            SuspendLayout();
            // 
            // buttonSave
            // 
            buttonSave.Location = new Point(1064, 599);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(94, 29);
            buttonSave.TabIndex = 33;
            buttonSave.Text = "Save";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += buttonSave_Click;
            // 
            // checkBoxActive
            // 
            checkBoxActive.AutoSize = true;
            checkBoxActive.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            checkBoxActive.Location = new Point(457, 14);
            checkBoxActive.Name = "checkBoxActive";
            checkBoxActive.Size = new Size(75, 24);
            checkBoxActive.TabIndex = 32;
            checkBoxActive.Text = "Active";
            checkBoxActive.UseVisualStyleBackColor = true;
            // 
            // dataGridViewPaths
            // 
            dataGridViewPaths.AllowUserToAddRows = false;
            dataGridViewPaths.AllowUserToDeleteRows = false;
            dataGridViewPaths.AllowUserToResizeColumns = false;
            dataGridViewPaths.AllowUserToResizeRows = false;
            dataGridViewPaths.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewPaths.Location = new Point(14, 271);
            dataGridViewPaths.Name = "dataGridViewPaths";
            dataGridViewPaths.ReadOnly = true;
            dataGridViewPaths.RowHeadersWidth = 51;
            dataGridViewPaths.RowTemplate.Height = 29;
            dataGridViewPaths.Size = new Size(1144, 126);
            dataGridViewPaths.TabIndex = 31;
            dataGridViewPaths.DataBindingComplete += dataGridViewPaths_DataBindingComplete;
            dataGridViewPaths.RowStateChanged += dataGridViewPaths_RowStateChanged;
            // 
            // textBoxTitle
            // 
            textBoxTitle.Location = new Point(147, 44);
            textBoxTitle.MaxLength = 256;
            textBoxTitle.Name = "textBoxTitle";
            textBoxTitle.Size = new Size(911, 27);
            textBoxTitle.TabIndex = 30;
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            labelTitle.Location = new Point(14, 47);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(47, 20);
            labelTitle.TabIndex = 29;
            labelTitle.Text = "Title*";
            // 
            // textBoxId
            // 
            textBoxId.Location = new Point(147, 12);
            textBoxId.MaxLength = 10;
            textBoxId.Name = "textBoxId";
            textBoxId.Size = new Size(162, 27);
            textBoxId.TabIndex = 28;
            textBoxId.KeyPress += textBoxId_KeyPress;
            // 
            // labelId
            // 
            labelId.AutoSize = true;
            labelId.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            labelId.Location = new Point(14, 15);
            labelId.Name = "labelId";
            labelId.Size = new Size(30, 20);
            labelId.TabIndex = 27;
            labelId.Text = "Id*";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(14, 240);
            label1.Name = "label1";
            label1.Size = new Size(123, 20);
            label1.TabIndex = 34;
            label1.Text = "Savegame Paths";
            // 
            // buttonDelete
            // 
            buttonDelete.Location = new Point(260, 236);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new Size(94, 29);
            buttonDelete.TabIndex = 36;
            buttonDelete.Text = "Delete";
            buttonDelete.UseVisualStyleBackColor = true;
            buttonDelete.Click += buttonDelete_Click;
            // 
            // buttonAdd
            // 
            buttonAdd.Location = new Point(147, 236);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(94, 29);
            buttonAdd.TabIndex = 35;
            buttonAdd.Text = "Add";
            buttonAdd.UseVisualStyleBackColor = true;
            buttonAdd.Click += buttonAdd_Click;
            // 
            // groupBoxPath
            // 
            groupBoxPath.Controls.Add(label2);
            groupBoxPath.Controls.Add(textBoxFilterOut);
            groupBoxPath.Controls.Add(buttonFolderOpen);
            groupBoxPath.Controls.Add(buttonFolderBrowser);
            groupBoxPath.Controls.Add(textBoxFilter);
            groupBoxPath.Controls.Add(labelFilter);
            groupBoxPath.Controls.Add(checkBoxRecursive);
            groupBoxPath.Controls.Add(checkBoxMachine);
            groupBoxPath.Controls.Add(textBoxPath);
            groupBoxPath.Controls.Add(labelPath);
            groupBoxPath.Controls.Add(textBoxPathId);
            groupBoxPath.Controls.Add(labelPathId);
            groupBoxPath.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            groupBoxPath.Location = new Point(14, 409);
            groupBoxPath.Name = "groupBoxPath";
            groupBoxPath.Size = new Size(1144, 175);
            groupBoxPath.TabIndex = 38;
            groupBoxPath.TabStop = false;
            groupBoxPath.Text = "Savegame Path Details";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(17, 138);
            label2.Name = "label2";
            label2.Size = new Size(75, 20);
            label2.TabIndex = 36;
            label2.Text = "Filter Out";
            // 
            // textBoxFilterOut
            // 
            textBoxFilterOut.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            textBoxFilterOut.Location = new Point(133, 135);
            textBoxFilterOut.MaxLength = 256;
            textBoxFilterOut.Name = "textBoxFilterOut";
            textBoxFilterOut.Size = new Size(990, 27);
            textBoxFilterOut.TabIndex = 35;
            textBoxFilterOut.TextChanged += textBoxFilterOut_TextChanged;
            // 
            // buttonFolderOpen
            // 
            buttonFolderOpen.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            buttonFolderOpen.Location = new Point(1094, 69);
            buttonFolderOpen.Name = "buttonFolderOpen";
            buttonFolderOpen.Size = new Size(29, 29);
            buttonFolderOpen.TabIndex = 34;
            buttonFolderOpen.Text = "+";
            buttonFolderOpen.UseVisualStyleBackColor = true;
            buttonFolderOpen.Click += buttonFolderOpen_Click;
            // 
            // buttonFolderBrowser
            // 
            buttonFolderBrowser.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            buttonFolderBrowser.Location = new Point(1060, 69);
            buttonFolderBrowser.Name = "buttonFolderBrowser";
            buttonFolderBrowser.Size = new Size(29, 29);
            buttonFolderBrowser.TabIndex = 33;
            buttonFolderBrowser.Text = "...";
            buttonFolderBrowser.UseVisualStyleBackColor = true;
            buttonFolderBrowser.Click += buttonFolderBrowser_Click;
            // 
            // textBoxFilter
            // 
            textBoxFilter.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            textBoxFilter.Location = new Point(133, 102);
            textBoxFilter.MaxLength = 256;
            textBoxFilter.Name = "textBoxFilter";
            textBoxFilter.Size = new Size(990, 27);
            textBoxFilter.TabIndex = 32;
            textBoxFilter.TextChanged += textBoxFilter_TextChanged;
            // 
            // labelFilter
            // 
            labelFilter.AutoSize = true;
            labelFilter.Location = new Point(17, 105);
            labelFilter.Name = "labelFilter";
            labelFilter.Size = new Size(45, 20);
            labelFilter.TabIndex = 31;
            labelFilter.Text = "Filter";
            // 
            // checkBoxRecursive
            // 
            checkBoxRecursive.AutoSize = true;
            checkBoxRecursive.Location = new Point(372, 40);
            checkBoxRecursive.Name = "checkBoxRecursive";
            checkBoxRecursive.Size = new Size(98, 24);
            checkBoxRecursive.TabIndex = 30;
            checkBoxRecursive.Text = "Recursive";
            checkBoxRecursive.UseVisualStyleBackColor = true;
            checkBoxRecursive.CheckedChanged += checkBoxRecursive_CheckedChanged;
            // 
            // checkBoxMachine
            // 
            checkBoxMachine.AutoSize = true;
            checkBoxMachine.Location = new Point(265, 39);
            checkBoxMachine.Name = "checkBoxMachine";
            checkBoxMachine.Size = new Size(90, 24);
            checkBoxMachine.TabIndex = 29;
            checkBoxMachine.Text = "Machine";
            checkBoxMachine.UseVisualStyleBackColor = true;
            checkBoxMachine.CheckedChanged += checkBoxMachine_CheckedChanged;
            // 
            // textBoxPath
            // 
            textBoxPath.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            textBoxPath.Location = new Point(133, 69);
            textBoxPath.MaxLength = 1024;
            textBoxPath.Name = "textBoxPath";
            textBoxPath.Size = new Size(920, 27);
            textBoxPath.TabIndex = 28;
            textBoxPath.TextChanged += textBoxPath_TextChanged;
            // 
            // labelPath
            // 
            labelPath.AutoSize = true;
            labelPath.Location = new Point(17, 72);
            labelPath.Name = "labelPath";
            labelPath.Size = new Size(48, 20);
            labelPath.TabIndex = 27;
            labelPath.Text = "Path*";
            // 
            // textBoxPathId
            // 
            textBoxPathId.Enabled = false;
            textBoxPathId.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            textBoxPathId.Location = new Point(133, 37);
            textBoxPathId.Name = "textBoxPathId";
            textBoxPathId.Size = new Size(107, 27);
            textBoxPathId.TabIndex = 26;
            // 
            // labelPathId
            // 
            labelPathId.AutoSize = true;
            labelPathId.Location = new Point(17, 40);
            labelPathId.Name = "labelPathId";
            labelPathId.Size = new Size(66, 20);
            labelPathId.TabIndex = 25;
            labelPathId.Text = "Path Id*";
            // 
            // buttonIGDBId
            // 
            buttonIGDBId.Location = new Point(1064, 42);
            buttonIGDBId.Name = "buttonIGDBId";
            buttonIGDBId.Size = new Size(94, 29);
            buttonIGDBId.TabIndex = 39;
            buttonIGDBId.Text = "Get Id";
            buttonIGDBId.UseVisualStyleBackColor = true;
            buttonIGDBId.Click += buttonIGDBId_Click;
            // 
            // buttonIGDB
            // 
            buttonIGDB.Location = new Point(315, 11);
            buttonIGDB.Name = "buttonIGDB";
            buttonIGDB.Size = new Size(94, 29);
            buttonIGDB.TabIndex = 40;
            buttonIGDB.Text = "IGDB";
            buttonIGDB.UseVisualStyleBackColor = true;
            buttonIGDB.Click += buttonIGDB_Click;
            // 
            // buttonInstallFolderOpen
            // 
            buttonInstallFolderOpen.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            buttonInstallFolderOpen.Location = new Point(1099, 140);
            buttonInstallFolderOpen.Name = "buttonInstallFolderOpen";
            buttonInstallFolderOpen.Size = new Size(29, 29);
            buttonInstallFolderOpen.TabIndex = 44;
            buttonInstallFolderOpen.Text = "+";
            buttonInstallFolderOpen.UseVisualStyleBackColor = true;
            // 
            // buttonExecFileBrowser
            // 
            buttonExecFileBrowser.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            buttonExecFileBrowser.Location = new Point(1064, 109);
            buttonExecFileBrowser.Name = "buttonExecFileBrowser";
            buttonExecFileBrowser.Size = new Size(29, 29);
            buttonExecFileBrowser.TabIndex = 43;
            buttonExecFileBrowser.Text = "...";
            buttonExecFileBrowser.UseVisualStyleBackColor = true;
            buttonExecFileBrowser.Click += buttonExecFileBrowser_Click;
            // 
            // textBoxExecPath
            // 
            textBoxExecPath.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            textBoxExecPath.Location = new Point(147, 109);
            textBoxExecPath.MaxLength = 1024;
            textBoxExecPath.Name = "textBoxExecPath";
            textBoxExecPath.Size = new Size(911, 27);
            textBoxExecPath.TabIndex = 42;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label3.Location = new Point(14, 112);
            label3.Name = "label3";
            label3.Size = new Size(120, 20);
            label3.TabIndex = 41;
            label3.Text = "Executable Path";
            // 
            // textBoxInstallPath
            // 
            textBoxInstallPath.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            textBoxInstallPath.Location = new Point(147, 141);
            textBoxInstallPath.MaxLength = 1024;
            textBoxInstallPath.Name = "textBoxInstallPath";
            textBoxInstallPath.Size = new Size(911, 27);
            textBoxInstallPath.TabIndex = 46;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label4.Location = new Point(14, 144);
            label4.Name = "label4";
            label4.Size = new Size(124, 20);
            label4.TabIndex = 45;
            label4.Text = "Installation Path";
            // 
            // checkBoxAdmin
            // 
            checkBoxAdmin.AutoSize = true;
            checkBoxAdmin.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            checkBoxAdmin.Location = new Point(556, 14);
            checkBoxAdmin.Name = "checkBoxAdmin";
            checkBoxAdmin.Size = new Size(78, 24);
            checkBoxAdmin.TabIndex = 47;
            checkBoxAdmin.Text = "Admin";
            checkBoxAdmin.UseVisualStyleBackColor = true;
            // 
            // comboBoxPlatform
            // 
            comboBoxPlatform.FormattingEnabled = true;
            comboBoxPlatform.Items.AddRange(new object[] { "PC (Windows)", "Nintendo Switch", "Nintendo Wii U" });
            comboBoxPlatform.Location = new Point(147, 76);
            comboBoxPlatform.Name = "comboBoxPlatform";
            comboBoxPlatform.Size = new Size(262, 28);
            comboBoxPlatform.TabIndex = 49;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label5.Location = new Point(14, 79);
            label5.Name = "label5";
            label5.Size = new Size(78, 20);
            label5.TabIndex = 48;
            label5.Text = "Platform*";
            // 
            // buttonInstallFolderBrowser
            // 
            buttonInstallFolderBrowser.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            buttonInstallFolderBrowser.Location = new Point(1064, 140);
            buttonInstallFolderBrowser.Name = "buttonInstallFolderBrowser";
            buttonInstallFolderBrowser.Size = new Size(29, 29);
            buttonInstallFolderBrowser.TabIndex = 50;
            buttonInstallFolderBrowser.Text = "...";
            buttonInstallFolderBrowser.UseVisualStyleBackColor = true;
            buttonInstallFolderBrowser.Click += buttonInstallFolderBrowser_Click;
            // 
            // checkBox3DVision
            // 
            checkBox3DVision.AutoSize = true;
            checkBox3DVision.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            checkBox3DVision.Location = new Point(457, 80);
            checkBox3DVision.Name = "checkBox3DVision";
            checkBox3DVision.Size = new Size(94, 24);
            checkBox3DVision.TabIndex = 51;
            checkBox3DVision.Text = "3DVision";
            checkBox3DVision.UseVisualStyleBackColor = true;
            // 
            // textBoxArguments
            // 
            textBoxArguments.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            textBoxArguments.Location = new Point(147, 173);
            textBoxArguments.MaxLength = 1024;
            textBoxArguments.Name = "textBoxArguments";
            textBoxArguments.Size = new Size(911, 27);
            textBoxArguments.TabIndex = 53;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label6.Location = new Point(14, 176);
            label6.Name = "label6";
            label6.Size = new Size(88, 20);
            label6.TabIndex = 52;
            label6.Text = "Arguments";
            // 
            // checkBoxStopMonitor
            // 
            checkBoxStopMonitor.AutoSize = true;
            checkBoxStopMonitor.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            checkBoxStopMonitor.Location = new Point(655, 14);
            checkBoxStopMonitor.Name = "checkBoxStopMonitor";
            checkBoxStopMonitor.Size = new Size(124, 24);
            checkBoxStopMonitor.TabIndex = 54;
            checkBoxStopMonitor.Text = "Stop Monitor";
            checkBoxStopMonitor.UseVisualStyleBackColor = true;
            // 
            // GameDetail
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1170, 641);
            Controls.Add(checkBoxStopMonitor);
            Controls.Add(textBoxArguments);
            Controls.Add(label6);
            Controls.Add(checkBox3DVision);
            Controls.Add(buttonInstallFolderBrowser);
            Controls.Add(comboBoxPlatform);
            Controls.Add(label5);
            Controls.Add(checkBoxAdmin);
            Controls.Add(textBoxInstallPath);
            Controls.Add(label4);
            Controls.Add(buttonInstallFolderOpen);
            Controls.Add(buttonExecFileBrowser);
            Controls.Add(textBoxExecPath);
            Controls.Add(label3);
            Controls.Add(buttonIGDB);
            Controls.Add(buttonIGDBId);
            Controls.Add(groupBoxPath);
            Controls.Add(buttonDelete);
            Controls.Add(buttonAdd);
            Controls.Add(label1);
            Controls.Add(buttonSave);
            Controls.Add(checkBoxActive);
            Controls.Add(dataGridViewPaths);
            Controls.Add(textBoxTitle);
            Controls.Add(labelTitle);
            Controls.Add(textBoxId);
            Controls.Add(labelId);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "GameDetail";
            StartPosition = FormStartPosition.CenterParent;
            Text = "GameDetail";
            Load += GameDetail_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridViewPaths).EndInit();
            groupBoxPath.ResumeLayout(false);
            groupBoxPath.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button buttonSave;
        private CheckBox checkBoxActive;
        private DataGridView dataGridViewPaths;
        private TextBox textBoxTitle;
        private Label labelTitle;
        private TextBox textBoxId;
        private Label labelId;
        private Label label1;
        private Button buttonDelete;
        private Button buttonAdd;
        private GroupBox groupBoxPath;
        private Button buttonFolderBrowser;
        private TextBox textBoxFilter;
        private Label labelFilter;
        private CheckBox checkBoxRecursive;
        private CheckBox checkBoxMachine;
        private TextBox textBoxPath;
        private Label labelPath;
        private TextBox textBoxPathId;
        private Label labelPathId;
        private Button buttonFolderOpen;
        private Button buttonIGDBId;
        private Button buttonIGDB;
        private Label label2;
        private TextBox textBoxFilterOut;
        private Button buttonInstallFolderOpen;
        private Button buttonExecFileBrowser;
        private TextBox textBoxExecPath;
        private Label label3;
        private TextBox textBoxInstallPath;
        private Label label4;
        private CheckBox checkBoxAdmin;
        private ComboBox comboBoxPlatform;
        private Label label5;
        private Button buttonInstallFolderBrowser;
        private CheckBox checkBox3DVision;
        private TextBox textBoxArguments;
        private Label label6;
        private CheckBox checkBoxStopMonitor;
    }
}