using GamesaveCloudCLI;
using GamesaveCloudLib;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.NativeInterop;
using System.Data;
using System.Data.SQLite;
using System.Text;
using System.Windows.Forms;

#pragma warning disable IDE1006 // Estilos de Nomenclatura
namespace GamesaveCloudManager
{
    public partial class Game : Form
    {
        DataTable? dtGame;
        SQLiteConnection? conn;
        SQLiteCommand? cmdGame;
        SQLiteDataAdapter? adapter;

        readonly string queryGame = "select game_id as 'Id', title as 'Title', " +
            "(select path from savegame s where s.game_id = g.game_id and s.savegame_id = (select min(savegame_id) from savegame s2 where s2.game_id = g.game_id)) as 'Path', " +
            "active from game g order by title";
        readonly string queryGameDelete = "delete from game where game_id = @game_id";
        private readonly string defaultCloudService = Synchronizer.GetDefaultCloudService();
        string pathDatabaseFile;
        Synchronizer sync;

        public Game()
        {
            InitializeComponent();
        }

        private void Game_Load(object sender, EventArgs e)
        {
            var progress = new Progress<string>(msg =>
            {
                textBoxStatus.Text += msg;
            });
            sync = new(progress);

            /*
            sync.Initialize(null, HelperFunctions.BuildOneDriveClient(), Handle);            
            conn = new SQLiteConnection("Data Source=" + pathDatabaseFile + ";Version=3;New=True;");
            cmdGame = new(queryGame, conn);
            adapter = new(cmdGame);
            LoadData();
            */

            StartSync(sync, HelperFunctions.BuildOneDriveClient(), Handle);
        }

        private async void StartSync(Synchronizer sync, IPublicClientApplication app, IntPtr handle)
        {
            await Task.Run(() => SyncBackgroundTask(sync, app, handle)).ContinueWith(EndSync); ;
        }

        private void EndSync(Task obj)
        {
            pathDatabaseFile = Synchronizer.GetPathDatabaseFile();
            conn = new SQLiteConnection("Data Source=" + pathDatabaseFile + ";Version=3;New=True;");
            cmdGame = new(queryGame, conn);
            adapter = new(cmdGame);
            LoadData();
        }

        private static void SyncBackgroundTask(Synchronizer sync, IPublicClientApplication app, IntPtr handle)
        {
            try
            {
                sync.Initialize(null, HelperFunctions.BuildOneDriveClient(), handle);
            }
            catch (Exception ex)
            {
                sync.Log(ex.ToString());
            }
            sync.Close();
        }


        private void LoadData()
        {
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
                });
                textBoxFilter.Invoke((MethodInvoker)delegate
                {
                    if (!String.IsNullOrEmpty(textBoxFilter.Text))
                    {
                        dtGame.DefaultView.RowFilter = string.Format("[_RowString] LIKE '%{0}%'", textBoxFilter.Text);
                    }
                });
                dataGridGame.Invoke((MethodInvoker)delegate
                {
                    dataGridGame.ClearSelection();
                });
            }
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                dataGridGame.Columns.Remove("Active");
            }
            catch { };
            DataGridViewCheckBoxColumn colActive = new()
            {
                DataPropertyName = "active",
                Name = "Active",
                TrueValue = 1,
                FalseValue = 0
            };
            dataGridGame.Columns.Add(colActive);

            dataGridGame.Columns["_RowString"].Visible = false;
            //dataGridView1.Columns["active"].Visible = false;
            dataGridGame.Columns["Id"].Width = (int)(dataGridGame.Width * 0.08);
            // also may be a good idea to set FILL for the last column
            // to accomodate the round up in conversions
            dataGridGame.Columns["Title"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridGame.Columns["Path"].Width = (int)(dataGridGame.Width * 0.60);
            dataGridGame.Columns["Active"].Width = (int)(dataGridGame.Width * 0.05);
            colActive.Width = (int)(dataGridGame.Width * 0.05);

        }

        private void dataGridGame_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (dataGridGame.SelectedRows.Count == 1)
            {
                buttonEdit.Enabled = true;
                buttonDelete.Enabled = true;

            }
            else
            {
                buttonEdit.Enabled = false;
                buttonDelete.Enabled = false;
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
                GameDetail detailForm = new(ref dataRow, conn, "ADD");
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
            // Calculate amount code goes here
            if (conn != null && dataGridGame.SelectedRows.Count == 1)
            {
                if (dtGame != null)
                {
                    var selectedGameId = dataGridGame.SelectedRows[0].Cells["Id"].Value;
                    DataRow[] foundRows = dtGame.Select("Id = " + selectedGameId.ToString());

                    if (foundRows != null && foundRows.Length == 1)
                    {
                        GameDetail detailForm = new(ref foundRows[0], conn, "EDIT");
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
            if (conn != null)
            {
                conn.Open();
                SQLiteCommand cmd = new(queryGameDelete, conn);
                cmd.Parameters.AddWithValue("@game_id", dataGridGame.SelectedRows[0].Cells["Id"].Value);
                cmd.ExecuteNonQuery();
                conn.Close();
                LoadData();
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

            /*
            var progress = new Progress<string>(msg =>
            {
                richTextBox1.Text += msg;
            });

            Synchronizer sync = new(progress);
            sync.Initialize("onedrive", HelperFunctions.BuildOneDriveClient(), this.Handle);
            */

            SyncForm syncForm = new(games);
            if (syncForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
            }
        }

    }
}
#pragma warning restore IDE1006 // Estilos de Nomenclatura