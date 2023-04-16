dotnet publish -p:PublishProfile=FolderProfile -p:Configuration=Release .\GamesaveCloudCLI\GamesaveCloudCLI.csproj
$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo(".\GamesaveCloudCLI\bin\Release\net7.0\publish\win-x64\GamesaveCloudCLI.exe").FileVersion
Remove-Item '.\GamesaveCloudCLI\bin\Release\net7.0\publish\win-x64\*' -Include GamesaveCloudCLI*.zip
Compress-Archive -Force -Path .\GamesaveCloudCLI\bin\Release\net7.0\publish\win-x64\*.* -DestinationPath .\published\GamesaveCloudCLI-$version.zip
git add -A
git commit -a -m "Publish version $version"
git push origin master