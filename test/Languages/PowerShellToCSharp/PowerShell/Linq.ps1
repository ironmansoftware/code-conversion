Get-Process | Where-Object Name -eq "PowerShell.exe" | Select-Object Name | Sort-Object 
Get-Process | ForEach-Object {
	$_.Kill()
}
$myVariableThatHasStuff | ForEach-Object {
	$_.Name
}