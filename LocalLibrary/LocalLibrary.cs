using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Xml;

namespace LocalLibrary
{
    public class LocalLibrary : LibraryPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private LocalLibrarySettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("56fa97bf-2401-4e7d-adeb-5bca4311a1c9");

        // Change to something more appropriate
        public override string Name => "Local Library";

        // Implementing Client adds ability to open it via special menu in playnite.
        public override LibraryClient Client { get; } = new LocalLibraryClient();

        public LocalLibrary(IPlayniteAPI api) : base(api)
        {
            settings = new LocalLibrarySettingsViewModel(this);
            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };
        }

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            //var client = new IgdbClient("https://api2.playnite.link/api/");
            //this.PlayniteApi.Paths.ExtensionsDataPath + "\\" + this.id
            //var IGDBgame = Task.Run(() => client.GetGame(185246)).Result;

            SQLiteConnection sqlite_conn;
            SQLiteCommand sqlite_cmd;
            SQLiteDataReader sqlreader;
            string gamesFolder = null;

            //var sync = new Synchronizer(null);
            //try
            //{
            //    sync.Initialize(cloudService);
            //}
            //catch (Exception ex)
            //{
            //    sync.Log(ex.ToString());
            //}
            //sync.Close();


            var dbPath = "C:\\Portable Apps\\GamesaveCloud\\config\\GamesaveDB.db";
            sqlite_conn = new SQLiteConnection("Data Source=" + dbPath + ";Version=3;New=True;");
            sqlite_conn.Open();

            sqlite_cmd = sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = "select value from parameter where parameter_id = 'GAMES_FOLDER'";
            sqlreader = sqlite_cmd.ExecuteReader();
            while (sqlreader.Read())
            {
                gamesFolder = sqlreader.GetString(0);
                gamesFolder = gamesFolder.Substring(1, gamesFolder.Length - 1);
            }
            sqlreader.Close();

            sqlite_cmd = sqlite_conn.CreateCommand();
            sqlite_cmd.CommandText = "select game_id, title, install_path, exec_path, arguments from game where active = 1 and platform = 'PC (Windows)'";
            sqlreader = sqlite_cmd.ExecuteReader();

            var games = new List<GameMetadata>();
            while (sqlreader.Read())
            {
                bool installed = false;

                var gameId = sqlreader.GetValue(0).ToString();
                var gameName = sqlreader.GetString(1);
                var installPath = sqlreader.GetString(2);
                var execPath = sqlreader.GetString(3);
                
                string arguments = "";
                if (!sqlreader.IsDBNull(4))
                {
                    arguments = sqlreader.GetString(4);
                }

                string[] drives = new string[] { "C", "D", "E", "F", "G", "H" };

                foreach ( var drive in drives ) {
                    if (Directory.Exists(drive + gamesFolder + "\\" + installPath))
                    {
                        installed = true;
                        installPath = drive + gamesFolder + "\\" + installPath;
                        break;
                    }
                }
                if (!installed)
                {
                    installPath = "C" + gamesFolder + "\\" + installPath;
                }

                games.Add(new GameMetadata()
                {
                    Name = gameName,
                    GameId = gameId,
                    Platforms = new HashSet<MetadataProperty> { new MetadataSpecProperty("pc_windows") },
                    InstallDirectory = installPath,
                    GameActions = new List<GameAction>
                    {
                        new GameAction()
                        {
                            Name = gameName,
                            IsPlayAction = true,
                            Type = GameActionType.File,
                            TrackingMode = TrackingMode.Default,
                            Path = execPath,
                            WorkingDir = "{InstallDir}",
                            Arguments = arguments
                        }
                    },
                    IsInstalled = installed
                });
            }
            sqlreader.Close();
            sqlite_conn.Close();
            return games;
        }

        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args)
        {
            if (args.Game.PluginId != Id)
            {
                yield break;
            }

            yield return new LocalInstallController(args.Game, this);
        }

        public override IEnumerable<UninstallController> GetUninstallActions(GetUninstallActionsArgs args)
        {
            if (args.Game.PluginId != Id)
            {
                yield break;
            }

            yield return new LocalUninstallController(args.Game, this);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new LocalLibrarySettingsView();
        }
    }
}