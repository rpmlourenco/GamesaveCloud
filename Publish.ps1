# TODO parse directories from PublishProfile xml
$GamesaveCloudCLI_PubFolder = '.\GamesaveCloudCLI\bin\Release\net7.0\singlefile'
$GamesaveCloudManager_PubFolder = '.\GamesaveCloudManager\bin\Release\net7.0-windows\singlefile'
$TempFolder = '.\published\Temp'

if (Test-Path -Path $GamesaveCloudCLI_PubFolder) {
    Remove-Item -Recurse -Force $GamesaveCloudCLI_PubFolder
}
if (Test-Path -Path $GamesaveCloudManager_PubFolder) {
    Remove-Item -Recurse -Force $GamesaveCloudManager_PubFolder
}

dotnet publish -p:PublishProfile=FolderProfileSingleFile -p:Configuration=Release .\GamesaveCloudCLI\GamesaveCloudCLI.csproj
dotnet publish -p:PublishProfile=FolderProfileSingleFile -p:Configuration=Release .\GamesaveCloudManager\GamesaveCloudManager.csproj

if (Test-Path -Path $TempFolder) {
    Remove-Item -Recurse -Force $TempFolder
}
New-Item -Path $TempFolder -ItemType Directory
Copy-Item -Path $GamesaveCloudCLI_PubFolder\*.* -Destination $TempFolder -recurse -Force
$versionCLI = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("$GamesaveCloudCLI_PubFolder\GamesaveCloudCLI.exe").FileVersion
Compress-Archive -Force -Path $TempFolder\*.* -DestinationPath .\published\GamesaveCloudCLI-$versionCLI.zip
Remove-Item -Recurse -Force $TempFolder

New-Item -Path $TempFolder -ItemType Directory
Copy-Item -Path $GamesaveCloudManager_PubFolder\*.* -Destination $TempFolder -recurse -Force
$versionManager = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("$GamesaveCloudCLI_PubFolder\GamesaveCloudManager.exe").FileVersion
Compress-Archive -Force -Path $TempFolder\*.* -DestinationPath .\published\GamesaveCloudManager-$versionManager.zip
Remove-Item -Recurse -Force $TempFolder

# Keep older versions in GitHub
#Remove-Item '.\published\*' -Include GamesaveCloudCLI*.zip

git add -A
git commit -a -m "Publish CLI $versionCLI Manager $versionManager"
git push origin master