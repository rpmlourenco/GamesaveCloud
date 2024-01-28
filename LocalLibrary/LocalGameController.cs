using Playnite;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
//using IGDBMetadata;

namespace LocalLibrary
{

    public class LocalPlayController : PlayController
    {
        private LocalLibrary pluginInstance;

        public LocalPlayController(Game game, LocalLibrary instance) : base(game)
        {
            pluginInstance = instance;
        }

        public override void Play(PlayActionArgs args)
        {
            pluginInstance.PlayniteApi.Dialogs.ShowMessage($"Not implemented (igdb)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    public class LocalInstallController : InstallController
    {
        private CancellationTokenSource watcherToken;
        private LocalLibrary pluginInstance;

        public LocalInstallController(Game game, LocalLibrary instance) : base(game)
        {
            Name = "Install using InstallButton Plugin";
            pluginInstance = instance;
        }

        public override void Dispose()
        {
            watcherToken?.Cancel();
        }

        public override void Install(InstallActionArgs args)
        {
            if (GameInstaller(Game)) InvokeOnInstalled(new GameInstalledEventArgs());
        }

        public bool GameInstaller(Game game)
        {

            //var client = new IgdbClient("https://api2.playnite.link/api/");
            //var result = Task.Run(() => client.GetGame(185246)).Result;
            pluginInstance.PlayniteApi.Dialogs.ShowMessage($"Not Implemented", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return true;
        }


        public async void StartInstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            await Task.Run(async () =>
            {
                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (Game.InstallDirectory == null)
                    {
                        await Task.Delay(10000);
                        continue;
                    }
                    else
                    {
                        var installInfo = new GameInstallationData()
                        {
                            InstallDirectory = Game.InstallDirectory
                        };

                        InvokeOnInstalled(new GameInstalledEventArgs(installInfo));
                        return;
                    }
                }
            });
        }
    }

    public class LocalUninstallController : UninstallController
    {

        private CancellationTokenSource watcherToken;
        private LocalLibrary pluginInstance;

        public LocalUninstallController(Game game, LocalLibrary instance) : base(game)
        {
            Name = "Uninstall using InstallButton Plugin";
            pluginInstance = instance;
        }

        public override void Dispose()
        {
            watcherToken?.Cancel();
        }

        public override void Uninstall(UninstallActionArgs args)
        {
            GameUninstaller(Game);
            InvokeOnUninstalled(new GameUninstalledEventArgs());
            //StartUninstallWatcher();
        }

        public void GameUninstaller(Game game)
        {
            pluginInstance.PlayniteApi.Dialogs.ShowMessage($"Not implemented (igdb)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public async void StartUninstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            await Task.Run(async () =>
            {
                while (true)
                {
                    if (watcherToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (Game.InstallDirectory != null)
                    {
                        await Task.Delay(10000);
                        continue;
                    }
                    else
                    {
                        InvokeOnUninstalled(new GameUninstalledEventArgs());
                        return;
                    }
                }
            });
        }
    }
} 