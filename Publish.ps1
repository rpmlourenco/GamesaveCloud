# TODO parse directories from PublishProfile xml
$GamesaveCloudCLI_PubFolder = '.\GamesaveCloudCLI\bin\Release\net7.0\publish'
$GamesaveCloudManager_PubFolder = '.\GamesaveCloudManager\bin\Release\net7.0-windows\publish'
$TempFolder = '.\published\Temp'

if (Test-Path -Path $GamesaveCloudCLI_PubFolder) {
    Remove-Item -Recurse -Force $GamesaveCloudCLI_PubFolder
}
if (Test-Path -Path $GamesaveCloudManager_PubFolder) {
    Remove-Item -Recurse -Force $GamesaveCloudManager_PubFolder
}

dotnet publish -p:PublishProfile=FolderProfile -p:Configuration=Release .\GamesaveCloudCLI\GamesaveCloudCLI.csproj
dotnet publish -p:PublishProfile=FolderProfile -p:Configuration=Release .\GamesaveCloudManager\GamesaveCloudManager.csproj

if (Test-Path -Path $TempFolder) {
    Remove-Item -Recurse -Force $TempFolder
}
New-Item -Path $TempFolder -ItemType Directory

Copy-Item -Path $GamesaveCloudManager_PubFolder\*.* -Destination $TempFolder -recurse -Force
Copy-Item -Path $GamesaveCloudCLI_PubFolder\*.* -Destination $TempFolder -recurse -Force

$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("$GamesaveCloudCLI_PubFolder\GamesaveCloudCLI.exe").FileVersion
# Keep older versions in GitHub
#Remove-Item '.\published\*' -Include GamesaveCloudCLI*.zip
Compress-Archive -Force -Path $TempFolder\*.* -DestinationPath .\published\GamesaveCloudCLI-$version.zip
Remove-Item -Recurse -Force $TempFolder
git add -A
git commit -a -m "Publish version $version"
git push origin master