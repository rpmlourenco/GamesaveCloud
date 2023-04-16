$dir = "C:\Users\rpmlo\source\repos\rpmlourenco\GamesaveCloud\GamesaveCloudUpdater\bin\Debug\net7.0"
$process = Start-Process -FilePath "$dir\GamesaveCloudUpdater.exe" -PassThru -Wait -WindowStyle Minimized -WorkingDirectory "$dir"
$process.HasExited
$result = $process.GetType().GetField('exitCode', 'NonPublic, Instance').GetValue($process)
if (0 -eq $result)
{
    Start-Process -FilePath "$dir\GamesaveCloudCLI.exe" "-t `"$Game`" -s onedrive" -Wait -WindowStyle Minimized -WorkingDirectory "$dir"
}