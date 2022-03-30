function Method
{
	Start-Process -FilePath "notepad.exe" -ArgumentList "myText.txt"
	Start-Process "notepad.exe" -ArgumentList "myText.txt"
}