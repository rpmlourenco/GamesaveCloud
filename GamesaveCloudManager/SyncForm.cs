using GamesaveCloudLib;
using System;

#pragma warning disable IDE1006 // Estilos de Nomenclatura
namespace GamesaveCloudManager
{
    public partial class SyncForm : Form
    {
        readonly List<long> games;
        Logger? logger;
        private readonly string defaultCloudService;

        public SyncForm(List<long> games)
        {
            InitializeComponent();
            this.games = games;
            this.Text = $"Synchronizing {games.Count} games";

            var sync = new Synchronizer(null);
            defaultCloudService = sync.GetDefaultCloudService();    
            sync.Close();
        }

        private void SyncForm_Load(object sender, EventArgs e)
        {
            comboBoxProvider.Text = defaultCloudService switch
            {
                "googledrive" => "GoogleDrive",
                _ => "OneDrive",
            };
            comboBoxProvider.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxDirection.Text = "Auto";
            comboBoxDirection.DropDownStyle = ComboBoxStyle.DropDownList;
            buttonClearLog.Enabled = false;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            buttonStart.Enabled = false;
            this.ControlBox = false;
            this.checkBoxAsync.Enabled = false;
            this.comboBoxProvider.Enabled = false;
            this.comboBoxDirection.Enabled = false;
            this.buttonDeleteLocal.Enabled = false;
            this.buttonDeleteCloud.Enabled = false;
            try
            {
                logger = new(Path.GetDirectoryName(Environment.ProcessPath));
                var progress = new Progress<string>(msg =>
                {
                    textBox1.AppendText(msg);
                    logger.Log(msg);
                });

                Synchronizer sync = new(progress);

                switch (comboBoxProvider.Text.ToLower())
                {
                    case "googledrive":
                        sync.Initialize("googledrive", HelperFunctions.BuildOneDriveClient(), Handle, false);
                        break;
                    case "onedrive":
                    default:
                        sync.Initialize("onedrive", HelperFunctions.BuildOneDriveClient(), Handle, false);
                        break;
                }

                StartSync(sync, comboBoxDirection.Text.ToLower(), games, this.checkBoxAsync.Checked);
            }
            catch (Exception exception)
            {
                textBox1.Text += exception.Message;
                buttonStart.Enabled = true;
                this.ControlBox = true;
                this.checkBoxAsync.Enabled = true;
                this.comboBoxProvider.Enabled = true;
                this.comboBoxDirection.Enabled = true;
                this.buttonDeleteLocal.Enabled = true;
                this.buttonDeleteCloud.Enabled = true;
            }
        }

        private async void StartSync(Synchronizer sync, string direction, List<long> games, bool async)
        {
            await Task.Run(() => SyncBackgroundTask(sync, direction, games, async)).ContinueWith(EndSync); ;
        }

        private void EndSync(Task obj)
        {
            buttonStart.Invoke((MethodInvoker)delegate
            {
                buttonStart.Enabled = true;
            });

            buttonDeleteLocal.Invoke((MethodInvoker)delegate
            {
                buttonDeleteLocal.Enabled = true;
            });

            buttonDeleteCloud.Invoke((MethodInvoker)delegate
            {
                buttonDeleteCloud.Enabled = true;
            });

            checkBoxAsync.Invoke((MethodInvoker)delegate
            {
                checkBoxAsync.Enabled = true;
            });

            comboBoxProvider.Invoke((MethodInvoker)delegate
            {
                comboBoxProvider.Enabled = true;
            });

            comboBoxDirection.Invoke((MethodInvoker)delegate
            {
                comboBoxDirection.Enabled = true;
            });

            this.Invoke((MethodInvoker)delegate
            {
                this.ControlBox = true;
            });

            logger?.Close();
        }

        private static void SyncBackgroundTask(Synchronizer sync, string direction, List<long> games, bool async)
        {

            try
            {
                foreach (var game in games)
                {
                    sync.Sync((int)game, null, direction, async);
                }
            }
            catch (Exception ex)
            {
                sync.Log(ex.ToString());
            }
            sync.Close();
        }

        private void SyncForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!buttonStart.Enabled)
            {
                e.Cancel = true;
            }
        }

        private void buttonDeleteLocal_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show($"Do you really want to delete local files?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                this.buttonStart.Enabled = false;
                this.ControlBox = false;
                this.checkBoxAsync.Enabled = false;
                this.comboBoxProvider.Enabled = false;
                this.comboBoxDirection.Enabled = false;
                this.buttonDeleteLocal.Enabled = false;
                this.buttonDeleteCloud.Enabled = false;
                try
                {
                    //logger = new();
                    var progress = new Progress<string>(msg =>
                    {
                        textBox1.AppendText(msg);
                        //logger.Log(msg);
                    });

                    Synchronizer sync = new(progress);

                    switch (comboBoxProvider.Text.ToLower())
                    {
                        case "googledrive":
                            sync.Initialize("googledrive", HelperFunctions.BuildOneDriveClient(), Handle, false);
                            break;
                        case "onedrive":
                        default:
                            sync.Initialize("onedrive", HelperFunctions.BuildOneDriveClient(), Handle, false);
                            break;
                    }

                    try
                    {
                        foreach (var game in games)
                        {
                            sync.DeleteLocalFiles((int)game);
                        }
                    }
                    catch (Exception ex)
                    {
                        sync.Log(ex.ToString());
                    }
                    sync.Close();

                }
                catch (Exception exception)
                {
                    textBox1.Text += exception.Message;
                }

                this.buttonStart.Enabled = true;
                this.ControlBox = true;
                this.checkBoxAsync.Enabled = true;
                this.comboBoxProvider.Enabled = true;
                this.comboBoxDirection.Enabled = true;
                this.buttonDeleteLocal.Enabled = true;
                this.buttonDeleteCloud.Enabled = true;

            }
        }

        private async void buttonDeleteCloud_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show($"Do you really want to delete cloud files?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                this.buttonStart.Enabled = false;
                this.ControlBox = false;
                this.checkBoxAsync.Enabled = false;
                this.comboBoxProvider.Enabled = false;
                this.comboBoxDirection.Enabled = false;
                this.buttonDeleteLocal.Enabled = false;
                this.buttonDeleteCloud.Enabled = false;
                try
                {
                    //logger = new();
                    var progress = new Progress<string>(msg =>
                    {
                        textBox1.AppendText(msg);
                        //logger.Log(msg);
                    });

                    Synchronizer sync = new(progress);

                    switch (comboBoxProvider.Text.ToLower())
                    {
                        case "googledrive":
                            sync.Initialize("googledrive", HelperFunctions.BuildOneDriveClient(), Handle, false);
                            break;
                        case "onedrive":
                        default:
                            sync.Initialize("onedrive", HelperFunctions.BuildOneDriveClient(), Handle, false);
                            break;
                    }

                    try
                    {
                        foreach (var game in games)
                        {
                            await sync.DeleteCloudFiles((int)game);
                        }
                    }
                    catch (Exception ex)
                    {
                        sync.Log(ex.ToString());
                    }
                    sync.Close();

                }
                catch (Exception exception)
                {
                    textBox1.Text += exception.Message;
                }

                this.buttonStart.Enabled = true;
                this.ControlBox = true;
                this.checkBoxAsync.Enabled = true;
                this.comboBoxProvider.Enabled = true;
                this.comboBoxDirection.Enabled = true;
                this.buttonDeleteLocal.Enabled = true;
                this.buttonDeleteCloud.Enabled = true;
            }
        }

        private void buttonClearLog_Click(object sender, EventArgs e)
        {
            textBox1.Text = string.Empty;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0)
            {
                buttonClearLog.Enabled = true;
            }
            else
            {
                buttonClearLog.Enabled = false;
            }
        }
    }
}
#pragma warning restore IDE1006 // Estilos de Nomenclatura