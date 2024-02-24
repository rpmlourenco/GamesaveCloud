using GamesaveCloudLib;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

#pragma warning disable IDE1006 // Estilos de Nomenclatura
namespace GamesaveCloudManager
{
    public partial class GameDetail : Form
    {
        //readonly string gamesFolder;
        readonly DataRow gameDataRow;
        readonly SQLiteConnection conn;
        DataTable? dtSavegame;
        readonly string mode;
        bool updateMaster = true;
        bool processrowchanged = true;
        readonly IGDBHelper igdbHelper;

        readonly string queryPath = "select game_id, savegame_id as 'Id', path as 'Path', filter as 'Filter', filter_out as 'Filter Out', machine, recursive from savegame where game_id = @game_id order by savegame_id";
        readonly string queryGameUpdate = "update game set title = @title, active = @active, platform = @platform, exec_path = @exec_path, install_path = @install_path, admin = @admin, tdvision = @tdvision, arguments = @arguments, stop_monitor = @stop_monitor where game_id = @game_id";
        readonly string queryGameInsert = "insert into game (game_id,title,active, platform, exec_path, install_path, admin, tdvision, arguments, stop_monitor) VALUES (@game_id,@title,@active,@platform,@exec_path,@install_path,@admin,@tdvision,@arguments, @stop_monitor)";
        readonly string queryPathDelete = "delete from savegame where game_id = @game_id";
        readonly string queryPathInsert = "insert into savegame (game_id,savegame_id,path,machine,recursive,filter,filter_out) VALUES (@game_id,@savegame_id,@path,@machine,@recursive,@filter,@filter_out)";

        public GameDetail(ref DataRow row, SQLiteConnection conn, string mode, IGDBHelper igdbHelper)
        {
            InitializeComponent();
            this.gameDataRow = row;
            this.conn = conn;
            this.mode = mode;
            this.igdbHelper = igdbHelper;
            groupBoxPath.Visible = false;
        }

        private void GameDetail_Load(object sender, EventArgs e)
        {
            textBoxId.Text = gameDataRow["Id"].ToString();
            if (mode == "EDIT")
            {
                textBoxId.Enabled = false;
                buttonIGDBId.Enabled = false;
                this.Text = "Edit Game " + gameDataRow["Id"].ToString();

                if (gameDataRow["Active"] == System.DBNull.Value || (Int64)gameDataRow["Active"] == 1)
                {
                    checkBoxActive.Checked = true;
                }
                else
                {
                    checkBoxActive.Checked = false;
                }

                if (gameDataRow["admin"] == System.DBNull.Value || (Int64)gameDataRow["admin"] == 0)
                {
                    checkBoxAdmin.Checked = false;
                }
                else
                {
                    checkBoxAdmin.Checked = true;
                }

                if (gameDataRow["tdvision"] == System.DBNull.Value || (Int64)gameDataRow["tdvision"] == 0)
                {
                    checkBox3DVision.Checked = false;
                }
                else
                {
                    checkBox3DVision.Checked = true;
                }

                if (gameDataRow["stop_monitor"] == System.DBNull.Value || (Int64)gameDataRow["stop_monitor"] == 0)
                {
                    checkBoxStopMonitor.Checked = false;
                }
                else
                {
                    checkBoxStopMonitor.Checked = true;
                }


            }
            else
            {
                textBoxId.Enabled = true;
                buttonIGDBId.Enabled = true;
                this.Text = "Add Game";
                checkBoxActive.Checked = true;
                checkBoxAdmin.Checked = false;
                checkBox3DVision.Checked = false;
            }
            textBoxTitle.Text = gameDataRow["Title"].ToString();

            comboBoxPlatform.Text = gameDataRow["platform"].ToString();
            comboBoxPlatform.DropDownStyle = ComboBoxStyle.DropDownList;
            textBoxExecPath.Text = gameDataRow["exec_path"].ToString();
            textBoxInstallPath.Text = gameDataRow["install_path"].ToString();
            textBoxArguments.Text = gameDataRow["arguments"].ToString();

            SQLiteCommand cmd = new(queryPath, conn);
            dtSavegame = new();
            cmd.Parameters.AddWithValue("@game_id", gameDataRow["Id"]);
            SQLiteDataAdapter adapterSavegame = new(cmd);
            adapterSavegame.Fill(dtSavegame);

            dataGridViewPaths.DataSource = dtSavegame;
            dataGridViewPaths.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewPaths.ColumnHeadersDefaultCellStyle.Font = new Font(DataGridView.DefaultFont, FontStyle.Bold);

        }

        private void dataGridViewPaths_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                dataGridViewPaths.Columns.Remove("machine");
            }
            catch { };
            DataGridViewCheckBoxColumn colMachine = new()
            {
                DataPropertyName = "machine",
                Name = "Machine",
                TrueValue = 1,
                FalseValue = 0
            };
            dataGridViewPaths.Columns.Add(colMachine);

            try
            {
                dataGridViewPaths.Columns.Remove("recursive");
            }
            catch { };
            DataGridViewCheckBoxColumn colRecursive = new()
            {
                DataPropertyName = "Recursive",
                Name = "Recursive",
                TrueValue = 1,
                FalseValue = 0
            };
            dataGridViewPaths.Columns.Add(colRecursive);

            dataGridViewPaths.Columns["game_id"].Visible = false;
            dataGridViewPaths.Columns[1].Width = (int)(dataGridViewPaths.Width * 0.07);
            dataGridViewPaths.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewPaths.Columns[3].Width = (int)(dataGridViewPaths.Width * 0.15);
            dataGridViewPaths.Columns[4].Width = (int)(dataGridViewPaths.Width * 0.15);
            dataGridViewPaths.Columns[5].Width = (int)(dataGridViewPaths.Width * 0.08);
            dataGridViewPaths.Columns[6].Width = (int)(dataGridViewPaths.Width * 0.08);

            if (dataGridViewPaths.Rows.Count > 0)
            {
                dataGridViewPaths.Rows[0].Selected = true;
            }

        }

        private void dataGridViewPaths_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (!processrowchanged) return;

            if (dataGridViewPaths.SelectedRows.Count == 1)
            {
                buttonDelete.Enabled = true;
            }
            else
            {
                buttonDelete.Enabled = false;
            }

            // For any other operation except, StateChanged, do nothing
            if (e.StateChanged != DataGridViewElementStates.Selected) return;

            // Calculate amount code goes here
            if (dataGridViewPaths.SelectedRows.Count == 1)
            {
                updateMaster = false;

                textBoxPathId.Text = dataGridViewPaths.SelectedRows[0].Cells["Id"].Value.ToString();
                textBoxPath.Text = dataGridViewPaths.SelectedRows[0].Cells["Path"].Value.ToString();
                textBoxFilter.Text = dataGridViewPaths.SelectedRows[0].Cells["Filter"].Value.ToString();
                textBoxFilterOut.Text = dataGridViewPaths.SelectedRows[0].Cells["Filter Out"].Value.ToString();

                if ((Int64)dataGridViewPaths.SelectedRows[0].Cells["machine"].Value == 1)
                {
                    checkBoxMachine.Checked = true;
                }
                else
                {
                    checkBoxMachine.Checked = false;
                }

                if ((Int64)dataGridViewPaths.SelectedRows[0].Cells["recursive"].Value == 1)
                {
                    checkBoxRecursive.Checked = true;
                }
                else
                {
                    checkBoxRecursive.Checked = false;
                }

                updateMaster = true;

                groupBoxPath.Visible = true;
            }
            else
            {
                groupBoxPath.Visible = false;
            }

        }

        private void UpdateMaster()
        {
            if (dtSavegame != null && updateMaster)
            {
                updateMaster = false;
                processrowchanged = false;

                DataRow[] foundRows = dtSavegame.Select("Id = " + dataGridViewPaths.SelectedRows[0].Cells["Id"].Value.ToString());
                if (foundRows != null && foundRows.Length == 1)
                {
                    DataGridViewRow gridrow = dataGridViewPaths.CurrentRow;
                    int index = dtSavegame.Rows.IndexOf(foundRows[0]);

                    dtSavegame.Rows[index]["Path"] = textBoxPath.Text;
                    dtSavegame.Rows[index]["Filter"] = textBoxFilter.Text;
                    dtSavegame.Rows[index]["Filter Out"] = textBoxFilterOut.Text;

                    if (checkBoxMachine.Checked == true)
                    {
                        dtSavegame.Rows[index]["machine"] = 1;
                    }
                    else
                    {
                        dtSavegame.Rows[index]["machine"] = 0;
                    }

                    if (checkBoxRecursive.Checked == true)
                    {
                        dtSavegame.Rows[index]["recursive"] = 1;
                    }
                    else
                    {
                        dtSavegame.Rows[index]["recursive"] = 0;
                    }

                    dataGridViewPaths.ClearSelection();
                    dataGridViewPaths.Rows[gridrow.Index].Selected = true;
                    dataGridViewPaths.CurrentCell = dataGridViewPaths[1, gridrow.Index];

                }

                processrowchanged = true;
                updateMaster = true;
            }
        }

        private void checkBoxMachine_CheckedChanged(object sender, EventArgs e)
        {
            UpdateMaster();
        }

        private void checkBoxRecursive_CheckedChanged(object sender, EventArgs e)
        {
            UpdateMaster();
        }

        private void textBoxPath_TextChanged(object sender, EventArgs e)
        {
            UpdateMaster();
        }

        private void textBoxFilter_TextChanged(object sender, EventArgs e)
        {
            UpdateMaster();
        }

        private void textBoxFilterOut_TextChanged(object sender, EventArgs e)
        {
            UpdateMaster();
        }

        private void buttonFolderBrowser_Click(object sender, EventArgs e)
        {
            var sync = new Synchronizer(null);
            using var fbd = new FolderBrowserDialog();
            if (!String.IsNullOrEmpty(textBoxPath.Text))
            {
                //fbd.InitialDirectory = HelperFunctions.ReplaceEnvironmentVariables(textBoxPath.Text);
                fbd.InitialDirectory = sync.ReplaceVariables(textBoxPath.Text);

            }
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                textBoxPath.Text = sync.UnreplaceVariables(fbd.SelectedPath);
            }
            sync.Close();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (dtSavegame != null)
            {
                long max = 0;
                if (dtSavegame.Rows.Count > 0)
                {
                    try
                    {
                        max = (long)dtSavegame.Compute("MAX([Id])", "");

                    }
                    catch { }
                }

                DataRow dataRow = dtSavegame.NewRow();
                dataRow["Id"] = max + 1;
                dataRow["machine"] = 0;
                dataRow["recursive"] = 1;
                dtSavegame.Rows.Add(dataRow);

                dataGridViewPaths.ClearSelection();
                foreach (DataGridViewRow gridrow in dataGridViewPaths.Rows)
                {
                    if (gridrow.Cells["Id"].Value.Equals(max + 1))
                    {
                        dataGridViewPaths.Rows[gridrow.Index].Selected = true;
                        break;
                    }
                }
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (dtSavegame != null)
            {
                DataRow[] foundRows = dtSavegame.Select("Id = " + dataGridViewPaths.SelectedRows[0].Cells["Id"].Value.ToString());
                if (foundRows != null && foundRows.Length == 1)
                {
                    int index = dtSavegame.Rows.IndexOf(foundRows[0]);
                    dtSavegame.Rows[index].Delete();
                    dataGridViewPaths.ClearSelection();
                    groupBoxPath.Visible = false;
                }
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                conn.Open();

                SQLiteCommand cmd = new("PRAGMA foreign_keys=ON", conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();

                using var transaction = conn.BeginTransaction();
                try
                {
                    //SQLiteCommand cmd;
                    if (mode == "EDIT")
                    {
                        // update game 
                        cmd = new(queryGameUpdate, conn);
                        cmd.Parameters.AddWithValue("@title", textBoxTitle.Text);
                        long active = 0;
                        if (checkBoxActive.Checked) { active = 1; }
                        cmd.Parameters.AddWithValue("@active", active);
                        cmd.Parameters.AddWithValue("@platform", comboBoxPlatform.Text);
                        cmd.Parameters.AddWithValue("@exec_path", textBoxExecPath.Text);
                        cmd.Parameters.AddWithValue("@install_path", textBoxInstallPath.Text);
                        long admin = 0;
                        if (checkBoxAdmin.Checked) { admin = 1; }
                        cmd.Parameters.AddWithValue("@admin", admin);
                        long tdvision = 0;
                        if (checkBox3DVision.Checked) { tdvision = 1; }
                        cmd.Parameters.AddWithValue("@tdvision", tdvision);
                        cmd.Parameters.AddWithValue("@arguments", textBoxArguments.Text);
                        long stop_monitor = 0;
                        if (checkBoxStopMonitor.Checked) { stop_monitor = 1; }
                        cmd.Parameters.AddWithValue("@stop_monitor", stop_monitor);

                        cmd.Parameters.AddWithValue("@game_id", gameDataRow["Id"]);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        gameDataRow["Id"] = long.Parse(textBoxId.Text);
                        // insert game                     
                        cmd = new(queryGameInsert, conn);
                        cmd.Parameters.AddWithValue("@title", textBoxTitle.Text);
                        long active = 0;
                        if (checkBoxActive.Checked) { active = 1; }
                        cmd.Parameters.AddWithValue("@active", active);
                        cmd.Parameters.AddWithValue("@platform", comboBoxPlatform.Text);
                        cmd.Parameters.AddWithValue("@exec_path", textBoxExecPath.Text);
                        cmd.Parameters.AddWithValue("@install_path", textBoxInstallPath.Text);
                        long admin = 0;
                        if (checkBoxAdmin.Checked) { admin = 1; }
                        cmd.Parameters.AddWithValue("@admin", admin);
                        long tdvision = 0;
                        if (checkBox3DVision.Checked) { tdvision = 1; }
                        cmd.Parameters.AddWithValue("@tdvision", tdvision);
                        cmd.Parameters.AddWithValue("@arguments", textBoxArguments.Text);
                        long stop_monitor = 0;
                        if (checkBoxStopMonitor.Checked) { stop_monitor = 1; }
                        cmd.Parameters.AddWithValue("@stop_monitor", stop_monitor);

                        cmd.Parameters.AddWithValue("@game_id", gameDataRow["Id"]);
                        cmd.ExecuteNonQuery();
                    }

                    // update paths
                    cmd = new(queryPathDelete, conn);
                    cmd.Parameters.AddWithValue("@game_id", gameDataRow["Id"]);
                    cmd.ExecuteNonQuery();

                    if (dtSavegame != null)
                    {
                        foreach (DataRow dataRow in dtSavegame.Rows)
                        {
                            if (dataRow.RowState != DataRowState.Deleted)
                            {
                                cmd = new(queryPathInsert, conn);
                                cmd.Parameters.AddWithValue("@game_id", gameDataRow["Id"]);
                                cmd.Parameters.AddWithValue("@savegame_id", dataRow["Id"]);
                                cmd.Parameters.AddWithValue("@path", dataRow["Path"]);
                                cmd.Parameters.AddWithValue("@machine", dataRow["machine"]);
                                cmd.Parameters.AddWithValue("@recursive", dataRow["recursive"]);
                                cmd.Parameters.AddWithValue("@Filter", dataRow["Filter"]);
                                cmd.Parameters.AddWithValue("@Filter_out", dataRow["Filter Out"]);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    transaction.Commit();
                    conn.Close();
                    DialogResult = System.Windows.Forms.DialogResult.OK;
                    this.Close();
                }
                catch (Exception exception)
                {
                    transaction.Rollback();
                    conn.Close();
                    switch (exception.Message)
                    {
                        case "constraint failed\r\nUNIQUE constraint failed: game.game_id":
                            MessageBox.Show("The game id already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case "constraint failed\r\nUNIQUE constraint failed: game.title":
                            MessageBox.Show("The game title already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        default:
                            MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                    }
                }
            }
        }

        private bool ValidateData()
        {
            if (String.IsNullOrEmpty(textBoxTitle.Text))
            {
                MessageBox.Show("Game title must not be empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxTitle.Focus();
                return false;
            }
            if (String.IsNullOrEmpty(comboBoxPlatform.Text))
            {
                MessageBox.Show("Game platform must not be empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBoxPlatform.Focus();
                return false;
            }
            if (comboBoxPlatform.Text.Equals("PC (Windows)") && String.IsNullOrEmpty(textBoxExecPath.Text))
            {
                MessageBox.Show("Game executable path must not be empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxExecPath.Focus();
                return false;
            }
            if (comboBoxPlatform.Text.Equals("PC (Windows)") && String.IsNullOrEmpty(textBoxInstallPath.Text))
            {
                MessageBox.Show("Game installation path must not be empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxInstallPath.Focus();
                return false;
            }

            if (dtSavegame != null)
            {
                foreach (DataRow dataRow in dtSavegame.Rows)
                {
                    if (dataRow.RowState != DataRowState.Deleted)
                    {
                        bool rowValid = true;
                        if (String.IsNullOrEmpty(dataRow["Path"].ToString()))
                        {
                            MessageBox.Show("Game path must not be empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            rowValid = false;
                        }

                        if (!rowValid)
                        {
                            dataGridViewPaths.ClearSelection();
                            foreach (DataGridViewRow gridrow in dataGridViewPaths.Rows)
                            {
                                if (gridrow.Cells["Id"].Value.Equals(dataRow["Id"]))
                                {
                                    dataGridViewPaths.Rows[gridrow.Index].Selected = true;
                                    //dataGridViewPaths.FirstDisplayedScrollingRowIndex = dataGridViewPaths.SelectedRows[0].Index;

                                    //needs to be set to column 1 because column 0 is empty
                                    dataGridViewPaths.CurrentCell = dataGridViewPaths[1, gridrow.Index];
                                    textBoxPath.Focus();
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;

        }

        private void textBoxId_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void buttonFolderOpen_Click(object sender, EventArgs e)
        {
            var sync = new Synchronizer(null);
            var path = sync.ReplaceVariables(textBoxPath.Text);
            sync.Close();

            if (!string.IsNullOrEmpty(textBoxPath.Text) && Directory.Exists(path))
            {
                ProcessStartInfo startInfo = new()
                {
                    Arguments = path,
                    FileName = "explorer.exe"
                };
                System.Diagnostics.Process.Start(startInfo);
            };
        }

        private async void buttonIGDBId_Click(object sender, EventArgs e)
        {
            var task = igdbHelper.GetIdAsync(textBoxTitle.Text);
            var id = await task;
            if (id != default)
            {
                textBoxId.Text = id.ToString();
            }

        }

        private async void buttonIGDB_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBoxId.Text))
            {
                var task = igdbHelper.GetGameAsync(long.Parse(textBoxId.Text));
                var url = await task;
                if (url != null)
                {
                    System.Diagnostics.Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
            }
        }

        private void buttonExecFileBrowser_Click(object sender, EventArgs e)
        {
            var sync = new Synchronizer(null);
            var gamesPath = sync.GetGamesPath();
            sync.Close();

            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = gamesPath,
                Title = "Browse Executable Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "exe",
                Filter = "Executable files|*.exe;*.cmd;*.bat;*.lnk;*.ps1|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (!openFileDialog1.FileName.StartsWith(gamesPath, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Executable must be inside games folder.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxTitle.Focus();
                    return;
                }

                var fullExecPath = openFileDialog1.FileName.Substring(gamesPath.Length + 1);
                var split = fullExecPath.Split(Path.DirectorySeparatorChar);
                textBoxInstallPath.Text = split.First();
                split[0] = "{InstallDir}";
                textBoxExecPath.Text = String.Join(Path.DirectorySeparatorChar, split); ;

                //textBoxExecPath.Text = openFileDialog1.FileName.Replace(gamesPath, "{InstallDir}");
            }

        }

        private void buttonInstallFolderBrowser_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog();
            fbd.InitialDirectory = @"D:\Games";
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                textBoxInstallPath.Text = fbd.SelectedPath;
                if (textBoxExecPath.Text.Contains("{InstallPath}"))
                {
                    textBoxExecPath.Text = "";
                }
            }
        }
    }
}
#pragma warning restore IDE1006 // Estilos de Nomenclatura