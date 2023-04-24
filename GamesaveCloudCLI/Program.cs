using GamesaveCloudLib;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace GamesaveCloudCLI
{
    [SupportedOSPlatform("windows")]
    static class Program
    {

        public static void Main(string[] args)
        {
            int? gameId = null;
            string gameTitle = null;
            string cloudService = null;
            bool help = false;
            string syncDirection = null;
            try
            {
                var options = new OptionSet() {
                    { "i|id=", "The {ID} of the game to be synchronized (0 to synchronize all games in the database).", v => {if (int.Parse(v) >= 0) gameId = int.Parse(v);} },
                    { "t|title=", "The {TITLE} of the game to be synchronized (see https://www.igdb.com/).", v => gameTitle = v },
                    { "s|service=", "The {SERVICE} to use. valid options are: " + $"{String.Join(", ",Synchronizer.CloudServices)}", v => {if (Synchronizer.CloudServices.Contains(v.ToLower())) cloudService = v.ToLower(); } },
                    { "d|direction=", "The {DIRECTION} of the synchronization (auto, tocloud, fromcloud).", v => {if (Synchronizer.SyncDirections.Contains(v.ToLower())) syncDirection = v.ToLower(); } },
                    { "h|?|help", v => help = v != null },
                };
                List<string> extra;
                extra = options.Parse(args);

                if (extra != null && extra.Count > 0)
                {
                    foreach (var item in extra)
                        Console.WriteLine("Unrecognized option: {0}", item);
                    help = true;
                }

                if (gameId == null && gameTitle == null)
                {
                    Console.WriteLine("Either the game id or title have to be provided.");
                    help = true;
                }

                if (help)
                {
                    ShowHelp(options);
                    return;
                }

                cloudService ??= "onedrive";

                syncDirection ??= "auto";

                LaunchSynchronizer(gameId, gameTitle, cloudService, syncDirection);
            }
            catch (Exception e)
            {
                Console.Write("GamesaveCloud: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try 'GamesaveCloud --help' for more information.");
                return;
            }

        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: GamesaveCloud OPTIONS");
            Console.WriteLine("Synchronize a gamesave in the cloud.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        static void LaunchSynchronizer(int? gameId, string gameName, string cloudService, string syncDirection)
        {
            Logger logger = new();
            var progress = new Progress<string>(msg =>
            {
                Console.Write(msg);
                logger.Log(msg);
            });
            var sync = new Synchronizer(progress);
            try
            {
                sync.Initialize(cloudService);
                sync.Sync(gameId, gameName, syncDirection);

            }
            catch (Exception ex)
            {
                sync.Log(ex.ToString());
            }
            sync.Close();
            logger.Close();
        }

        public static void UnitTests()
        {
            var progress = new Progress<string>(msg =>
            {
                Console.Write(msg);
            });

            var helper = new OneDriveHelper(progress);

            //GetFolder PASS
            //var folder = helper.GetFolder(helper._rootId, "GamesaveCloud");
            //Console.WriteLine("GetFolder " + folder.Name);

            //UploadFile PASS
            //helper.UploadFile("C:\\Users\\rpmlo\\Desktop\\Certificado INP.pdf", folder.Id, true);

            //DeleteFile PASS
            //var folder2 = helper.GetFolder(folder.Id, "my new folder");
            //helper.DeleteFile(folder2.Id);

            //GetFile PASS
            //var file = helper.GetFile(folder.Id, "Certificado INP.pdf");

            //DownloadFile PASS
            //helper.DownloadFile(file, "C:\\Users\\rpmlo\\Desktop\\Trash\\Certificado INP.pdf");
            //Console.WriteLine("DownloadFile " + file.Name + " " + file.ModifiedTime);

            //GetFiles PASS
            //var files = helper.GetFiles(folder.Id);
            //foreach ( var sfile in files)
            //{
            //    Console.WriteLine("GetFiles " + sfile.Name + " " + sfile.ModifiedTime);
            //}

            //GetFolders PASS
            var folders = helper.GetFolders("root");
            foreach (var sfolder in folders)
            {
                Console.WriteLine("GetFolders " + sfolder.Name);
            }

            var helper2 = new GoolgeDriveHelper();
            folders = helper2.GetFolders("root");
            foreach (var sfolder in folders)
            {
                Console.WriteLine("GetFolders " + sfolder.Name);
            }

            //NewFolder PASS
            //var newfolder = helper.NewFolder("my new folder", folder.Id);
            //Console.WriteLine("NewFolder " + newfolder.Name);

        }
    }
}