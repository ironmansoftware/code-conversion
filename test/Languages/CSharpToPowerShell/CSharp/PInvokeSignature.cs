[DllImport("advapi32.dll", SetLastError = true)]
public static extern bool AbortSystemShutdown(string lpMachineName);

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