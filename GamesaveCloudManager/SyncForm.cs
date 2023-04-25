using GamesaveCloudCLI;
using GamesaveCloudLib;

#pragma warning disable IDE1006 // Estilos de Nomenclatura
namespace GamesaveCloudManager
{
    public partial class SyncForm : Form
    {
        readonly List<long> games;
        Logger? logger;

        public SyncForm(List<long> games)
        {
            InitializeComponent();
            this.games = games;
            this.Text = $"Synchronizing {games.Count} games";
        }

        private void SyncForm_Load(object sender, EventArgs e)
        {
            comboBoxProvider.Text = "OneDrive";
            comboBoxProvider.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxDirection.Text = "Auto";
            comboBoxDirection.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            buttonStart.Enabled = false;

            try
            {
                logger = new();
                var progress = new Progress<string>(msg =>
                {
                    textBox1.Text += msg;
                    logger.Log(msg);
                });

                Synchronizer sync = new(progress);

                switch (comboBoxProvider.Text.ToLower())
                {
                    case "googledrive":
                        sync.Initialize("googledrive");
                        break;
                    case "onedrive":
                    default:
                        sync.Initialize("onedrive", HelperFunctions.BuildOneDriveClient(), Handle);
                        break;
                }
                StartSync(sync, comboBoxDirection.Text.ToLower(), games);
            }
            catch
            {
                buttonStart.Enabled = true;
            }
        }

        private async void StartSync(Synchronizer sync, string direction, List<long> games)
        {
            await Task.Run(() => SyncBackgroundTask(sync, direction, games)).ContinueWith(EndSync); ;
        }

        private void EndSync(Task obj)
        {
            buttonStart.Invoke((MethodInvoker)delegate
            {
                buttonStart.Enabled = true;
            });
            logger?.Close();
        }

        private static void SyncBackgroundTask(Synchronizer sync, string direction, List<long> games)
        {

            try
            {
                foreach (var game in games)
                {
                    sync.Sync((int)game, null, direction);
                }
            }
            catch (Exception ex)
            {
                sync.Log(ex.ToString());
            }
            sync.Close();
        }
    }
}
#pragma warning restore IDE1006 // Estilos de Nomenclatura