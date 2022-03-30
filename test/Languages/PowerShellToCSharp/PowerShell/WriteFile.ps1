function Method()
{
	Out-File -InputObject "My output test" -FilePath "supersecretfile.txt"
	Out-File "supersecretfile.txt" -InputObject "My output test" -Append
	Add-Content -Value "My output test" -Path "supersecretfile.txt"
}