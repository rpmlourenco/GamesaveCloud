Imports Google.Apis.Auth.OAuth2
Imports Google.Apis.Drive.v3
Imports Google.Apis.Services
Imports Google.Apis.Util.Store
Imports System.IO
Imports System.Reflection


Public Class GoolgeDriveHelper

    Private credentials As UserCredential
    Private service As DriveService


    Public Sub New(stream As Stream)
        credentials = Autenticacao(stream)
        service = OpenService(credentials)
    End Sub


    Public Function Autenticacao(stream As Stream) As UserCredential
        Dim credenciais As UserCredential

        Using stream
            Dim pathAtual = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            Dim pathCredenciais = Path.Combine(pathAtual, "credential")
            credenciais = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.FromStream(stream).Secrets, {DriveService.Scope.Drive}, "user", Threading.CancellationToken.None, New FileDataStore(pathCredenciais, True)).Result
        End Using
        Return credenciais
    End Function

    Private Function OpenService(ByVal credenciais As UserCredential) As DriveService
        Return New DriveService(New BaseClientService.Initializer() With {.HttpClientInitializer = credenciais})
    End Function

    Public Function Query(sQuery As String, sFields As String) As IList(Of Data.File)

        Dim request As FilesResource.ListRequest = service.Files.List()
        request.Q = sQuery
        request.Spaces = "drive"
        request.Fields = sFields

        Return request.Execute().Files

    End Function

    Public Function GetFiles(parentId As String, Optional name As String = Nothing) As IList(Of Data.File)

        Dim sQuery As String
        If name IsNot Nothing Then
            sQuery = "mimeType != 'application/vnd.google-apps.folder' and trashed = false and parents in '" + parentId + "' and name = '" + name + "'"
        Else
            sQuery = "mimeType != 'application/vnd.google-apps.folder' and trashed = false and parents in '" + parentId + "'"
        End If

        Return Query(sQuery, "files(id,name,createdTime,modifiedTime,size)")

    End Function

    Public Function GetFolders(parentId As String, Optional name As String = Nothing) As IList(Of Data.File)

        Dim sQuery As String
        If name IsNot Nothing Then
            sQuery = "mimeType = 'application/vnd.google-apps.folder' and trashed = false and parents in '" + parentId + "' and name = '" + name + "'"
        Else
            sQuery = "mimeType = 'application/vnd.google-apps.folder' and trashed = false and parents in '" + parentId + "'"
        End If

        Return Query(sQuery, "files(id,name)")

    End Function

    Public Function GetFile(parentId As String, name As String) As Data.File

        Dim files = GetFiles(parentId, name)
        If (files IsNot Nothing And files.Count > 0) Then
            Return files(0)
        Else
            Return Nothing
        End If

    End Function

    Public Function GetFolder(parentId As String, name As String) As Data.File

        Dim files = GetFolders(parentId, name)
        If (files IsNot Nothing And files.Count > 0) Then
            Return files(0)
        Else
            Return Nothing
        End If

    End Function

    Private Function FindFile(name As String, files As IList(Of Data.File)) As Data.File

        If (files IsNot Nothing And files.Count > 0) Then
            For Each file In files
                If file.Name = name Then
                    Return file
                End If
            Next
        Else
            Return Nothing
        End If

        Return Nothing

    End Function

    Public Function UploadFileInFolder(filePath As String, folderId As String, checkExists As Boolean) As Data.File

        Dim plist As List(Of String) = New List(Of String)
        plist.Add(folderId) 'Set parent driveFolder

        Dim fileName As String = Path.GetFileName(filePath)

        If File.Exists(filePath) Then
            Dim fileMetadata = New Data.File()
            fileMetadata.Name = Path.GetFileName(filePath)
            fileMetadata.Parents = plist
            fileMetadata.CreatedTime = File.GetCreationTime(filePath)
            fileMetadata.ModifiedTime = File.GetLastWriteTime(filePath)

            If checkExists Then
                Dim drivefile = GetFile(folderId, fileName)
                If (drivefile IsNot Nothing) Then
                    service.Files().Delete(drivefile.Id).Execute()
                End If
            End If

            Dim request As FilesResource.CreateMediaUpload

            Using stream = New System.IO.FileStream(filePath, System.IO.FileMode.Open)
                request = service.Files.Create(fileMetadata, stream, "application/octet-stream")
                request.Fields = "id, parents"
                request.Upload()
            End Using

            Return request.ResponseBody
        Else
            Return Nothing
        End If

    End Function

    Public Function DownloadFileByName(fileName As String, parentId As String, outputPath As String) As Boolean

        Dim fullOutputPath As String = Path.Combine(outputPath, fileName)
        Dim outputStream = New FileStream(fullOutputPath, FileMode.Create, FileAccess.ReadWrite)
        Dim driveFile As Data.File = GetFile(parentId, fileName)

        If driveFile IsNot Nothing Then

            Dim request = service.Files.Get(driveFile.Id)
            request.Download(outputStream)
            outputStream.Flush()
            outputStream.Close()

            File.SetLastWriteTime(fullOutputPath, driveFile.ModifiedTime)
            File.SetCreationTime(fullOutputPath, driveFile.CreatedTime)

            Return True
        Else
            Return False
        End If
    End Function

    Public Function DownloadFile(driveFile As Data.File, outputPath As String) As Boolean

        Dim fileName = Path.GetFileName(outputPath)
        Dim outputStream = New FileStream(outputPath, FileMode.Create, FileAccess.ReadWrite)

        Dim request = service.Files.Get(driveFile.Id)
        request.Download(outputStream)
        outputStream.Flush()
        outputStream.Close()

        File.SetLastWriteTime(outputPath, driveFile.ModifiedTime)
        File.SetCreationTime(outputPath, driveFile.CreatedTime)

        Return True

    End Function

    Public Sub DeleteFileByName(fileName As String, parentId As String)
        Dim driveFile = GetFile(parentId, fileName)
        If (driveFile IsNot Nothing) Then
            service.Files().Delete(driveFile.Id).Execute()
        End If
    End Sub

    Public Sub DeleteFile(drivefile As Data.File)
        service.Files().Delete(drivefile.Id).Execute()
    End Sub


    Public Function NewFolder(name As String, parentId As String, Optional createdTime As Date = Nothing, Optional modifiedTime As Date = Nothing) As Data.File

        Dim plist As List(Of String) = New List(Of String)
        plist.Add(parentId) 'Set parent driveFolder

        Dim folder = New Data.File()
        folder.Name = name
        folder.MimeType = "application/vnd.google-apps.folder"
        folder.Parents = plist
        If (createdTime <> Nothing) Then
            folder.CreatedTime = createdTime
        End If
        If (modifiedTime <> Nothing) Then
            folder.ModifiedTime = modifiedTime
        End If

        Dim request = service.Files.Create(folder)
        Return request.Execute()

    End Function



    Public Sub SyncFromLocal(folderPath As String, folderId As String)

        ' Delete files in drive which do not exist locally
        Dim files = GetFiles(folderId)
        If (files IsNot Nothing And files.Count > 0) Then
            For Each file In files
                Dim checkFile As String = Path.Combine(folderPath, file.Name)
                If Not System.IO.File.Exists(checkFile) Then
                    service.Files().Delete(file.Id).Execute()
                End If
            Next
        End If

        Dim folders = GetFolders(folderId)
        If (folders IsNot Nothing And folders.Count > 0) Then
            For Each folder In folders
                Dim checkFolder As String = Path.Combine(folderPath, folder.Name)
                If Not Directory.Exists(checkFolder) Then
                    service.Files().Delete(folder.Id).Execute()
                End If
            Next
        End If

        ' Uploads non existing or different files
        Dim fileEntries As String() = Directory.GetFiles(folderPath)
        For Each fileEntry In fileEntries
            Dim fileLastModified As Date = File.GetLastWriteTime(fileEntry)
            Dim size As Long = (New FileInfo(fileEntry)).Length

            Dim driveFile = FindFile(Path.GetFileName(fileEntry), files)
            If driveFile Is Nothing Then
                UploadFileInFolder(fileEntry, folderId, False)
            Else
                Dim driveLastModified As Date = driveFile.ModifiedTime
                If fileLastModified.ToString("yyyyMMdd HHmmss") <> driveLastModified.ToString("yyyyMMdd HHmmss") Or size <> driveFile.Size Then
                    service.Files().Delete(driveFile.Id).Execute()
                    UploadFileInFolder(fileEntry, folderId, False)
                End If
            End If
        Next

        Dim folderEntries As String() = Directory.GetDirectories(folderPath)
        For Each folderEntry In folderEntries

            Dim folderName = Path.GetFileName(folderEntry)
            Dim driveFolder = FindFile(folderName, folders)

            If driveFolder Is Nothing Then
                'driveFolder = DriveNewFolder(service, folderName, folderId, Directory.GetCreationTime(folderEntry), Directory.GetLastWriteTime(folderEntry))
                driveFolder = NewFolder(folderName, folderId)
            End If

            SyncFromLocal(folderEntry, driveFolder.Id)
        Next

    End Sub

    Public Sub SyncFromDrive(folderPath As String, folderId As String)

        Dim files = GetFiles(folderId)
        Dim folders = GetFolders(folderId)

        Dim fileEntries As String() = Directory.GetFiles(folderPath)
        For Each fileEntry In fileEntries

            Dim driveFile = FindFile(Path.GetFileName(fileEntry), files)
            If driveFile Is Nothing Then
                File.Delete(fileEntry)
            End If
        Next

        Dim folderEntries As String() = Directory.GetDirectories(folderPath)
        For Each folderEntry In folderEntries

            Dim folderName = Path.GetFileName(folderEntry)
            Dim driveFolder = FindFile(folderName, folders)

            If driveFolder Is Nothing Then
                Directory.Delete(folderEntry, True)
            End If
        Next

        If (files IsNot Nothing And files.Count > 0) Then
            For Each fileEntry In files
                Dim checkFile As String = Path.Combine(folderPath, fileEntry.Name)
                If Not File.Exists(checkFile) Then
                    DownloadFile(fileEntry, checkFile)
                Else
                    Dim fileLastModified As Date = File.GetLastWriteTime(checkFile)
                    Dim size As Long = (New FileInfo(checkFile)).Length
                    Dim driveLastModified As Date = fileEntry.ModifiedTime

                    If fileLastModified.ToString("yyyyMMdd HHmmss") <> driveLastModified.ToString("yyyyMMdd HHmmss") Or size <> fileEntry.Size Then
                        DownloadFile(fileEntry, checkFile)
                    End If
                End If
            Next
        End If

        If (folders IsNot Nothing And folders.Count > 0) Then
            For Each folder In folders

                Dim checkFolder As String = System.IO.Path.Combine(folderPath, folder.Name)
                If Not Directory.Exists(checkFolder) Then
                    Directory.CreateDirectory(checkFolder)
                    'Directory.SetCreationTime(checkFolder, driveFolder.CreatedTime)
                    'Directory.SetLastWriteTime(checkFolder, driveFolder.ModifiedTime)
                End If

                SyncFromDrive(checkFolder, folder.Id)
            Next
        End If

    End Sub

    Public Function LastModifiedDate(folderId As String, ByRef totalFiles As Integer) As Date

        Dim lastModified As Date = Nothing
        Dim files = GetFiles(folderId)
        totalFiles += files.Count
        If (files IsNot Nothing And files.Count > 0) Then
            For Each file In files

                If lastModified = Nothing Or file.ModifiedTime > lastModified Then
                    lastModified = file.ModifiedTime
                End If

            Next
        End If

        Dim folders = GetFolders(folderId)
        totalFiles += folders.Count
        If (folders IsNot Nothing And folders.Count > 0) Then
            For Each folder In folders

                Dim folderLastModified As Date = LastModifiedDate(folder.Id, totalFiles)
                If lastModified = Nothing Or folderLastModified > lastModified Then
                    lastModified = folderLastModified
                End If

            Next
        End If

        Return lastModified

    End Function

    Public Function LocalLastModifiedDate(folderPath As String, ByRef totalFiles As Integer) As Date

        Dim lastModified As Date = Nothing

        Dim fileEntries As String() = Directory.GetFiles(folderPath)
        totalFiles += fileEntries.Length
        For Each fileName In fileEntries
            Dim fileLastModified As Date = File.GetLastWriteTime(fileName)
            If lastModified = Nothing Or fileLastModified > lastModified Then
                lastModified = fileLastModified
            End If
        Next

        Dim folderEntries As String() = Directory.GetDirectories(folderPath)
        totalFiles += folderEntries.Length
        For Each folderEntry In folderEntries
            Dim folderLastModified As Date = LocalLastModifiedDate(folderEntry, totalFiles)
            If lastModified = Nothing Or folderLastModified > lastModified Then
                lastModified = folderLastModified
            End If
        Next

        Return lastModified

    End Function

End Class
