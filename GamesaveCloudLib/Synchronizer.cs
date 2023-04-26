using GamesaveCloudLib;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;

namespace GamesaveCloudCLI;

public class Synchronizer
{

    public static readonly List<string> CloudServices = new() { "googledrive", "onedrive" };
    public static readonly string fallbackCloudService = "onedrive";
    public static readonly List<string> SyncDirections = new() { "auto", "tocloud", "fromcloud" };

    private static readonly string databaseFile = "GamesaveDB.db";

    private ICloudDriveHelper driveHelper;
    private ICloudFile gamesaveRootFolder;
    private ICloudFile configFolder;
    private string backupFolder;
    private string pathDatabaseFile;
    public string PathDatabaseFile { get => pathDatabaseFile; }
    public string cloudService;

    private DateTime sessionStartTime;

    // configuration stored in sqlite
    private bool performBackup;
    private readonly IProgress<string> progress;

    public class Args
    {
        public string command;
        public string game_id;
    }

    [SupportedOSPlatform("windows")]
    public Synchronizer(IProgress<string> progress)
    {
        this.progress = progress;
    }

    [SupportedOSPlatform("windows")]
    public void Initialize(string cloudService = null, IPublicClientApplication clientApp = null, IntPtr? handle = null, bool syncDatabase = true)
    {
        sessionStartTime = DateTime.Now;

        if (!IsValidCloudService(cloudService))
        {
            var defaultCloudService = GetDefaultCloudService();
            if (string.IsNullOrEmpty(defaultCloudService))
            {
                IniFile iniFile = new();
                iniFile.Write("DefaultCloudService", fallbackCloudService);
                this.cloudService = fallbackCloudService;
                Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Default cloud service set as " + fallbackCloudService + "." + Environment.NewLine);
            }
            else
            {
                this.cloudService = defaultCloudService;
                Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Using default cloud service " + defaultCloudService + "." + Environment.NewLine);
            }
        }
        else
        {
            this.cloudService = cloudService;
        }

        var pathAtual = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        var startTime = DateTime.Now;
        Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Connecting to " + this.cloudService + "... " + Environment.NewLine);

        driveHelper = this.cloudService switch
        {
            "googledrive" => new GoolgeDriveHelper(),
            "onedrive" => new OneDriveHelper(progress, clientApp, handle),
            _ => new OneDriveHelper(progress, clientApp, handle),
        };

        gamesaveRootFolder = driveHelper.GetFolder("root", "GamesaveCloud");
        gamesaveRootFolder ??= driveHelper.NewFolder("GamesaveCloud", "root");

        var endTime = DateTime.Now;
        var secs = endTime.Subtract(startTime).TotalSeconds;

        Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Connected with " + driveHelper._username + $" in {secs:N2} seconds" + Environment.NewLine);

        configFolder = driveHelper.GetFolder(gamesaveRootFolder.Id, "config");
        configFolder ??= driveHelper.NewFolder("config", gamesaveRootFolder.Id);

        var pathConfigFolder = Path.Combine(pathAtual, "config");
        if (!Directory.Exists(pathConfigFolder))
        {
            Directory.CreateDirectory(pathConfigFolder);
        }

        IniFile ini = new();
        _ = SyncPath(pathConfigFolder, configFolder.Id, false, "config", false, ini.SFilename, "auto");
        pathDatabaseFile = Path.Combine(pathConfigFolder, databaseFile);

        if (syncDatabase)
        {
            var dbSync = SyncPath(pathConfigFolder, configFolder.Id, false, "config", false, databaseFile, "auto");
            if (!File.Exists(pathDatabaseFile))
            {
                var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(str => str.EndsWith(databaseFile));
                var sReader = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                Stream sWriter = new FileStream(pathDatabaseFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                BinaryStreamCopy(sReader, sWriter);
                sWriter.Flush();
                sWriter.Close();
                sReader.Close();

                SyncPath(pathConfigFolder, configFolder.Id, false, "config", false, databaseFile, "auto");

                Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Database initialized and synched to " + this.cloudService + Environment.NewLine);
            }
            else
            {
                switch (dbSync)
                {
                    case 1:
                        Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Database synched from local computer" + Environment.NewLine);
                        break;
                    case -1:
                        Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Database synched from " + this.cloudService + Environment.NewLine);
                        break;
                    default:
                        Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Database in sync" + Environment.NewLine);
                        break;
                }

            }
        }
        else
        {
            if (!File.Exists(pathDatabaseFile))
            {
                throw new Exception("Database not found.");
            }
        }

        backupFolder = Path.Combine(pathAtual, "backup");
        if (!Directory.Exists(backupFolder))
        {
            Directory.CreateDirectory(backupFolder);
        }

        SQLiteConnection sqlite_conn;
        SQLiteCommand sqlite_cmd;
        SQLiteDataReader sqlite_datareader;

        sqlite_conn = new SQLiteConnection("Data Source=" + pathDatabaseFile + ";Version=3;New=True;");
        sqlite_conn.Open();

        sqlite_cmd = sqlite_conn.CreateCommand();

        sqlite_cmd.CommandText = "select value from parameter where parameter_id = 'BACKUP'";
        sqlite_datareader = sqlite_cmd.ExecuteReader();
        if (sqlite_datareader.Read())
        {
            performBackup = Convert.ToInt32(sqlite_datareader.GetString(0)) == 1;
        }
        else
        {
            performBackup = true;
        }
        sqlite_datareader.Close();
        sqlite_conn.Close();
    }

    public void Sync(int? gameId, string gameTitle, string syncDirection)
    {
        SQLiteConnection sqlite_conn;
        SQLiteCommand sqlite_cmd;
        SQLiteDataReader sqlite_datareader;

        var startTime = DateTime.Now;
        Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + $": *** Synchronizing... " + Environment.NewLine);

        sqlite_conn = new SQLiteConnection("Data Source=" + pathDatabaseFile + ";Version=3;New=True;");
        sqlite_conn.Open();

        // sqlite_cmd = sqlite_conn.CreateCommand()
        // sqlite_cmd.CommandText = "INSERT INTO people (first_name, last_name) VALUES ('Rui','Martins');"
        // sqlite_cmd.ExecuteNonQuery()

        sqlite_cmd = sqlite_conn.CreateCommand();

        if (gameId == 0)
        {
            sqlite_cmd.CommandText = "select game_id, title from game where active = 1";
            sqlite_datareader = sqlite_cmd.ExecuteReader();
        }
        else if (gameId != null)
        {
            sqlite_cmd.CommandText = "select game_id, title from game where active = 1 and game_id = @gameid";
            sqlite_cmd.Parameters.AddWithValue("@gameid", gameId);
            sqlite_datareader = sqlite_cmd.ExecuteReader();
        }
        else if (gameTitle != null)
        {
            sqlite_cmd.CommandText = "select game_id, title from game where active = 1 and title = @title";
            sqlite_cmd.Parameters.AddWithValue("@title", gameTitle);
            sqlite_datareader = sqlite_cmd.ExecuteReader();
        }
        else
        {
            Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Either the game id or title have to be provided." + Environment.NewLine);
            sqlite_conn.Close();
            return;
        }

        var count = 0;
        while (sqlite_datareader.Read()) // Read() returns true if there is still a result line to read
        {

            object game_id;
            string title;
            var syncState = 0;

            game_id = sqlite_datareader.GetValue(0);
            title = sqlite_datareader.GetString(1);

            Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + title + " ");

            SQLiteCommand sqlite_cmd2;
            SQLiteDataReader sqlite_datareader2;

            sqlite_cmd2 = sqlite_conn.CreateCommand();
            sqlite_cmd2.CommandText = "select savegame_id, path, machine, recursive, filter from savegame where game_id = @gameid";
            sqlite_cmd2.Parameters.Add("@gameid", (DbType)SqlDbType.Int).Value = game_id;
            sqlite_datareader2 = sqlite_cmd2.ExecuteReader();

            while (sqlite_datareader2.Read())
            {
                int savegame_id;
                string path;
                int machine;
                int recursive;
                string filter = null;

                savegame_id = (int)(long)sqlite_datareader2.GetValue(0);
                path = sqlite_datareader2.GetString(1);
                path = ReplaceEnvironmentVariables(path);
                machine = (int)(long)sqlite_datareader2.GetValue(2);
                recursive = (int)(long)sqlite_datareader2.GetValue(3);
                try
                {
                    filter = sqlite_datareader2.GetString(4);
                }
                catch { }

                if (machine == 1)
                {
                    recursive = 1;
                    filter = null;
                }

                var gameFolder = driveHelper.GetFolder(gamesaveRootFolder.Id, game_id.ToString());
                ICloudFile gamesaveFolder = null;

                if (gameFolder is not null)
                {
                    gamesaveFolder = driveHelper.GetFolder(gameFolder.Id, savegame_id.ToString());
                    if (gamesaveFolder is not null)
                    {
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                    }
                    else if (Directory.Exists(path))
                    {
                        gamesaveFolder = driveHelper.NewFolder(savegame_id.ToString(), gameFolder.Id);
                    }
                }
                else if (Directory.Exists(path))
                {
                    gameFolder = driveHelper.NewFolder(game_id.ToString(), gamesaveRootFolder.Id);
                    gamesaveFolder = driveHelper.NewFolder(savegame_id.ToString(), gameFolder.Id);
                }

                if (gamesaveFolder is not null)
                {

                    var syncResult = SyncPath(path, gamesaveFolder.Id, performBackup, game_id.ToString() + "_" + savegame_id.ToString() + "_" + DateTime.Now.ToString("ddMMyyyy_HHmmss"), recursive == 1, filter, syncDirection);
                    if (syncResult == -1)
                    {
                        if (machine == 1)
                        {
                            // update machine subfolders with the most recent when synchronizing from cloudService
                            UpdateMachine(path);
                        }
                        // sync from cloudService
                        syncState = 3;
                    }
                    else if (syncResult == 1)
                    {
                        // sync from local computer
                        syncState = 2;
                    }
                    else if (syncResult == 0)
                    {
                        if (syncState == 0)
                        {
                            // already sync
                            syncState = 1;
                        }
                    }

                }

            }
            sqlite_datareader2.Close();

            switch (syncState)
            {
                case 0:
                    {
                        // skipped: not in local and not in cloudService and game active
                        Log("skipped" + Environment.NewLine);
                        break;
                    }
                case 1:
                    {
                        Log("in sync" + Environment.NewLine);
                        break;
                    }
                case 2:
                    {
                        Log("synched from local computer" + Environment.NewLine);
                        break;
                    }
                case 3:
                    {
                        Log("synched from " + cloudService + Environment.NewLine);
                        break;
                    }
            }
            count++;
        }
        sqlite_datareader.Close();
        sqlite_conn.Close();

        if (count == 0)
        {
            Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + $": No games found in the database with the specified criteria. ID={gameId} TITLE={gameTitle}" + Environment.NewLine);
            sqlite_conn.Close();
            return;
        }

        var endTime = DateTime.Now;
        var secs = endTime.Subtract(startTime).TotalSeconds;
        Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + $": *** Synchronization completed in {secs:N2} seconds" + Environment.NewLine);

    }


    public void Close()
    {
        var sessionEndTime = DateTime.Now;
        var secs = sessionEndTime.Subtract(sessionStartTime).TotalSeconds;
        Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + $": Total session time {secs:N2} seconds" + Environment.NewLine);
        Log(Environment.NewLine);

        //logFile?.Close();

    }

    public void Log(string text)
    {
        progress.Report(text);
        //Console.Write(text);
        //logFile?.Write(text);
    }

    private static void BinaryStreamCopy(Stream sReader, Stream sWriter)
    {

        // Create a read buffer. The buffer length is less than or equal to the data length
        var dataLength = sReader.Length;
        var bufferLength = (int)(dataLength >= 1024L ? 1024L : dataLength);
        var buffer = new byte[bufferLength];
        while (true)
        {
            var read = sReader.Read(buffer, 0, buffer.Length);
            if (read == 0)
            {
                break;
            }

            sWriter.Write(buffer, 0, read);
        }

    }

    // Fully synchronizes a local and could folder
    // Result: 0 no synch needed, 1 synchronizes from local, -1 synchronizes from cloudService
    private int SyncPath(string folderPath, string folderId, bool backup, string backupName, bool recursive, string filter, string syncDirection)
    {
        var syncResult = 0;

        switch (syncDirection)
        {
            case "tocloud":
                driveHelper.SyncFromLocal(folderPath, folderId, recursive, filter);
                syncResult = 1;
                break;
            case "fromcloud":
                if (backup)
                {
                    ZipFile.CreateFromDirectory(folderPath, Path.Combine(backupFolder, backupName) + ".zip", CompressionLevel.Optimal, true);
                }
                driveHelper.SyncFromDrive(folderPath, folderId, recursive, filter);
                syncResult = -1;
                break;
            default:
                var totalFilesDrive = 0;
                var lastModifiedDrive = driveHelper.LastModifiedDate(folderId, ref totalFilesDrive, recursive, filter);
                var totalFilesLocal = 0;
                var lastModifiedLocal = driveHelper.LocalLastModifiedDate(folderPath, ref totalFilesLocal, recursive, filter);

                if (lastModifiedDrive != default)
                {
                    if (lastModifiedLocal != default)
                    {
                        if (lastModifiedDrive.ToString("yyyyMMdd HHmmss") != lastModifiedLocal.ToString("yyyyMMdd HHmmss") || totalFilesDrive != totalFilesLocal)
                        {
                            if (lastModifiedDrive > lastModifiedLocal)
                            {
                                if (backup)
                                {
                                    ZipFile.CreateFromDirectory(folderPath, Path.Combine(backupFolder, backupName) + ".zip", CompressionLevel.Optimal, true);
                                }
                                driveHelper.SyncFromDrive(folderPath, folderId, recursive, filter);
                                syncResult = -1;
                            }
                            else
                            {
                                driveHelper.SyncFromLocal(folderPath, folderId, recursive, filter);
                                syncResult = 1;
                            }
                        }
                    }
                    else
                    {
                        if (backup)
                        {
                            ZipFile.CreateFromDirectory(folderPath, Path.Combine(backupFolder, backupName) + ".zip", CompressionLevel.Optimal, true);
                        }
                        driveHelper.SyncFromDrive(folderPath, folderId, recursive, filter);
                        syncResult = -1;
                    }
                }
                else if (lastModifiedLocal != default)
                {
                    driveHelper.SyncFromLocal(folderPath, folderId, recursive, filter);
                    syncResult = 1;
                }
                break;
        }

        return syncResult;

    }

    private static string ReplaceEnvironmentVariables(string sPath)
    {

        var pos1 = sPath.IndexOf("%");
        if (pos1 == -1)
        {
            return sPath;
        }
        var pos2 = sPath.IndexOf("%", pos1 + 1);
        if (pos2 == -1)
        {
            return sPath;
        }

        var variable = sPath.Substring(pos1, pos2 - pos1 + 1);
        return sPath.Replace(variable, Environment.GetEnvironmentVariable(variable.Replace("%", "")));

    }

    private void UpdateMachine(string folderPath)
    {

        DateTime lastModified = default;
        string lastModifiedPath = null;
        var folderEntries = Directory.GetDirectories(folderPath);
        foreach (var folderEntry in folderEntries)
        {
            var totalFiles = 0;
            var folderLastModified = driveHelper.LocalLastModifiedDate(folderEntry, ref totalFiles, true, null);
            if (lastModified == default || folderLastModified > lastModified)
            {
                lastModified = folderLastModified;
                lastModifiedPath = folderEntry;
            }
        }

        foreach (var folderEntry in folderEntries)
        {
            if (folderEntry != lastModifiedPath)
            {
                Directory.Delete(folderEntry, true);
                Directory.CreateDirectory(folderEntry);
                Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(lastModifiedPath, folderEntry, true);
            }
        }

    }

    public static bool IsValidCloudService(string cloudServiceName)
    {
        if (string.IsNullOrEmpty(cloudServiceName)) { return false; }
        if (CloudServices.Contains(cloudServiceName)) { return true; }
        return false;
    }

    public static string GetDefaultCloudService()
    {
        IniFile ini = new();
        var defaultCloudService = ini.Read("DefaultCloudService");

        if (IsValidCloudService(defaultCloudService))
        {
            return defaultCloudService;
        }
        else
        {
            return null;
        }

    }

    public static string GetPathDatabaseFile()
    {
        var pathAtual = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var pathConfigFolder = Path.Combine(pathAtual, "config");
        return Path.Combine(pathConfigFolder, databaseFile);
    }


}