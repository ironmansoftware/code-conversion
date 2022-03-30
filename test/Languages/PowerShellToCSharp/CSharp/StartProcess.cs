void Method()
{
	Process process = new Process();
	ProcessStartInfo startInfo = new ProcessStartInfo();
	startInfo.FileName = "notepad.exe";
	startInfo.Arguments = "myText.txt";
	process.StartInfo = startInfo;
	process.Start();
	;
	Process process = new Process();
	ProcessStartInfo startInfo = new ProcessStartInfo();
	startInfo.FileName = "notepad.exe";
	startInfo.Arguments = "myText.txt";
	process.StartInfo = startInfo;
	process.Start();
	;
}