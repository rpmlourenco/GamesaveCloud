Imports Google.Apis.Drive.v3
Imports System.Data
Imports System.Data.SQLite
Imports System.IO
Imports System.IO.Compression
Imports System.Reflection

Public Class Synchronizer

    Dim driveHelper As GoolgeDriveHelper

    Dim gamesaveRootFolder As Data.File
    Dim configFolder As Data.File
    Dim backupFolder As String
    Dim pathConfigFile As String
    Public version As String = "0.84"
    Dim logFile As StreamWriter

    ' configuration stored in sqlite
    Dim syncInterval As Integer
    Dim performBackup As Boolean

    Public Class Args
        Public command As String
        Public game_id As String
    End Class

    Public Sub Initialize()

        Dim pathAtual = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)

        logFile = New StreamWriter(Path.Combine(pathAtual, "GamesaveCloud.log"), True)

        Dim myFile As New FileInfo(Path.Combine(pathAtual, "GamesaveCloud.log"))
        If myFile.Length > 0 Then
            logFile.WriteLine()
        End If

        Dim startTime = DateTime.Now
        Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Connecting to Google Drive... ")

        driveHelper = New GoolgeDriveHelper(Assembly.GetExecutingAssembly().GetManifestResourceStream("GamesaveCloudCLI.drive_api.json"))

        gamesaveRootFolder = driveHelper.GetFolder("root", "GamesaveCloud")
        If gamesaveRootFolder Is Nothing Then
            gamesaveRootFolder = driveHelper.NewFolder("GamesaveCloud", "root")
        End If

        configFolder = driveHelper.GetFolder(gamesaveRootFolder.Id, "config")
        If configFolder Is Nothing Then
            configFolder = driveHelper.NewFolder("config", gamesaveRootFolder.Id)
        End If


        Dim pathConfigFolder = Path.Combine(pathAtual, "config")
        If Not System.IO.Directory.Exists(pathConfigFolder) Then
            Directory.CreateDirectory(pathConfigFolder)
        End If

        SyncPath(pathConfigFolder, configFolder.Id, False, "config")

        pathConfigFile = Path.Combine(pathConfigFolder, "GamesaveDB.db")

        If Not System.IO.File.Exists(pathConfigFile) Then
            Dim sReader As Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GamesaveCloudCLI.GamesaveDB.db")
            Dim sWriter As Stream = New FileStream(pathConfigFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)
            BinaryStreamCopy(sReader, sWriter)
            sWriter.Flush()
            sWriter.Close()
            sReader.Close()

            SyncPath(pathConfigFolder, configFolder.Id, False, "config")
        End If

        backupFolder = Path.Combine(pathAtual, "backup")
        If Not System.IO.Directory.Exists(backupFolder) Then
            Directory.CreateDirectory(backupFolder)
        End If

        Dim endTime = DateTime.Now
        Dim secs As Double = endTime.Subtract(startTime).TotalSeconds

        Log($"connected in {secs.ToString("N2")} seconds" + Environment.NewLine)

        Dim sqlite_conn As SQLiteConnection
        Dim sqlite_cmd As SQLiteCommand
        Dim sqlite_datareader As SQLiteDataReader

        sqlite_conn = New SQLiteConnection("Data Source=" + pathConfigFile + ";Version=3;New=True;")
        sqlite_conn.Open()

        sqlite_cmd = sqlite_conn.CreateCommand()

        sqlite_cmd.CommandText = "select value from parameter where parameter_id = 'SYNC_INTERVAL'"
        sqlite_datareader = sqlite_cmd.ExecuteReader()
        If sqlite_datareader.Read() Then
            syncInterval = Convert.ToInt32(sqlite_datareader.GetString(0))
        Else
            syncInterval = 60000
        End If
        sqlite_datareader.Close()

        sqlite_cmd.CommandText = "select value from parameter where parameter_id = 'BACKUP'"
        sqlite_datareader = sqlite_cmd.ExecuteReader()
        If sqlite_datareader.Read() Then
            performBackup = Convert.ToInt32(sqlite_datareader.GetString(0)) = 1
        Else
            performBackup = True
        End If

        sqlite_conn.Close()

    End Sub

    Public Sub Sync(pgame_id As Integer)

        Dim sqlite_conn As SQLiteConnection
        Dim sqlite_cmd As SQLiteCommand
        Dim sqlite_datareader As SQLiteDataReader

        Dim startTime = DateTime.Now
        Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + $": *** Synchronizing... " + Environment.NewLine)

        sqlite_conn = New SQLiteConnection("Data Source=" + pathConfigFile + ";Version=3;New=True;")
        sqlite_conn.Open()

        'sqlite_cmd = sqlite_conn.CreateCommand()
        'sqlite_cmd.CommandText = "INSERT INTO people (first_name, last_name) VALUES ('Rui','Martins');"
        'sqlite_cmd.ExecuteNonQuery()

        sqlite_cmd = sqlite_conn.CreateCommand()

        If pgame_id = Nothing Then
            sqlite_cmd.CommandText = "select game_id, title from game where active = 1"
            sqlite_datareader = sqlite_cmd.ExecuteReader()
        Else
            sqlite_cmd.CommandText = "select game_id, title from game where active = 1 and game_id = @gameid"
            sqlite_cmd.Parameters.Add("@gameid", SqlDbType.Int).Value = pgame_id
            sqlite_datareader = sqlite_cmd.ExecuteReader()
        End If

        While (sqlite_datareader.Read()) ' Read() returns true if there is still a result line to read

            Dim game_id As Object
            Dim title As String
            Dim syncState As Integer = 0

            game_id = sqlite_datareader.GetValue(0)
            title = sqlite_datareader.GetString(1)

            Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + title + " ")

            Dim sqlite_cmd2 As SQLiteCommand
            Dim sqlite_datareader2 As SQLiteDataReader

            sqlite_cmd2 = sqlite_conn.CreateCommand()
            sqlite_cmd2.CommandText = "select savegame_id, path, machine from savegame where game_id = @gameid"
            sqlite_cmd2.Parameters.Add("@gameid", SqlDbType.Int).Value = game_id
            sqlite_datareader2 = sqlite_cmd2.ExecuteReader()

            While (sqlite_datareader2.Read())

                Dim savegame_id As Integer
                Dim path As String
                Dim machine As Integer

                savegame_id = sqlite_datareader2.GetValue(0)
                path = sqlite_datareader2.GetString(1)
                path = ReplaceEnvironmentVariables(path)
                machine = sqlite_datareader2.GetValue(2)

                Dim gameFolder As Data.File = driveHelper.GetFolder(gamesaveRootFolder.Id, game_id.ToString)
                Dim gamesaveFolder As Data.File = Nothing

                If gameFolder IsNot Nothing Then
                    gamesaveFolder = driveHelper.GetFolder(gameFolder.Id, savegame_id.ToString)
                    If gamesaveFolder IsNot Nothing Then
                        If Not Directory.Exists(path) Then
                            Directory.CreateDirectory(path)
                        End If
                    Else
                        If Directory.Exists(path) Then
                            gamesaveFolder = driveHelper.NewFolder(savegame_id.ToString, gameFolder.Id)
                        End If
                    End If
                Else
                    If Directory.Exists(path) Then
                        gameFolder = driveHelper.NewFolder(game_id.ToString, gamesaveRootFolder.Id)
                        gamesaveFolder = driveHelper.NewFolder(savegame_id.ToString, gameFolder.Id)
                    End If
                End If

                If gamesaveFolder IsNot Nothing Then

                    Dim syncResult = SyncPath(path, gamesaveFolder.Id, performBackup, game_id.ToString() + "_" + savegame_id.ToString() + "_" + DateTime.Now.ToString("ddMMyyyy_HHmmss"))
                    If syncResult = -1 Then
                        If (machine = 1) Then
                            ' update machine subfolders with the most recent when synchronizing from cloud
                            UpdateMachine(path)
                        End If
                        ' sync from cloud
                        syncState = 3
                    ElseIf syncResult = 1 Then
                        ' sync from local computer
                        syncState = 2
                    ElseIf syncResult = 0 Then
                        If syncState = 0 Then
                            ' already sync
                            syncState = 1
                        End If
                    End If

                End If

            End While
            sqlite_datareader2.Close()

            Select Case syncState
                Case 0
                    ' skipped: not in local and not in cloud and game active
                    Log("skipped" + Environment.NewLine)
                Case 1
                    Log("in sync" + Environment.NewLine)
                Case 2
                    Log("synched from local computer" + Environment.NewLine)
                Case 3
                    Log("synched from cloud" + Environment.NewLine)
            End Select

        End While
        sqlite_datareader.Close()
        sqlite_conn.Close()

        Dim endTime = DateTime.Now
        Dim secs As Double = endTime.Subtract(startTime).TotalSeconds
        Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + $": *** Synchronization completed in {secs.ToString("N2")} seconds" + Environment.NewLine)

    End Sub


    Public Sub Close()
        logFile.Close()
    End Sub


    Private Sub Log(ByVal text As String)
        Console.Write(text)
        logFile.Write(text)
    End Sub

    Private Sub BinaryStreamCopy(sReader As Stream, sWriter As Stream)

        ' Create a read buffer. The buffer length is less than or equal to the data length
        Dim dataLength = sReader.Length
        Dim bufferLength As Integer = CInt(If(dataLength >= 1024, 1024, dataLength))
        Dim buffer As Byte() = New Byte(bufferLength - 1) {}
        Dim read As Integer = 0

        While True
            read = sReader.Read(buffer, 0, buffer.Length)
            If read = 0 Then Exit While
            sWriter.Write(buffer, 0, read)
        End While

    End Sub

    ' Fully synchronizes a local and could folder
    ' Result: 0 no synch needed, 1 synchronizes from local, -1 synchronizes from cloud
    Private Function SyncPath(folderPath As String, folderId As String, backup As Boolean, backupName As String) As Integer

        Dim syncResult As Integer = 0
        Dim totalFilesDrive As Integer = 0
        Dim lastModifiedDrive As Date = driveHelper.LastModifiedDate(folderId, totalFilesDrive)
        Dim totalFilesLocal As Integer = 0
        Dim lastModifiedLocal As Date = driveHelper.LocalLastModifiedDate(folderPath, totalFilesLocal)

        If lastModifiedDrive <> Nothing Then
            If lastModifiedLocal <> Nothing Then
                If lastModifiedDrive.ToString("yyyyMMdd HHmmss") <> lastModifiedLocal.ToString("yyyyMMdd HHmmss") Or totalFilesDrive <> totalFilesLocal Then
                    If lastModifiedDrive > lastModifiedLocal Then
                        If backup Then
                            ZipFile.CreateFromDirectory(folderPath, Path.Combine(backupFolder, backupName) + ".zip", CompressionLevel.Optimal, True)
                        End If
                        driveHelper.SyncFromDrive(folderPath, folderId)
                        syncResult = -1
                    Else
                        driveHelper.SyncFromLocal(folderPath, folderId)
                        syncResult = 1
                    End If
                End If
            Else
                If backup Then
                    ZipFile.CreateFromDirectory(folderPath, Path.Combine(backupFolder, backupName) + ".zip", CompressionLevel.Optimal, True)
                End If
                driveHelper.SyncFromDrive(folderPath, folderId)
                syncResult = -1
            End If
        Else
            If lastModifiedLocal <> Nothing Then
                driveHelper.SyncFromLocal(folderPath, folderId)
                syncResult = 1
            End If
        End If

        Return syncResult

    End Function

    Private Function ReplaceEnvironmentVariables(sPath As String) As String

        Dim pos1 = sPath.IndexOf("%")
        If pos1 = -1 Then
            Return sPath
        End If
        Dim pos2 = sPath.IndexOf("%", pos1 + 1)
        If pos2 = -1 Then
            Return sPath
        End If

        Dim variable = sPath.Substring(pos1, pos2 - pos1 + 1)
        Return sPath.Replace(variable, Environment.GetEnvironmentVariable(variable.Replace("%", "")))

    End Function

    Private Sub UpdateMachine(folderPath As String)

        Dim lastModified As Date = Nothing
        Dim lastModifiedPath As String = Nothing
        Dim folderEntries As String() = Directory.GetDirectories(folderPath)
        For Each folderEntry In folderEntries
            Dim totalFiles As Integer = 0
            Dim folderLastModified As Date = driveHelper.LocalLastModifiedDate(folderEntry, totalFiles)
            If lastModified = Nothing Or folderLastModified > lastModified Then
                lastModified = folderLastModified
                lastModifiedPath = folderEntry
            End If
        Next

        For Each folderEntry In folderEntries
            If folderEntry <> lastModifiedPath Then
                Directory.Delete(folderEntry, True)
                Directory.CreateDirectory(folderEntry)
                FileIO.FileSystem.CopyDirectory(lastModifiedPath, folderEntry, True)
            End If
        Next

    End Sub


End Class
