﻿using GamesaveCloudLib;
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
            bool init = false;
            string syncDirection = null;
            bool async = true;
            bool playnite = false;
            try
            {
                var options = new OptionSet() {
                    { "i|id=", "The {ID} of the game to be synchronized (0 to synchronize all games in the database).", v => {if (int.Parse(v) >= 0) gameId = int.Parse(v);} },
                    { "t|title=", "The {TITLE} of the game to be synchronized (see https://www.igdb.com/).", v => gameTitle = v },
                    { "s|service=", "The {SERVICE} to use. valid options are: " + $"{String.Join(", ",Synchronizer.CloudServices)}", v => cloudService = v },
                    { "d|direction=", "The {DIRECTION} of the synchronization (auto, tocloud, fromcloud).", v => syncDirection = v },
                    { "a|async=", "Asynchronous operation - downloads/uploads in parallel (yes/no). Default is yes.", v => async = !(v.ToLower() == "no" || v.ToLower() == "n") },
                    { "p|playnite=", "Launch Playnite Fullscreen App after sync (yes/no). Default is no.", v => playnite = !(v.ToLower() == "no" || v.ToLower() == "n") },
                    { "r|refresh", "Refresh database only.", v => init = v != null },
                    { "h|?|help", "Show help.", v => help = v != null }
                };
                List<string> extra;
                extra = options.Parse(args);

                if (extra != null && extra.Count > 0)
                {
                    foreach (var item in extra)
                        Console.WriteLine("Unrecognized option: {0}", item);
                    help = true;
                }

                if (gameId == null && gameTitle == null && !init)
                {
                    Console.WriteLine("Either the game id or title have to be provided.");
                    help = true;
                }

                // validate cloud service
                if (cloudService != null)
                {
                    if (Synchronizer.CloudServices.Contains(cloudService.ToLower()))
                    {
                        cloudService = cloudService.ToLower();
                    }
                    else
                    {
                        Console.WriteLine("Invalid cloud service.");
                        help = true;
                    }
                }

                // validate direction
                if (syncDirection == null)
                {
                    syncDirection = "auto";
                }
                else
                {
                    if (Synchronizer.SyncDirections.Contains(syncDirection.ToLower()))
                    {
                        syncDirection = syncDirection.ToLower();
                    }
                    else
                    {
                        Console.WriteLine("Invalid direction.");
                        help = true;
                    }
                }

                if (help)
                {
                    ShowHelp(options);
                    return;
                }

                LaunchSynchronizer(init, gameId, gameTitle, cloudService, syncDirection, async, playnite);
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

        static void LaunchSynchronizer(bool init, int? gameId, string gameName, string cloudService, string syncDirection, bool async = true, bool playnite = false)
        {
            /*
            Logger logger = new();
            var progress = new Progress<string>(msg =>
            {
                Console.Write(msg);
                logger.Log(msg);
            });
            */
            var sync = new Synchronizer(null);
            try
            {
                sync.Initialize(cloudService);

                if (!init)
                {
                    sync.Sync(gameId, gameName, syncDirection, async);
                }

                if (playnite)
                {
                    System.Diagnostics.Process.Start("C:\\Portable Apps\\Playnite\\Playnite.FullscreenApp.exe");
                }

            }
            catch (Exception ex)
            {
                sync.Log(ex.ToString());
            }
            sync.Close();
            //logger.Close();
        }

        public static void UnitTests()
        {
            Synchronizer.UpdateMachine("C:\\Users\\rpmlo\\Desktop\\teste\\users", true, "", "sharedsettings.cfg");
            /*
            IniFile ini = new IniFile();
            ini.Write("DefaultCloudService","onedrive");
            var defaultCloudService = ini.Read("DefaultCloudService");
            Console.WriteLine(defaultCloudService);
            /*
            var progress = new Progress<string>(msg =>
            {
                Console.Write(msg);
            });

            var helper = new OneDriveHelper(progress);
            */

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
            /*
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
            */

            //NewFolder PASS
            //var newfolder = helper.NewFolder("my new folder", folder.Id);
            //Console.WriteLine("NewFolder " + newfolder.Name);

        }
    }
}