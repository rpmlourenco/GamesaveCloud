# post build script
$current = (get-item $PWD).FullName
$parent = (get-item $PWD).parent.FullName
# New-Item "C:\Users\rpmlo\Desktop\test.txt"
#$current | Out-file -FilePath "C:\Users\rpmlo\Desktop\test.txt" -Append
#$parent | Out-file -FilePath "C:\Users\rpmlo\Desktop\test.txt" -Append

Start-Process -WindowStyle Minimized -Wait -FilePath "C:\Portable Apps\Playnite\Toolbox.exe" -ArgumentList 'pack', $current, $parent
$pext = (Get-Childitem –Path $parent -Filter "LocalLibrary*.pext" -File -ErrorAction SilentlyContinue | Select-Object -First 1).FullName
$extension = (Get-Childitem –Path "C:\Users\rpmlo\Desktop\Playnite\Extensions" -Filter "LocalLibrary*" -Directory -ErrorAction SilentlyContinue | Select-Object -First 1).FullName

$arg = "x $pext -aoa -o$extension"

#"hello" | Out-file -FilePath "C:\Users\rpmlo\Desktop\test.txt" -Append
#$pext | Out-file -FilePath "C:\Users\rpmlo\Desktop\test.txt" -Append
#$extension | Out-file -FilePath "C:\Users\rpmlo\Desktop\test.txt" -Append
#$arg | Out-file -FilePath "C:\Users\rpmlo\Desktop\test.txt" -Append

Start-Process -FilePath "C:\Program Files\7-Zip\7z.exe" -ArgumentList $arg