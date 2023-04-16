Imports Google.Apis.Drive.v3
Imports System.IO
Imports System.Reflection
Imports System.Data.SQLite
Imports System.ComponentModel
Imports System.IO.Compression
Imports GamesaveCloudLib

Public Class Form1

    Dim driveHelper As GoolgeDriveHelper
    Dim watchers As New List(Of GameFileSystemWatcher)
    Dim syncQueue As New List(Of SaveGameFile)

    Dim gamesaveRootFolder As Data.File
    Dim configFolder As Data.File
    Dim backupFolder As String
    Dim pathConfigFile As String
    Dim timer As System.Timers.Timer
    Dim timerStartWatchers As System.Timers.Timer
    Public version As String = "0.84"
    Dim logFile As System.IO.StreamWriter

    ' configuration stored in sqlite
    Dim syncInterval As Integer
    Dim performBackup As Boolean

    Public Class Args
        Public command As String
        Public game_id As String
    End Class


    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        'System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = False

    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.Text = Me.Text + " " + version
        Dim clargs As String() = Environment.GetCommandLineArgs()

        Dim intGameId As Integer
        If clargs.Length = 2 AndAlso Integer.TryParse(clargs(1), intGameId) Then
            Me.WindowState = FormWindowState.Minimized
            NotifyIcon1.Visible = True
            'NotifyIcon1.ShowBalloonTip(5, "System", "DoubleClick on the Icon to restore the application.", ToolTipIcon.Info)
            Me.Hide()

            Dim args As New Args
            args.command = "sync game"
            args.game_id = clargs(1)
            Button1.Enabled = False

            BackgroundWorker1.WorkerReportsProgress = True
            BackgroundWorker1.WorkerSupportsCancellation = True
            BackgroundWorker1.RunWorkerAsync(args)
        Else
            Dim args As New Args
            args.command = "initialize"
            Button1.Enabled = False

            BackgroundWorker1.WorkerReportsProgress = True
            BackgroundWorker1.WorkerSupportsCancellation = True
            BackgroundWorker1.RunWorkerAsync(args)
        End If

    End Sub

    Private Function EnqueueChange(savegameFile As SaveGameFile) As Boolean

        Dim last = syncQueue.FindLast(Function(x) x.path = savegameFile.path And x.savegame.savegame_id = savegameFile.savegame.savegame_id)

        If (last IsNot Nothing) Then
            If last.changeType <> savegameFile.changeType Then
                syncQueue.Add(savegameFile)
                Return True
            End If
        Else
            syncQueue.Add(savegameFile)
            Return True
        End If

        Return False

    End Function
    Private Shared Sub OnChanged(sender As Object, e As FileSystemEventArgs)

        If e.ChangeType <> WatcherChangeTypes.Changed Then
            Return
        End If

        Dim form As Form1 = DirectCast(sender, GameFileSystemWatcher).form
        Dim savegame As SaveGame = DirectCast(sender, GameFileSystemWatcher).savegame
        Dim savegameFile = New SaveGameFile(savegame, e.FullPath, "", "changed")

        If form.EnqueueChange(savegameFile) Then
            form.Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + $": Changed: {savegame.title} {Path.GetFileName(e.FullPath)} " + Environment.NewLine)
        End If

    End Sub

    Private Shared Sub OnCreated(sender As Object, e As FileSystemEventArgs)
        Dim form As Form1 = DirectCast(sender, GameFileSystemWatcher).form
        Dim savegame As SaveGame = DirectCast(sender, GameFileSystemWatcher).savegame
        Dim savegameFile = New SaveGameFile(savegame, e.FullPath, "", "changed")

        If form.EnqueueChange(savegameFile) Then
            form.Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + $": Changed: {savegame.title} {Path.GetFileName(e.FullPath)} " + Environment.NewLine)
        End If
    End Sub

    Private Shared Sub OnDeleted(sender As Object, e As FileSystemEventArgs)
        Dim form As Form1 = DirectCast(sender, GameFileSystemWatcher).form
        Dim savegame As SaveGame = DirectCast(sender, GameFileSystemWatcher).savegame
        Dim savegameFile = New SaveGameFile(savegame, e.FullPath, "", "deleted")

        If form.EnqueueChange(savegameFile) Then
            form.Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + $": Deleted: {savegame.title} {Path.GetFileName(e.FullPath)} " + Environment.NewLine)
        End If
    End Sub

    Private Shared Sub OnRenamed(sender As Object, e As RenamedEventArgs)
        Dim form As Form1 = DirectCast(sender, GameFileSystemWatcher).form
        Dim savegame As SaveGame = DirectCast(sender, GameFileSystemWatcher).savegame
        Dim savegameFile = New SaveGameFile(savegame, e.FullPath, e.OldFullPath, "renamed")

        If form.EnqueueChange(savegameFile) Then
            form.Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + $": Renamed: {savegame.title} {Path.GetFileName(e.FullPath)} " + Environment.NewLine)
        End If
    End Sub

    Private Shared Sub OnError(sender As Object, e As ErrorEventArgs)
        PrintException(e.GetException())
    End Sub

    Private Shared Sub PrintException(ex As Exception)
        If ex IsNot Nothing Then
            MsgBox($"Message: {ex.Message}")
        End If
    End Sub

    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If WindowState = FormWindowState.Minimized Then
            ShowInTaskbar = False
            NotifyIcon1.Visible = True
            'NotifyIcon1.ShowBalloonTip(1000)
        End If
    End Sub

    Private Sub NotifyIcon1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseDoubleClick
        ShowInTaskbar = True
        NotifyIcon1.Visible = False
        WindowState = FormWindowState.Normal
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Button1.Enabled = False

        Dim args As New Args
        args.command = "sync"
        BackgroundWorker1.RunWorkerAsync(args)

    End Sub

    Public Sub BinaryStreamCopy(sReader As Stream, sWriter As Stream)

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

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork

        Dim worker As BackgroundWorker = CType(sender, BackgroundWorker)
        Dim args As Args = e.Argument
        Select Case args.command
            Case "initialize"
                Initialize(worker, e)
                Sync(worker, e, True)
                timerStartWatchers = New Timers.Timer(2000)
                AddHandler timerStartWatchers.Elapsed, New Timers.ElapsedEventHandler(AddressOf TimerStartWatchersElapsed)
                timerStartWatchers.Start()
                'timer = New Timers.Timer(syncInterval)
                'AddHandler timer.Elapsed, New Timers.ElapsedEventHandler(AddressOf TimerElapsed)
                'timer.Start()

            Case "sync"
                timer.Stop()
                DisableWatchers()
                Sync(worker, e, False)
                timerStartWatchers = New Timers.Timer(2000)
                AddHandler timerStartWatchers.Elapsed, New Timers.ElapsedEventHandler(AddressOf TimerStartWatchersElapsed)
                timerStartWatchers.Start()

            Case "sync game"
                Initialize(worker, e)
                Sync(worker, e, False, args.game_id)
                logFile.Close()
                Application.Exit()

            Case "sync queue"
                timer.Stop()
                ProcessSyncQueue(worker, e, args.game_id)
                timer.Start()

        End Select

        EnableSyncButton()
    End Sub

    ' This event handler updates the progress.

    Sub TimerElapsed(ByVal sender As Object, ByVal e As Timers.ElapsedEventArgs)

        If Not BackgroundWorker1.IsBusy Then
            DisableSyncButton()
            Dim args As New Args
            args.command = "sync queue"
            BackgroundWorker1.RunWorkerAsync(args)
        End If

    End Sub

    Sub TimerStartWatchersElapsed(ByVal sender As Object, ByVal e As Timers.ElapsedEventArgs)

        timerStartWatchers.Stop()
        timer = New Timers.Timer(syncInterval)
        AddHandler timer.Elapsed, New Timers.ElapsedEventHandler(AddressOf TimerElapsed)
        timer.Start()
        EnableWatchers()


    End Sub

    Private Sub Initialize(ByRef worker As BackgroundWorker, ByRef e As ComponentModel.DoWorkEventArgs)

        Dim pathAtual = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)

        logFile = My.Computer.FileSystem.OpenTextFileWriter(Path.Combine(pathAtual, "GamesaveCloud.log"), True)

        Dim myFile As New FileInfo(Path.Combine(pathAtual, "GamesaveCloud.log"))
        If myFile.Length > 0 Then
            logFile.WriteLine()
        End If

        Dim startTime = DateTime.Now
        Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": Connecting to Google Drive... ")

        driveHelper = New GoolgeDriveHelper(Assembly.GetExecutingAssembly().GetManifestResourceStream("GamesaveCloudWin.drive_api.json"))

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
            My.Computer.FileSystem.CreateDirectory(pathConfigFolder)
        End If

        SyncPath(pathConfigFolder, configFolder.Id, False, "config")

        pathConfigFile = Path.Combine(pathConfigFolder, "GamesaveDB.db")

        If Not System.IO.File.Exists(pathConfigFile) Then
            Dim sReader As Stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GamesaveCloudWin.GamesaveDB.db")
            Dim sWriter As Stream = New FileStream(pathConfigFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)
            BinaryStreamCopy(sReader, sWriter)
            sWriter.Flush()
            sWriter.Close()
            sReader.Close()

            SyncPath(pathConfigFolder, configFolder.Id, False, "config")
        End If

        backupFolder = Path.Combine(pathAtual, "backup")
        If Not System.IO.Directory.Exists(backupFolder) Then
            My.Computer.FileSystem.CreateDirectory(backupFolder)
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

    Private Sub Sync(ByRef worker As BackgroundWorker, ByRef e As ComponentModel.DoWorkEventArgs, createWatchers As Boolean, Optional pgame_id As Integer = Nothing)

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

            If (worker.CancellationPending = True) Then
                e.Cancel = True
                Exit While
            End If

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

                    If createWatchers Then

                        Dim savegame = New SaveGame(game_id, savegame_id, title, path, gamesaveFolder)
                        Dim watcher As GameFileSystemWatcher
                        watcher = New GameFileSystemWatcher(path) With {
                        .savegame = savegame,
                        .form = Me,
                        .NotifyFilter = NotifyFilters.Attributes Or
                        NotifyFilters.CreationTime Or
                        NotifyFilters.DirectoryName Or
                        NotifyFilters.FileName Or
                        NotifyFilters.LastWrite Or
                        NotifyFilters.Security Or
                        NotifyFilters.Size
                    }

                        AddHandler watcher.Changed, AddressOf OnChanged
                        AddHandler watcher.Created, AddressOf OnCreated
                        AddHandler watcher.Deleted, AddressOf OnDeleted
                        AddHandler watcher.Renamed, AddressOf OnRenamed
                        AddHandler watcher.Error, AddressOf OnError

                        watcher.Filter = ""
                        watcher.IncludeSubdirectories = True
                        watcher.EnableRaisingEvents = False

                        watchers.Add(watcher)

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

    Private Sub ProcessSyncQueue(ByRef worker As BackgroundWorker, ByRef e As ComponentModel.DoWorkEventArgs, Optional pgame_id As Integer = Nothing)

        Dim startTime = DateTime.Now
        Dim tempSyncQueue = New List(Of SaveGameFile)
        tempSyncQueue.AddRange(syncQueue)
        syncQueue.Clear()

        If tempSyncQueue.Count > 0 Then
            Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + $": *** Synchronizing {tempSyncQueue.Count} files..." + Environment.NewLine)
        End If

        For Each savegameFile As SaveGameFile In tempSyncQueue

            Dim words As String() = savegameFile.path.Replace(savegameFile.savegame.path + "\", "").Split(New Char() {"\"c})
            Dim folder As Data.File = savegameFile.savegame.driveFolder
            For i As Integer = 0 To words.Length - 2
                folder = driveHelper.GetFolder(folder.Id, words(i))
            Next

            Select Case savegameFile.changeType
                Case "changed", "created"
                    driveHelper.UploadFileInFolder(savegameFile.path, folder.Id, True)
                Case "deleted"
                    driveHelper.DeleteFileByName(Path.GetFileName(savegameFile.path), folder.Id)
                Case "renamed"
                    driveHelper.DeleteFileByName(Path.GetFileName(savegameFile.oldPath), folder.Id)
                    driveHelper.UploadFileInFolder(savegameFile.path, folder.Id, True)
            End Select

            Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + $": Synchronized: {savegameFile.savegame.title} {Path.GetFileName(savegameFile.path)} ({savegameFile.changeType})" + Environment.NewLine)
        Next

        Dim endTime = DateTime.Now
        Dim secs As Double = endTime.Subtract(startTime).TotalSeconds
        If tempSyncQueue.Count > 0 Then
            Log(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + $": *** Synchronization completed in {secs.ToString("N2")} seconds" + Environment.NewLine)
        End If

    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing

        Log(Environment.NewLine + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": +++ Exiting...")
        BackgroundWorker1.CancelAsync()
        While BackgroundWorker1.IsBusy
            Application.DoEvents()
        End While
        logFile.Close()

    End Sub

    Private Sub BackgroundWorker1_ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged
        Log(e.UserState)
    End Sub

    Private Sub Log(ByVal text As String)

        If Me.TextBox1.InvokeRequired Then
            Me.TextBox1.Invoke(New LogInvoker(AddressOf Log), text)
        Else
            Me.TextBox1.Text = TextBox1.Text + text
            Me.TextBox1.SelectionStart = TextBox1.Text.Length
            Me.TextBox1.ScrollToCaret()

            logFile.Write(text)
        End If
    End Sub

    Private Delegate Sub LogInvoker(ByVal text As String)

    Private Sub EnableSyncButton()
        If Me.Button1.InvokeRequired Then
            Me.Button1.Invoke(New MethodInvoker(AddressOf EnableSyncButton))
        Else
            Me.Button1.Enabled = True
        End If
    End Sub

    Private Sub DisableSyncButton()
        If Me.Button1.InvokeRequired Then
            Me.Button1.Invoke(New MethodInvoker(AddressOf DisableSyncButton))
        Else
            Me.Button1.Enabled = False
        End If
    End Sub


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
                My.Computer.FileSystem.CopyDirectory(lastModifiedPath, folderEntry, True)
            End If
        Next

    End Sub

    Private Sub DisableWatchers(Optional gameId As Integer = Nothing)

        For Each watcher In watchers

            If gameId <> Nothing Then
                If watcher.savegame.game_id = gameId Then
                    watcher.EnableRaisingEvents = False
                End If
            Else
                watcher.EnableRaisingEvents = False
            End If

        Next

    End Sub

    Private Sub EnableWatchers(Optional gameId As Integer = Nothing)

        For Each watcher In watchers

            If gameId <> Nothing Then
                If watcher.savegame.game_id = gameId Then
                    watcher.EnableRaisingEvents = True
                End If
            Else
                watcher.EnableRaisingEvents = True
            End If

        Next

    End Sub

    ' Fully synchronizes a local and could folder
    ' Result: 0 no synch needed, 1 synchronizes from local, -1 synchronizes from cloud
    Public Function SyncPath(folderPath As String, folderId As String, backup As Boolean, backupName As String) As Integer

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

End Class
