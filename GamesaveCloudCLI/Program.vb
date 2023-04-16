Module Program
    Sub Main(args As String())

        Dim intGameId As Integer
        If args.Length = 1 AndAlso Integer.TryParse(args(0), intGameId) Then

            Dim sync = New Synchronizer
            sync.Initialize()
            sync.Sync(args(0))
            sync.Close()
        End If

    End Sub
End Module
