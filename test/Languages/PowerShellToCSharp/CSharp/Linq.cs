System.Diagnostics.Process.GetProcesses().Where(m => m.Name == "PowerShell.exe").Select(m => m.Name).OrderBy(m => m);
System.Diagnostics.Process.GetProcesses().ToList().ForEach(_ => { _.Kill();
 });
myVariableThatHasStuff.ToList().ForEach(_ => { _.Name;
 });