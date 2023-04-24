using System.Data;
using System.Data.SQLite;
using System.Diagnostics;

#pragma warning disable IDE1006 // Estilos de Nomenclatura
namespace GamesaveCloudManager
{
    public partial class GameDetail : Form
    {
        readonly DataRow gameDataRow;
        readonly SQLiteConnection conn;
        DataTable? dtSavegame;
        readonly string mode;
        bool updateMaster = true;

        readonly string queryPath = "select game_id, savegame_id as 'Id', path as 'Path', filter as 'Filter', machine, recursive from savegame where game_id = @game_id order by savegame_id";
        readonly string queryGameUpdate = "update game set title = @title, active = @active where game_id = @game_id";
        readonly string queryGameInsert = "INSERT INTO game (game_id,title,active) VALUES (@game_id,@title,@active)";
        readonly string queryPathDelete = "delete from savegame where game_id = @game_id";
        readonly string queryPathInsert = "INSERT INTO savegame (game_id,savegame_id,path,machine,recursive,filter) VALUES (@game_id,@savegame_id,@path,@machine,@recursive,@filter)";

        public GameDetail(ref DataRow row, SQLiteConnection conn, string mode)
        {
            InitializeComponent();
            this.gameDataRow = row;
            this.conn = conn;
            this.mode = mode;
            groupBoxPath.Visible = false;
        }

        private void GameDetail_Load(object sender, EventArgs e)
        {
            textBoxId.Text = gameDataRow["Id"].ToString();
            if (mode == "EDIT")
            {
                textBoxId.Enabled = false;
                this.Text = "Edit Game " + gameDataRow["Id"].ToString();

                if ((Int64)gameDataRow["Active"] == 1)
                {
                    checkBoxActive.Checked = true;
                }
                else
                {
                    checkBoxActive.Checked = false;
                }
            }
            else
            {
                this.Text = "Add Game";
                checkBoxActive.Checked = true;
            }
            textBoxTitle.Text = gameDataRow["Title"].ToString();

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
            dataGridViewPaths.Columns[4].Width = (int)(dataGridViewPaths.Width * 0.08);
            dataGridViewPaths.Columns[5].Width = (int)(dataGridViewPaths.Width * 0.08);

            if (dataGridViewPaths.Rows.Count > 0)
            {
                dataGridViewPaths.Rows[0].Selected = true;
            }

        }

        private void dataGridViewPaths_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
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
                DataRow[] foundRows = dtSavegame.Select("Id = " + dataGridViewPaths.SelectedRows[0].Cells["Id"].Value.ToString());
                if (foundRows != null && foundRows.Length == 1)
                {
                    int index = dtSavegame.Rows.IndexOf(foundRows[0]);

                    dtSavegame.Rows[index]["Path"] = textBoxPath.Text;

                    dtSavegame.Rows[index]["Filter"] = textBoxFilter.Text;

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

                }
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

        private void buttonFolderBrowser_Click(object sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog();
            if (!String.IsNullOrEmpty(textBoxPath.Text))
            {
                fbd.InitialDirectory = HelperFunctions.ReplaceEnvironmentVariables(textBoxPath.Text);
            }
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                textBoxPath.Text = HelperFunctions.UnreplaceEnvironmentVariables(fbd.SelectedPath);
            }
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

                using var transaction = conn.BeginTransaction();
                try
                {
                    SQLiteCommand cmd;
                    if (mode == "EDIT")
                    {
                        // update game 
                        cmd = new(queryGameUpdate, conn);
                        cmd.Parameters.AddWithValue("@title", textBoxTitle.Text);
                        long active = 0;
                        if (checkBoxActive.Checked) { active = 1; }
                        cmd.Parameters.AddWithValue("@active", active);
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
                                cmd.Parameters.AddWithValue("@Filter", dataRow["filter"]);
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
                            MessageBox.Show("The game id already exists.", "Error");
                            break;
                        case "constraint failed\r\nUNIQUE constraint failed: game.title":
                            MessageBox.Show("The game title already exists.", "Error");
                            break;
                        default:
                            MessageBox.Show(exception.Message, "Error");
                            break;
                    }
                }
            }
        }

        private bool ValidateData()
        {
            if (String.IsNullOrEmpty(textBoxTitle.Text))
            {
                MessageBox.Show("Game title must not be empty.", "Warning");
                textBoxTitle.Focus();
                return false;
            }
            if (dtSavegame != null)
            {
                foreach (DataRow dataRow in dtSavegame.Rows)
                {
                    if (dataRow.RowState != DataRowState.Deleted && String.IsNullOrEmpty(dataRow["Path"].ToString()))
                    {
                        MessageBox.Show("Game path must not be empty.", "Warning");
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
                        return false;
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
            if (!string.IsNullOrEmpty(textBoxPath.Text) && Directory.Exists(HelperFunctions.ReplaceEnvironmentVariables(textBoxPath.Text)))
            {
                ProcessStartInfo startInfo = new()
                {
                    Arguments = HelperFunctions.ReplaceEnvironmentVariables(textBoxPath.Text),
                    FileName = "explorer.exe"
                };
                Process.Start(startInfo);
            };
        }

    }
}
#pragma warning restore IDE1006 // Estilos de Nomenclatura