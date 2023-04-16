Imports System.IO
Imports GamesaveCloudLib

Public Class GameFileSystemWatcher
    Inherits FileSystemWatcher
    Public savegame As SaveGame
    Public form As Object

    Public Sub New(path As String)
        MyBase.New(path)
    End Sub
    Public Sub New(path As String, filter As String)
        MyBase.New(path, filter)
    End Sub

End Class

