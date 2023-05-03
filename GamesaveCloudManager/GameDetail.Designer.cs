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
            ((System.ComponentModel.ISupportInitialize)dataGridViewPaths).BeginInit();
            groupBoxPath.SuspendLayout();
            SuspendLayout();
            // 
            // buttonSave
            // 
            buttonSave.Location = new Point(1064, 476);
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
            checkBoxActive.Location = new Point(338, 14);
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
            dataGridViewPaths.Location = new Point(104, 75);
            dataGridViewPaths.Name = "dataGridViewPaths";
            dataGridViewPaths.ReadOnly = true;
            dataGridViewPaths.RowHeadersWidth = 51;
            dataGridViewPaths.RowTemplate.Height = 29;
            dataGridViewPaths.Size = new Size(1054, 126);
            dataGridViewPaths.TabIndex = 31;
            dataGridViewPaths.DataBindingComplete += dataGridViewPaths_DataBindingComplete;
            dataGridViewPaths.RowStateChanged += dataGridViewPaths_RowStateChanged;
            // 
            // textBoxTitle
            // 
            textBoxTitle.Location = new Point(104, 42);
            textBoxTitle.MaxLength = 256;
            textBoxTitle.Name = "textBoxTitle";
            textBoxTitle.Size = new Size(954, 27);
            textBoxTitle.TabIndex = 30;
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            labelTitle.Location = new Point(14, 45);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(47, 20);
            labelTitle.TabIndex = 29;
            labelTitle.Text = "Title*";
            // 
            // textBoxId
            // 
            textBoxId.Location = new Point(104, 12);
            textBoxId.MaxLength = 10;
            textBoxId.Name = "textBoxId";
            textBoxId.Size = new Size(107, 27);
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
            label1.Location = new Point(14, 75);
            label1.Name = "label1";
            label1.Size = new Size(48, 20);
            label1.TabIndex = 34;
            label1.Text = "Paths";
            // 
            // buttonDelete
            // 
            buttonDelete.Location = new Point(1064, 229);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new Size(94, 29);
            buttonDelete.TabIndex = 36;
            buttonDelete.Text = "Delete";
            buttonDelete.UseVisualStyleBackColor = true;
            buttonDelete.Click += buttonDelete_Click;
            // 
            // buttonAdd
            // 
            buttonAdd.Location = new Point(964, 229);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(94, 29);
            buttonAdd.TabIndex = 35;
            buttonAdd.Text = "Add";
            buttonAdd.UseVisualStyleBackColor = true;
            buttonAdd.Click += buttonAdd_Click;
            // 
            // groupBoxPath
            // 
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
            groupBoxPath.Location = new Point(14, 273);
            groupBoxPath.Name = "groupBoxPath";
            groupBoxPath.Size = new Size(1144, 151);
            groupBoxPath.TabIndex = 38;
            groupBoxPath.TabStop = false;
            groupBoxPath.Text = "Path Details";
            // 
            // buttonFolderOpen
            // 
            buttonFolderOpen.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            buttonFolderOpen.Location = new Point(1094, 67);
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
            buttonFolderBrowser.Location = new Point(1060, 67);
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
            textBoxFilter.Location = new Point(90, 100);
            textBoxFilter.MaxLength = 256;
            textBoxFilter.Name = "textBoxFilter";
            textBoxFilter.Size = new Size(1033, 27);
            textBoxFilter.TabIndex = 32;
            textBoxFilter.TextChanged += textBoxFilter_TextChanged;
            // 
            // labelFilter
            // 
            labelFilter.AutoSize = true;
            labelFilter.Location = new Point(17, 103);
            labelFilter.Name = "labelFilter";
            labelFilter.Size = new Size(45, 20);
            labelFilter.TabIndex = 31;
            labelFilter.Text = "Filter";
            // 
            // checkBoxRecursive
            // 
            checkBoxRecursive.AutoSize = true;
            checkBoxRecursive.Location = new Point(329, 40);
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
            checkBoxMachine.Location = new Point(222, 39);
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
            textBoxPath.Location = new Point(90, 67);
            textBoxPath.MaxLength = 1024;
            textBoxPath.Name = "textBoxPath";
            textBoxPath.Size = new Size(963, 27);
            textBoxPath.TabIndex = 28;
            textBoxPath.TextChanged += textBoxPath_TextChanged;
            // 
            // labelPath
            // 
            labelPath.AutoSize = true;
            labelPath.Location = new Point(17, 70);
            labelPath.Name = "labelPath";
            labelPath.Size = new Size(48, 20);
            labelPath.TabIndex = 27;
            labelPath.Text = "Path*";
            // 
            // textBoxPathId
            // 
            textBoxPathId.Enabled = false;
            textBoxPathId.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            textBoxPathId.Location = new Point(90, 37);
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
            buttonIGDBId.Location = new Point(1064, 40);
            buttonIGDBId.Name = "buttonIGDBId";
            buttonIGDBId.Size = new Size(94, 29);
            buttonIGDBId.TabIndex = 39;
            buttonIGDBId.Text = "Get Id";
            buttonIGDBId.UseVisualStyleBackColor = true;
            buttonIGDBId.Click += buttonIGDBId_Click;
            // 
            // buttonIGDB
            // 
            buttonIGDB.Location = new Point(217, 11);
            buttonIGDB.Name = "buttonIGDB";
            buttonIGDB.Size = new Size(94, 29);
            buttonIGDB.TabIndex = 40;
            buttonIGDB.Text = "IGDB";
            buttonIGDB.UseVisualStyleBackColor = true;
            buttonIGDB.Click += buttonIGDB_Click;
            // 
            // GameDetail
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1170, 517);
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
    }
}