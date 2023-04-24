namespace GamesaveCloudManager
{
    partial class Game
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dataGridGame = new DataGridView();
            textBoxFilter = new TextBox();
            label1 = new Label();
            buttonAdd = new Button();
            buttonDelete = new Button();
            buttonEdit = new Button();
            buttonClear = new Button();
            label2 = new Label();
            buttonSyncGames = new Button();
            button2 = new Button();
            richTextBox1 = new RichTextBox();
            ((System.ComponentModel.ISupportInitialize)dataGridGame).BeginInit();
            SuspendLayout();
            // 
            // dataGridGame
            // 
            dataGridGame.AllowUserToAddRows = false;
            dataGridGame.AllowUserToDeleteRows = false;
            dataGridGame.AllowUserToResizeColumns = false;
            dataGridGame.AllowUserToResizeRows = false;
            dataGridGame.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridGame.Location = new Point(12, 45);
            dataGridGame.Name = "dataGridGame";
            dataGridGame.ReadOnly = true;
            dataGridGame.RowHeadersWidth = 51;
            dataGridGame.RowTemplate.Height = 29;
            dataGridGame.Size = new Size(1401, 366);
            dataGridGame.TabIndex = 0;
            dataGridGame.DataBindingComplete += dataGridView1_DataBindingComplete;
            dataGridGame.RowStateChanged += dataGridGame_RowStateChanged;
            // 
            // textBoxFilter
            // 
            textBoxFilter.Location = new Point(60, 12);
            textBoxFilter.Name = "textBoxFilter";
            textBoxFilter.Size = new Size(523, 27);
            textBoxFilter.TabIndex = 1;
            textBoxFilter.TextChanged += textBoxFilter_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(42, 20);
            label1.TabIndex = 2;
            label1.Text = "Filter";
            // 
            // buttonAdd
            // 
            buttonAdd.Location = new Point(1119, 420);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(94, 29);
            buttonAdd.TabIndex = 18;
            buttonAdd.Text = "Add";
            buttonAdd.UseVisualStyleBackColor = true;
            buttonAdd.Click += buttonAdd_Click;
            // 
            // buttonDelete
            // 
            buttonDelete.Location = new Point(1319, 420);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new Size(94, 29);
            buttonDelete.TabIndex = 19;
            buttonDelete.Text = "Delete";
            buttonDelete.UseVisualStyleBackColor = true;
            buttonDelete.Click += buttonDelete_Click;
            // 
            // buttonEdit
            // 
            buttonEdit.Location = new Point(1219, 420);
            buttonEdit.Name = "buttonEdit";
            buttonEdit.Size = new Size(94, 29);
            buttonEdit.TabIndex = 27;
            buttonEdit.Text = "Edit";
            buttonEdit.UseVisualStyleBackColor = true;
            buttonEdit.Click += buttonEdit_Click;
            // 
            // buttonClear
            // 
            buttonClear.Location = new Point(589, 10);
            buttonClear.Name = "buttonClear";
            buttonClear.Size = new Size(94, 29);
            buttonClear.TabIndex = 28;
            buttonClear.Text = "Clear Filter";
            buttonClear.UseVisualStyleBackColor = true;
            buttonClear.Click += buttonClear_Click;
            // 
            // label2
            // 
            label2.BorderStyle = BorderStyle.Fixed3D;
            label2.Location = new Point(12, 458);
            label2.Name = "label2";
            label2.Size = new Size(1401, 2);
            label2.TabIndex = 29;
            // 
            // buttonSyncGames
            // 
            buttonSyncGames.Location = new Point(12, 420);
            buttonSyncGames.Name = "buttonSyncGames";
            buttonSyncGames.Size = new Size(120, 29);
            buttonSyncGames.TabIndex = 30;
            buttonSyncGames.Text = "Sync Games";
            buttonSyncGames.UseVisualStyleBackColor = true;
            buttonSyncGames.Click += buttonSyncGames_Click;
            // 
            // button2
            // 
            button2.Location = new Point(138, 420);
            button2.Name = "button2";
            button2.Size = new Size(120, 29);
            button2.TabIndex = 31;
            button2.Text = "Sync Config";
            button2.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(12, 471);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(1401, 120);
            richTextBox1.TabIndex = 32;
            richTextBox1.Text = "";
            // 
            // Game
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1425, 603);
            Controls.Add(richTextBox1);
            Controls.Add(button2);
            Controls.Add(buttonSyncGames);
            Controls.Add(label2);
            Controls.Add(buttonClear);
            Controls.Add(buttonEdit);
            Controls.Add(buttonDelete);
            Controls.Add(buttonAdd);
            Controls.Add(label1);
            Controls.Add(textBoxFilter);
            Controls.Add(dataGridGame);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Game";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Gamesave Cloud Manager";
            Load += Game_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridGame).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridGame;
        private TextBox textBoxFilter;
        private Label label1;
        private Button buttonAdd;
        private Button buttonDelete;
        private Button buttonEdit;
        private Button buttonClear;
        private Label label2;
        private Button buttonSyncGames;
        private Button button2;
        private RichTextBox richTextBox1;
    }
}