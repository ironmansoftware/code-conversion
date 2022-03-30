function AbortSystemShutdown
{
	param([string]$lpMachineName)
	Add-Type -TypeDefinition '
		using System;
		using System.Runtime.InteropServices;
		public static class PInvoke {
			[DllImport("advapi32.dll", SetLastError = true)]
			public static extern bool AbortSystemShutdown(string lpMachineName);
		}
	'
	[PInvoke]::AbortSystemShutdown($lpMachineName)
}

function CredUIPromptForCredentialsW
{
	param([ref][CREDUI_INFO]$creditUR, [string]$targetName, [IntPtr]$reserved1, [int]$iError, [StringBuilder]$userName, [int]$maxUserName, [StringBuilder]$password, [int]$maxPassword, [ref][bool]$pfSave, [CREDUI_FLAGS]$flags)
	Add-Type -TypeDefinition '
		using System;
		using System.Runtime.InteropServices;
		public static class PInvoke {
			[DllImport("credui", CharSet = CharSet.Unicode)]
			public static extern CredUIReturnCodes CredUIPromptForCredentialsW(ref CREDUI_INFO creditUR,
			string targetName,
			IntPtr reserved1,
			int iError,
			StringBuilder userName,
			int maxUserName,
			StringBuilder password,
			int maxPassword,
			[MarshalAs(UnmanagedType.Bool)] ref bool pfSave,
			CREDUI_FLAGS flags);
		}
	'
	[PInvoke]::CredUIPromptForCredentialsW([ref]$creditUR, $targetName, $reserved1, $iError, $userName, $maxUserName, $password, $maxPassword, [ref]$pfSave, $flags)
}