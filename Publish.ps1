dotnet publish -p:PublishProfile=FolderProfile -p:Configuration=Release .\GamesaveCloudCLI\GamesaveCloudCLI.csproj
dotnet publish -p:PublishProfile=FolderProfile -p:Configuration=Release .\GamesaveCloudManager\GamesaveCloudManager.csproj

$TempFolder = '.\published\Temp'
if (Test-Path -Path $TempFolder) {
    Remove-Item -Recurse -Force $TempFolder
}
New-Item -Path $TempFolder -ItemType Directory

Copy-Item -Path .\GamesaveCloudCLI\bin\Release\net7.0\publish\win-x64 -Destination $TempFolder -Force
Copy-Item -Path .\GamesaveCloudManager\bin\Release\net7.0-windows\publish -Destination $TempFolder -Force

$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo(".\GamesaveCloudCLI\bin\Release\net7.0\publish\win-x64\GamesaveCloudCLI.exe").FileVersion
#Remove-Item '.\published\*' -Include GamesaveCloudCLI*.zip
Compress-Archive -Force -Path $TempFolder\*.* -DestinationPath .\published\GamesaveCloudCLI-$version.zip
#git add -A
#git commit -a -m "Publish version $version"
#git push origin master