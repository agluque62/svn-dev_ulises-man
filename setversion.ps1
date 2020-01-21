Write-Host "Buscando ficheros ..."
$configFiles = Get-ChildItem . AssemblyInfo.cs -rec
foreach ($file in $configFiles)
{
    (Get-Content $file.PSPath) |
    Foreach-Object { $_ -replace "2.5.4", "2.5.5" } |
    Set-Content $file.PSPath
}
# Write-Host "Press any key to continue ..."
# $x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
Read-Host 'Press Enter to continue…' | Out-Null