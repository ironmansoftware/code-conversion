void Method()
{
	using (System.Management.Automation.PowerShell powershell = System.Management.Automation.PowerShell.Create())
	{
		powershell.AddCommand("Backup-BitLockerKeyProtector");
		powershell.AddArgument("XYZ");
		powershell.Invoke();
	}
}