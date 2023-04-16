Public Class SaveGameFile
    Public savegame As SaveGame
    Public path As String
    Public oldPath As String
    Public changeType As String

    Public Sub New(savegame As SaveGame, path As String, oldPath As String, changeType As String)
        Me.savegame = savegame
        Me.path = path
        Me.oldPath = oldPath
        Me.changeType = changeType
    End Sub

End Class

