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
using System.Threading.Tasks;

namespace GamesaveCloudLib;

public class Synchronizer
{
    public static readonly List<string> CloudServices = new() { "googledrive", "onedrive" };
    public static readonly string fallbackCloudService = "onedrive";
    public static readonly string fallbackGamesPath = @"C:\Games";
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
    private readonly Logger logger;
    public string workingPath;

    public List<KeyValuePair<string, string>> variables;

    [SupportedOSPlatform("windows")]
    public Synchronizer(IProgress<string> progress, string workingPath = null)
    {
        if (workingPath == null || !Directory.Exists(workingPath))
        {
            this.workingPath = Path.GetDirectoryName(Environment.ProcessPath);
        }
        else
        {
            this.workingPath = workingPath;
        }

        if (progress == null)
        {
            logger = new(this.workingPath);
        }
        else
        {
            this.progress = progress;
        }

        IniFile ini = new(this.workingPath);
        variables = ini.GetSectionKeys("Variables");
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
                IniFile iniFile = new(workingPath);
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

        if (GetGamesPath() == null)
        {
            IniFile iniFile = new(workingPath);
            iniFile.Write("games", fallbackGamesPath, "Variables");
            Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Default games path set as " + fallbackGamesPath + "." + Environment.NewLine);
        }

        //var pathAtual = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //var pathAtual = Path.GetDirectoryName(System.AppContext.BaseDirectory);
        //var pathAtual = Path.GetDirectoryName(workingPath);
        var pathAtual = workingPath;

        var startTime = DateTime.Now;
        Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Connecting to " + this.cloudService + "... " + Environment.NewLine);

        driveHelper = this.cloudService switch
        {
            "googledrive" => new GoolgeDriveHelper(workingPath),
            "onedrive" => new OneDriveHelper(workingPath, progress, clientApp, handle),
            _ => new OneDriveHelper(workingPath, progress, clientApp, handle),
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

        //IniFile ini = new(workingPath);
        //var keys = ini.GetSectionKeys("MySection");
        //_ = SyncPath(pathConfigFolder, configFolder.Id, false, "config", false, false, ini.SFilename, null, "auto", true);

        pathDatabaseFile = Path.Combine(pathConfigFolder, databaseFile);
        if (syncDatabase)
        {
            var dbSync = SyncPath(pathConfigFolder, configFolder.Id, false, "config", false, false, databaseFile, null, "auto", true);
            if (!File.Exists(pathDatabaseFile))
            {
                var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(str => str.EndsWith(databaseFile));
                var sReader = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                Stream sWriter = new FileStream(pathDatabaseFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                BinaryStreamCopy(sReader, sWriter);
                sWriter.Flush();
                sWriter.Close();
                sReader.Close();

                SyncPath(pathConfigFolder, configFolder.Id, false, "config", false, false, databaseFile, null, "auto", true);

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

    public void Sync(int? gameId, string gameTitle, string syncDirection, bool async = true)
    {
        SQLiteConnection sqlite_conn;
        SQLiteCommand sqlite_cmd;
        SQLiteDataReader sqlite_datareader;

        var startTime = DateTime.Now;
        Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + $": *** Synchronizing... " + Environment.NewLine);

        sqlite_conn = new SQLiteConnection("Data Source=" + pathDatabaseFile + ";Version=3;New=True;");
        sqlite_conn.Open();

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

            if (game_id.ToString().Equals("900000000"))
            {
                WaitPlaynite();
            }

            Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + title + " ");

            SQLiteCommand sqlite_cmd2;
            SQLiteDataReader sqlite_datareader2;

            sqlite_cmd2 = sqlite_conn.CreateCommand();
            sqlite_cmd2.CommandText = "select savegame_id, path, machine, recursive, filter, filter_out from savegame where game_id = @gameid";
            sqlite_cmd2.Parameters.Add("@gameid", (DbType)SqlDbType.Int).Value = game_id;
            sqlite_datareader2 = sqlite_cmd2.ExecuteReader();

            while (sqlite_datareader2.Read())
            {
                int savegame_id;
                string path;
                int machine;
                int recursive;
                string filterIn = null;
                string filterOut = null;

                savegame_id = (int)(long)sqlite_datareader2.GetValue(0);
                path = sqlite_datareader2.GetString(1);
                path = ReplaceVariables(path);
                if (!Path.IsPathRooted(path))
                {
                    sqlite_datareader2.Close();
                    sqlite_datareader.Close();
                    sqlite_conn.Close();
                    throw new Exception($"Savegame path '{path}' must be fully qualified. Check your configuration.");
                }

                machine = (int)(long)sqlite_datareader2.GetValue(2);
                recursive = (int)(long)sqlite_datareader2.GetValue(3);
                try
                {
                    if (sqlite_datareader2.GetValue(4).GetType().Equals(typeof(String)))
                    {
                        filterIn = (String)sqlite_datareader2.GetString(4);
                    }
                }
                catch (Exception) { }
                try
                {
                    if (sqlite_datareader2.GetValue(5).GetType().Equals(typeof(String)))
                    {
                        filterOut = (String)sqlite_datareader2.GetString(5);
                    }
                }
                catch (Exception) { }

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

                    if (machine == 1)
                    {
                        UpdateMachineBeforeSync(path, gamesaveFolder.Id, recursive == 1, filterIn, filterOut);
                    }

                    var syncResult = SyncPath(path, gamesaveFolder.Id, performBackup, game_id.ToString() + "_" + savegame_id.ToString() + "_" + DateTime.Now.ToString("ddMMyyyy_HHmmss"), recursive == 1, machine == 1, filterIn, filterOut, syncDirection, async);

                    if (syncResult == -1)
                    {
                        if (machine == 1)
                        {
                            // update machine subfolders with the most recent when synchronizing from cloudService
                            UpdateMachine(path, recursive == 1, filterIn, filterOut);
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

    public async Task<bool> DeleteCloudFiles(int gameId)
    {
        SQLiteConnection sqlite_conn;
        SQLiteCommand sqlite_cmd;
        SQLiteDataReader sqlite_datareader;

        Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + $": *** Deleting cloud files... " + Environment.NewLine);

        sqlite_conn = new SQLiteConnection("Data Source=" + pathDatabaseFile + ";Version=3;New=True;");
        sqlite_conn.Open();

        sqlite_cmd = sqlite_conn.CreateCommand();

        if (gameId != default)
        {
            sqlite_cmd.CommandText = "select game_id, title from game where active = 1 and game_id = @gameid";
            sqlite_cmd.Parameters.AddWithValue("@gameid", gameId);
            sqlite_datareader = sqlite_cmd.ExecuteReader();
        }
        else
        {
            Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + $"Game with id {gameId} not found" + Environment.NewLine);
            return false;
        }

        if (sqlite_datareader.Read())
        {
            string title;
            title = sqlite_datareader.GetString(1);

            Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + title + " ");

            var gameFolder = driveHelper.GetFolder(gamesaveRootFolder.Id, gameId.ToString());
            if (gameFolder is not null)
            {
                await driveHelper.DeleteFile(gameFolder.Id);
                Log("deleted from cloud" + Environment.NewLine);
            }
            else
            {
                Log("does not exist in cloud" + Environment.NewLine);
            }
        }
        sqlite_datareader.Close();
        sqlite_conn.Close();
        return true;
    }

    public bool DeleteLocalFiles(int gameId)
    {
        SQLiteConnection sqlite_conn;
        SQLiteCommand sqlite_cmd;
        SQLiteDataReader sqlite_datareader;

        Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + $": *** Deleting local files... " + Environment.NewLine);

        sqlite_conn = new SQLiteConnection("Data Source=" + pathDatabaseFile + ";Version=3;New=True;");
        sqlite_conn.Open();

        sqlite_cmd = sqlite_conn.CreateCommand();

        if (gameId != default)
        {
            sqlite_cmd.CommandText = "select game_id, title from game where active = 1 and game_id = @gameid";
            sqlite_cmd.Parameters.AddWithValue("@gameid", gameId);
            sqlite_datareader = sqlite_cmd.ExecuteReader();
        }
        else
        {
            Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + $"Game with id {gameId} not found" + Environment.NewLine);
            return false;
        }

        if (sqlite_datareader.Read())
        {
            string title;
            title = sqlite_datareader.GetString(1);

            Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + title + " ");

            SQLiteCommand sqlite_cmd2 = sqlite_conn.CreateCommand();
            sqlite_cmd2.CommandText = "select savegame_id, path, machine, recursive, filter, filter_out from savegame where game_id = @gameid";
            sqlite_cmd2.Parameters.Add("@gameid", (DbType)SqlDbType.Int).Value = gameId;
            SQLiteDataReader sqlite_datareader2 = sqlite_cmd2.ExecuteReader();

            while (sqlite_datareader2.Read())
            {
                //int savegame_id;
                string path;
                int machine;
                int recursive;
                string filterIn = null;
                string filterOut = null;

                //savegame_id = (int)(long)sqlite_datareader2.GetValue(0);
                path = sqlite_datareader2.GetString(1);
                path = ReplaceVariables(path);
                machine = (int)(long)sqlite_datareader2.GetValue(2);
                recursive = (int)(long)sqlite_datareader2.GetValue(3);
                try
                {
                    if (sqlite_datareader2.GetValue(4).GetType().Equals(typeof(String)))
                    {
                        filterIn = (String)sqlite_datareader2.GetString(4);
                    }
                }
                catch (Exception) { }
                try
                {
                    if (sqlite_datareader2.GetValue(5).GetType().Equals(typeof(String)))
                    {
                        filterOut = (String)sqlite_datareader2.GetString(5);
                    }
                }
                catch (Exception) { }

                if (DeleteDirectory(path, RecursiveLevel(recursive == 1, machine == 1), filterIn, filterOut))
                {
                    Log("deleted locally" + Environment.NewLine);
                }
                else
                {
                    Log("does not exist locally" + Environment.NewLine);
                }

            }
        }
        sqlite_datareader.Close();
        sqlite_conn.Close();
        return true;
    }

    // machine recursive starts at 2nd level
    public static int RecursiveLevel(bool recursive, bool machine)
    {
        if (!recursive)
        {
            if (!machine)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
        else
        {
            return 0;
        }
    }

    public void Close()
    {
        var sessionEndTime = DateTime.Now;
        var secs = sessionEndTime.Subtract(sessionStartTime).TotalSeconds;
        Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + $": Total session time {secs:N2} seconds" + Environment.NewLine);
        Log(Environment.NewLine);

        //logFile?.Close();
        logger?.Close();

    }

    public void Log(string text)
    {
        if (progress != null)
        {
            progress.Report(text);
        }
        else
        {
            Console.Write(text);
            //logFile?.Write(text);
            logger.Log(text);
        }

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

    private void SyncPathFromLocal(string folderPath, string folderId, bool recursive, bool machine, string filterIn, string filterOut, bool async)
    {
        if (async)
        {
            driveHelper.SyncFromLocalAsync(folderPath, folderId, RecursiveLevel(recursive, machine), filterIn, filterOut).Wait();
        }
        else
        {
            driveHelper.SyncFromLocal(folderPath, folderId, RecursiveLevel(recursive, machine), filterIn, filterOut);
        }
    }

    private void SyncPathFromDrive(string folderPath, string folderId, bool backup, string backupName, bool recursive, bool machine, string filterIn, string filterOut, bool async)
    {
        if (backup)
        {
            ZipFile.CreateFromDirectory(folderPath, Path.Combine(backupFolder, backupName) + ".zip", CompressionLevel.Optimal, true);
        }
        if (async)
        {
            driveHelper.SyncFromDriveAsync(folderPath, folderId, RecursiveLevel(recursive, machine), filterIn, filterOut).Wait();
        }
        else
        {
            driveHelper.SyncFromDrive(folderPath, folderId, RecursiveLevel(recursive, machine), filterIn, filterOut);
        }
    }


    // Fully synchronizes a local and could folder
    // Result: 0 no synch needed, 1 synchronizes from local, -1 synchronizes from cloudService
    private int SyncPath(string folderPath, string folderId, bool backup, string backupName, bool recursive, bool machine, string filterIn, string filterOut, string syncDirection, bool async)
    {
        var syncResult = 0;

        switch (syncDirection)
        {
            case "tocloud":
                SyncPathFromLocal(folderPath, folderId, recursive, machine, filterIn, filterOut, async);
                syncResult = 1;
                break;
            case "fromcloud":
                SyncPathFromDrive(folderPath, folderId, backup, backupName, recursive, machine, filterIn, filterOut, async);
                syncResult = -1;
                break;
            default:
                //var totalFilesDrive = 0;
                //var lastModifiedDrive = driveHelper.LastModifiedDate(folderId, ref totalFilesDrive, RecursiveLevel(recursive, machine), filterIn, filterOut);
                var task = driveHelper.LastModifiedDateAsync(folderId, RecursiveLevel(recursive, machine), filterIn, filterOut);
                task.Wait();
                var CloudFolderInfo = task.Result;
                var lastModifiedDrive = CloudFolderInfo.LastModified;
                var totalFilesDrive = CloudFolderInfo.TotalFiles;

                var totalFilesLocal = 0;
                var lastModifiedLocal = ICloudDriveHelper.LocalLastModifiedDate(folderPath, ref totalFilesLocal, RecursiveLevel(recursive, machine), filterIn, filterOut);

                if (lastModifiedDrive != default)
                {
                    if (lastModifiedLocal != default)
                    {
                        if (lastModifiedDrive.ToString("yyyyMMdd HHmmss") != lastModifiedLocal.ToString("yyyyMMdd HHmmss") || totalFilesDrive != totalFilesLocal)
                        {
                            if (lastModifiedDrive > lastModifiedLocal)
                            {
                                SyncPathFromDrive(folderPath, folderId, backup, backupName, recursive, machine, filterIn, filterOut, async);
                                syncResult = -1;
                            }
                            else
                            {
                                SyncPathFromLocal(folderPath, folderId, recursive, machine, filterIn, filterOut, async);
                                syncResult = 1;
                            }
                        }
                    }
                    else
                    {
                        SyncPathFromDrive(folderPath, folderId, backup, backupName, recursive, machine, filterIn, filterOut, async);
                        syncResult = -1;
                    }
                }
                else if (lastModifiedLocal != default)
                {
                    SyncPathFromLocal(folderPath, folderId, recursive, machine, filterIn, filterOut, async);
                    syncResult = 1;
                }
                break;
        }

        return syncResult;

    }

    public string ReplaceVariables(string sPath)
    {
        var result = ReplaceEnvironmentVariables(sPath);
        return ReplaceUserVariables(result);
    }

    public string UnreplaceVariables(string sPath)
    {
        var result = UnreplaceEnvironmentVariables(sPath);
        return UnreplaceUserVariables(result);
    }

    // replace environment variables
    private string ReplaceEnvironmentVariables(string sPath)
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

    private string UnreplaceEnvironmentVariables(string sPath)
    {
        string[] enVariables = { "PROGRAMDATA", "COMMONPROGRAMFILES", "COMMONPROGRAMFILES(x86)", "PROGRAMFILES", "PROGRAMFILES(X86)", "LOCALAPPDATA", "APPDATA", "USERPROFILE", "PUBLIC" };

        foreach (string variable in enVariables)
        {
            var variableValue = Environment.GetEnvironmentVariable(variable);
            if (!String.IsNullOrEmpty(variableValue))
            {
                var pos = sPath.IndexOf(variableValue);
                if (pos != -1)
                {
                    return sPath.Replace(variableValue, "%" + variable + "%");
                }
            }
        }

        return sPath;
    }


    // replace user-defined variables
    private string ReplaceUserVariables(string sPath)
    {
        var result = sPath;

        foreach (var userVariable in variables)
        {
            if (result.Contains($"{{{userVariable.Key}}}"))
            {
                result = result.Replace($"{{{userVariable.Key}}}", $"{userVariable.Value}");
            }
        }
        return result;
    }

    // un-replace user-defined variables
    private string UnreplaceUserVariables(string sPath)
    {
        var result = sPath;

        foreach (var userVariable in variables)
        {
            if (result.Contains($"{userVariable.Value}"))
            {
                result = result.Replace($"{userVariable.Value}", $"{{{userVariable.Key}}}");
            }
        }
        return result;
    }


    public void UpdateMachineBeforeSync(string folderPath, string folderId, bool recursive, string filterIn, string filterOut)
    {

        DateTime lastModified = default;
        string lastModifiedPath = null;

        var localFolders = Directory.GetDirectories(folderPath);
        for (int i = 0; i < localFolders.Length; i++)
        {
            localFolders[i] = Path.GetFileName(localFolders[i]);
        }
        localFolders = ICloudDriveHelper.FilterFiles(filterIn, filterOut, localFolders);

        if (localFolders.Length > 1)
        {
            List<String> newFolders = new List<String>();
            IList<ICloudFile> cloudFolders = driveHelper.GetFolders(folderId);
            cloudFolders = ICloudDriveHelper.FilterFiles(filterIn, filterOut, cloudFolders);

            if (cloudFolders != null && localFolders.Count() > cloudFolders.Count())
            {
                foreach (var localFolder in localFolders)
                {
                    if (ICloudDriveHelper.FindFile(localFolder, cloudFolders) == null)
                    {
                        newFolders.Add(localFolder);
                    }
                    else
                    {
                        var totalFiles = 0;
                        var folderLastModified = ICloudDriveHelper.LocalLastModifiedDate(folderPath + "\\" + localFolder, ref totalFiles, RecursiveLevel(recursive, true), filterIn, filterOut);
                        if (lastModified == default || folderLastModified > lastModified)
                        {
                            lastModified = folderLastModified;
                            lastModifiedPath = localFolder;
                        }
                    }
                }

                foreach (var newFolder in newFolders)
                {

                    CopyDirectory(folderPath + "\\" + lastModifiedPath, folderPath + "\\" + newFolder, true, RecursiveLevel(recursive, true), filterIn, filterOut);
                }
            }

        }

    }


    public static void UpdateMachine(string folderPath, bool recursive, string filterIn, string filterOut)
    {

        DateTime lastModified = default;
        string lastModifiedPath = null;
        var folderEntries = Directory.GetDirectories(folderPath);
        foreach (var folderEntry in folderEntries)
        {
            var totalFiles = 0;
            var folderLastModified = ICloudDriveHelper.LocalLastModifiedDate(folderEntry, ref totalFiles, RecursiveLevel(recursive, true), filterIn, filterOut);
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
                //Directory.Delete(folderEntry, true);
                //Directory.CreateDirectory(folderEntry);
                //Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(lastModifiedPath, folderEntry, true);
                CopyDirectory(lastModifiedPath, folderEntry, true, RecursiveLevel(recursive, true), filterIn, filterOut);
            }
        }
    }

    private static void CopyDirectory(string sourcePath, string targetPath, bool overwrite, int recursive, string filterIn, string filterOut, int level = 1)
    {
        foreach (string filePath in Directory.GetFiles(sourcePath))
        {
            if (ICloudDriveHelper.IsMatch(Path.GetFileName(filePath), filterIn, filterOut))
            {
                File.Copy(filePath, filePath.Replace(sourcePath, targetPath), overwrite);
            }
        }

        if (recursive == 0 || level < recursive)
        {
            foreach (string dirPath in Directory.GetDirectories(sourcePath))
            {
                if (!Directory.Exists(dirPath.Replace(sourcePath, targetPath)))
                {
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                }
                CopyDirectory(dirPath, dirPath.Replace(sourcePath, targetPath), overwrite, recursive, filterIn, filterOut, level + 1);
            }

        }
    }

    private static bool DeleteDirectory(string targetPath, int recursive, string filterIn, string filterOut, int level = 1)
    {
        if (Directory.Exists(targetPath))
        {
            foreach (string filePath in Directory.GetFiles(targetPath))
            {
                if (ICloudDriveHelper.IsMatch(Path.GetFileName(filePath), filterIn, filterOut))
                {
                    File.Delete(filePath);
                }
            }

            if (recursive == 0 || level < recursive)
            {
                foreach (string dirPath in Directory.GetDirectories(targetPath))
                {
                    DeleteDirectory(dirPath, recursive, filterIn, filterOut, level + 1);
                }
            }

            if (!Directory.EnumerateFileSystemEntries(targetPath).Any())
            {
                Directory.Delete(targetPath);
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    private void WaitPlaynite()
    {
        System.Diagnostics.Process[] pname1;
        System.Diagnostics.Process[] pname2;
        do
        {
            pname1 = System.Diagnostics.Process.GetProcessesByName("Playnite.DesktopApp");
            pname2 = System.Diagnostics.Process.GetProcessesByName("Playnite.FullscreenApp");

            if (pname1.Length != 0 || pname2.Length != 0)
            {
                Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Waiting for Playnite to terminate" + Environment.NewLine);
                System.Threading.Thread.Sleep(1000);
            }

        } while (pname1.Length != 0 || pname2.Length != 0);

    }

    public static bool IsValidCloudService(string cloudServiceName)
    {
        if (string.IsNullOrEmpty(cloudServiceName)) { return false; }
        if (CloudServices.Contains(cloudServiceName)) { return true; }
        return false;
    }

    public string GetDefaultCloudService()
    {
        IniFile ini = new(workingPath);
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

    public string GetPathDatabaseFile()
    {
        var pathConfigFolder = Path.Combine(workingPath, "config");
        return Path.Combine(pathConfigFolder, databaseFile);
    }


    public string GetGamesPath()
    {
        var gamesVar = variables.Where(v => (v.Key.Equals("games")));
        if (gamesVar.Count() > 0)
        {
            return gamesVar.First().Value;
        }
        return null;
    }

}