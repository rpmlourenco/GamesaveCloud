Imports Google.Apis.Drive.v3
Public Class SaveGame
    Public game_id As Integer
    Public savegame_id As Integer
    Public title As String
    Public path As String
    Public driveFolder As Data.File

    Public Sub New(game_id As Integer, savegame_id As Integer, title As String, path As String, driveFolder As Data.File)
        Me.game_id = game_id
        Me.savegame_id = savegame_id
        Me.title = title
        Me.path = path
        Me.driveFolder = driveFolder
    End Sub
End Class
