<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As ComponentModel.ComponentResourceManager = New ComponentModel.ComponentResourceManager(GetType(Form1))
        NotifyIcon1 = New NotifyIcon(components)
        Button1 = New Button()
        TextBox1 = New TextBox()
        BackgroundWorker1 = New ComponentModel.BackgroundWorker()
        SuspendLayout()
        ' 
        ' NotifyIcon1
        ' 
        NotifyIcon1.Icon = CType(resources.GetObject("NotifyIcon1.Icon"), Icon)
        NotifyIcon1.Text = "NotifyIcon1"
        NotifyIcon1.Visible = True
        ' 
        ' Button1
        ' 
        Button1.Location = New Point(12, 10)
        Button1.Margin = New Padding(1)
        Button1.Name = "Button1"
        Button1.Size = New Size(88, 28)
        Button1.TabIndex = 0
        Button1.Text = "Do Stuff"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' TextBox1
        ' 
        TextBox1.Location = New Point(12, 42)
        TextBox1.Multiline = True
        TextBox1.Name = "TextBox1"
        TextBox1.ScrollBars = ScrollBars.Vertical
        TextBox1.Size = New Size(833, 366)
        TextBox1.TabIndex = 1
        ' 
        ' BackgroundWorker1
        ' 
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(857, 420)
        Controls.Add(TextBox1)
        Controls.Add(Button1)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Margin = New Padding(3, 4, 3, 4)
        Name = "Form1"
        Text = "Gamesave Cloud"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents NotifyIcon1 As NotifyIcon
    Friend WithEvents Button1 As Button
    Public WithEvents TextBox1 As TextBox
    Friend WithEvents BackgroundWorker1 As System.ComponentModel.BackgroundWorker
End Class
