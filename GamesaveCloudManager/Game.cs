using GamesaveCloudLib;
using Microsoft.Identity.Client;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Text;

#pragma warning disable IDE1006 // Estilos de Nomenclatura
namespace GamesaveCloudManager
{
    public partial class Game : Form
    {
        //string? gamesFolder;
        DataTable? dtGame;
        SQLiteConnection? conn;
        SQLiteCommand? cmdGame;
        SQLiteDataAdapter? adapter;
        readonly IGDBHelper igdbHelper = new();

        readonly string queryGame = "select game_id as 'Id', title as 'Title', " +
            "(select path from savegame s where s.game_id = g.game_id and s.savegame_id = (select min(savegame_id) from savegame s2 where s2.game_id = g.game_id)) as 'Path', " +
            "active, platform, exec_path, install_path, admin, tdvision, arguments, stop_monitor from game g order by title";
        readonly string queryGameDelete = "delete from game where game_id = @game_id";
        //readonly string queryGetGamesFolder = "select value from parameter where parameter_id = 'GAMES_FOLDER'";
        readonly string queryGetWindowsGames = "select title, install_path from game where active = 1 and platform = 'PC (Windows)'";

        string? pathDatabaseFile;

        bool closing = false;

        public Game()
        {
            InitializeComponent();
            Enabled = false;
        }

        private void Game_Load(object sender, EventArgs e)
        {

            System.Reflection.Assembly ass = System.Reflection.Assembly.GetExecutingAssembly();
            System.Version? ver = ass.GetName().Version;
            if (ver != null) this.Text += " " + ver.Major + "." + ver.Minor;

            var progress = new Progress<string>(msg =>
            {
                textBoxStatus.AppendText(msg);
            });

            Synchronizer sync = new(progress);
            var clientApp = HelperFunctions.BuildOneDriveClient();
            if (clientApp != null)
            {
                StartSync(sync, clientApp, Handle);
            }

        }

        private async void StartSync(Synchronizer sync, IPublicClientApplication app, IntPtr handle)
        {
            labelStatus.Text = "Synchronizing database";
            pathDatabaseFile = sync.GetPathDatabaseFile();
            await Task.Run(() => SyncBackgroundTask(sync, app, handle)).ContinueWith(EndSync, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void EndSync(Task obj)
        {
            labelStatus.Invoke((MethodInvoker)delegate { labelStatus.Text = "Database Synchronized"; });

            //pathDatabaseFile = Synchronizer.GetPathDatabaseFile();
            conn = new SQLiteConnection("Data Source=" + pathDatabaseFile + ";Version=3;New=True;");
            cmdGame = new(queryGame, conn);
            adapter = new(cmdGame);

            //conn.Open();
            //var sqlite_cmd = conn.CreateCommand();
            //sqlite_cmd.CommandText = queryGetGamesFolder;
            //var sqlite_datareader = sqlite_cmd.ExecuteReader();
            //while (sqlite_datareader.Read())
            //{
            //    gamesFolder = sqlite_datareader.GetString(0);

            //}
            //sqlite_datareader.Close();
            //conn.Close();
            LoadData();
        }

        private static void SyncBackgroundTask(Synchronizer sync, IPublicClientApplication app, IntPtr handle)
        {
            try
            {
                sync.Initialize(null, app, handle, true);
            }
            catch (Exception ex)
            {
                sync.Log(ex.ToString());
            }
            sync.Close();
        }


        private void LoadData()
        {
            labelStatus.Invoke((MethodInvoker)delegate { labelStatus.Text = "Loading database"; });
            if (adapter != null)
            {
                dtGame = new();
                adapter.Fill(dtGame);

                DataColumn dcRowString = dtGame.Columns.Add("_RowString", typeof(string));
                foreach (DataRow dataRow in dtGame.Rows)
                {
                    StringBuilder sb = new();
                    for (int i = 0; i < dtGame.Columns.Count - 1; i++)
                    {
                        sb.Append(dataRow[i].ToString());
                        sb.Append('\t');
                    }
                    dataRow[dcRowString] = sb.ToString();
                }

                dataGridGame.Invoke((MethodInvoker)delegate
                {
                    dataGridGame.DataSource = dtGame;
                    dataGridGame.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                    dataGridGame.ColumnHeadersDefaultCellStyle.Font = new Font(DataGridView.DefaultFont, FontStyle.Bold);
                    dataGridGame.ClearSelection();
                });
                textBoxFilter.Invoke((MethodInvoker)delegate
                {
                    if (!String.IsNullOrEmpty(textBoxFilter.Text))
                    {
                        dtGame.DefaultView.RowFilter = string.Format("[_RowString] LIKE '%{0}%'", textBoxFilter.Text);
                    }
                });

                this.Invoke((MethodInvoker)delegate { this.Enabled = true; });
                labelStatus.Invoke((MethodInvoker)delegate { labelStatus.Text = $"Displaying {dtGame.DefaultView.Count} out of {dtGame.Rows.Count} games"; });

            }
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                //dataGridGame.Columns.Remove("platform");
                dataGridGame.Columns.Remove("exec_path");
                dataGridGame.Columns.Remove("install_path");
                dataGridGame.Columns.Remove("admin");
                dataGridGame.Columns.Remove("tdvision");
                dataGridGame.Columns.Remove("arguments");
                dataGridGame.Columns.Remove("stop_monitor");
                dataGridGame.Columns.Remove("active");
            }
            catch { };

            dataGridGame.Columns["platform"].HeaderText = "Platform";

            if (dataGridGame.Columns["Active"] == null)
            {
                DataGridViewCheckBoxColumn colActive = new()
                {
                    DataPropertyName = "active",
                    Name = "Active",
                    TrueValue = 1,
                    FalseValue = 0
                };
                dataGridGame.Columns.Add(colActive);
            }

            dataGridGame.Columns["_RowString"].Visible = false;
            //dataGridView1.Columns["active"].Visible = false;
            dataGridGame.Columns["Id"].Width = (int)(dataGridGame.Width * 0.08);
            // also may be a good idea to set FILL for the last column
            // to accomodate the round up in conversions
            dataGridGame.Columns["Title"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridGame.Columns["Path"].Width = (int)(dataGridGame.Width * 0.50);
            dataGridGame.Columns["platform"].Width = (int)(dataGridGame.Width * 0.10);
            dataGridGame.Columns["Active"].Width = (int)(dataGridGame.Width * 0.05);
        }

        private void dataGridGame_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (dataGridGame.SelectedRows.Count == 1)
            {
                buttonEdit.Enabled = true;
                buttonDelete.Enabled = true;
                buttonIGDB.Enabled = true;

            }
            else
            {
                buttonEdit.Enabled = false;
                buttonDelete.Enabled = false;
                buttonIGDB.Enabled = false;
            }

            if (dataGridGame.SelectedRows.Count > 0)
            {
                buttonSyncGames.Enabled = true;
            }
            else
            {
                buttonSyncGames.Enabled = false;
            }

            // For any other operation except, StateChanged, do nothing
            //if (e.StateChanged != DataGridViewElementStates.Selected) return;

            //TODO

        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (conn != null && dtGame != null)
            {
                DataRow dataRow = dtGame.NewRow();
                GameDetail detailForm = new(ref dataRow, conn, "ADD", igdbHelper);
                if (detailForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var addedGameId = dataRow["Id"];
                    LoadData();
                    foreach (DataGridViewRow gridrow in dataGridGame.Rows)
                    {
                        if (gridrow.Cells["Id"].Value.Equals(addedGameId))
                        {
                            dataGridGame.Rows[gridrow.Index].Selected = true;
                            dataGridGame.CurrentCell = dataGridGame[0, gridrow.Index];
                            break;
                        }
                    }
                }
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (conn != null && dataGridGame.SelectedRows.Count == 1)
            {
                if (dtGame != null)
                {
                    var selectedGameId = dataGridGame.SelectedRows[0].Cells["Id"].Value;
                    DataRow[] foundRows = dtGame.Select("Id = " + selectedGameId.ToString());

                    if (foundRows != null && foundRows.Length == 1)
                    {
                        GameDetail detailForm = new(ref foundRows[0], conn, "EDIT", igdbHelper);
                        if (detailForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            LoadData();
                            foreach (DataGridViewRow gridrow in dataGridGame.Rows)
                            {
                                if (gridrow.Cells["Id"].Value.Equals(selectedGameId))
                                {
                                    dataGridGame.Rows[gridrow.Index].Selected = true;
                                    //dataGridGame.FirstDisplayedScrollingRowIndex = dataGridGame.SelectedRows[0].Index;
                                    dataGridGame.CurrentCell = dataGridGame[0, gridrow.Index];
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void textBoxFilter_TextChanged(object sender, EventArgs e)
        {
            if (dtGame != null)
            {
                //dtGame.DefaultView.RowFilter = string.Format("Convert([{0}], 'System.String') LIKE '%{1}%'", "Title", textBox1.Text);
                dtGame.DefaultView.RowFilter = string.Format("[_RowString] LIKE '%{0}%'", textBoxFilter.Text);
                labelStatus.Text = $"Displaying {dtGame.DefaultView.Count} out of {dtGame.Rows.Count} games";
            }

        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBoxFilter.Text = string.Empty;
            if (dtGame != null)
            {
                dtGame.DefaultView.RowFilter = string.Empty;
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show($"Do you really want to delete '{dataGridGame.SelectedRows[0].Cells["Title"].Value}'?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                if (conn != null)
                {
                    conn.Open();
                    SQLiteCommand cmd = new("PRAGMA foreign_keys=ON", conn);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    cmd = new(queryGameDelete, conn);
                    cmd.Parameters.AddWithValue("@game_id", dataGridGame.SelectedRows[0].Cells["Id"].Value);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    conn.Close();
                    LoadData();
                }
            }
        }

        private void buttonSyncGames_Click(object sender, EventArgs e)
        {

            var selectedRows = dataGridGame.SelectedRows;
            List<long> games = new();
            foreach (DataGridViewRow row in selectedRows)
            {
                games.Add((long)row.Cells["Id"].Value);
            }

            SyncForm syncForm = new(games);
            if (syncForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
            }
        }

        private void buttonSyncConfig_Click(object sender, EventArgs e)
        {
            var progress = new Progress<string>(msg =>
            {
                textBoxStatus.AppendText(msg);
            });
            Synchronizer sync = new(progress);
            var clientApp = HelperFunctions.BuildOneDriveClient();
            if (clientApp != null)
            {
                Enabled = false;
                StartSyncConfig(sync, clientApp, Handle);
            }

        }

        private async void StartSyncConfig(Synchronizer sync, IPublicClientApplication app, IntPtr handle)
        {
            labelStatus.Text = "Synchronizing database";
            await Task.Run(() => SyncBackgroundTask(sync, app, handle)).ContinueWith(EndSyncConfig, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void EndSyncConfig(Task obj)
        {
            if (!closing)
            {
                this.Invoke((MethodInvoker)delegate { this.Enabled = true; });
                if (dtGame != null)
                {
                    labelStatus.Invoke((MethodInvoker)delegate { labelStatus.Text = $"Displaying {dtGame.DefaultView.Count} out of {dtGame.Rows.Count} games"; });
                }

            }
            else
            {
                this.Close();
            }
        }

        private async void buttonIGDB_Click(object sender, EventArgs e)
        {
            if (dataGridGame.SelectedRows.Count == 1)
            {
                var task = igdbHelper.GetGameAsync((long)dataGridGame.SelectedRows[0].Cells["Id"].Value);
                var url = await task;
                if (url != null)
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
            }
        }

        private void Game_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closing)
            {
                closing = true;
                e.Cancel = true;
                buttonSyncConfig_Click(this, new EventArgs());
            }
        }

        private void buttonCheckBackup_Click(object sender, EventArgs e)
        {
            textBoxStatus.AppendText($"----- Checking Backup -----{Environment.NewLine}");
            SQLiteConnection conn = new SQLiteConnection("Data Source=" + pathDatabaseFile + ";Version=3;New=True;");
            conn.Open();
            var sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = queryGetWindowsGames;
            var sqlreader = sqlite_cmd.ExecuteReader();
            while (sqlreader.Read())
            {
                var title = sqlreader.GetString(0);
                var install_path = sqlreader.GetString(1);
                if (!Directory.Exists($@"E:\Gamevault\{install_path}"))
                {
                    textBoxStatus.AppendText($"No Backup for {title}{Environment.NewLine}");
                }

            }
            sqlreader.Close();
            conn.Close();
        }
    }
}
#pragma warning restore IDE1006 // Estilos de Nomenclatura