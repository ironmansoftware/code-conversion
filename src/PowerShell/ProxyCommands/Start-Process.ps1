function Start-Process { 
[CmdletBinding(DefaultParameterSetName='Default', HelpUri='http://go.microsoft.com/fwlink/?LinkID=135261')]
param(
    [Parameter(Mandatory=$true, Position=0)]
    [Alias('PSPath')]
    [ValidateNotNullOrEmpty()]
    [string]
    ${FilePath},

    [Parameter(Position=1)]
    [Alias('Args')]
    [ValidateNotNullOrEmpty()]
    [string[]]
    ${ArgumentList},

    [Parameter(ParameterSetName='Default')]
    [Alias('RunAs')]
    [ValidateNotNullOrEmpty()]
    [pscredential]
    [System.Management.Automation.CredentialAttribute()]
    ${Credential},

    [ValidateNotNullOrEmpty()]
    [string]
    ${WorkingDirectory},

    [Parameter(ParameterSetName='Default')]
    [Alias('Lup')]
    [switch]
    ${LoadUserProfile},

    [Parameter(ParameterSetName='Default')]
    [Alias('nnw')]
    [switch]
    ${NoNewWindow},

    [switch]
    ${PassThru},

    [Parameter(ParameterSetName='Default')]
    [Alias('RSE')]
    [ValidateNotNullOrEmpty()]
    [string]
    ${RedirectStandardError},

    [Parameter(ParameterSetName='Default')]
    [Alias('RSI')]
    [ValidateNotNullOrEmpty()]
    [string]
    ${RedirectStandardInput},

    [Parameter(ParameterSetName='Default')]
    [Alias('RSO')]
    [ValidateNotNullOrEmpty()]
    [string]
    ${RedirectStandardOutput},

    [Parameter(ParameterSetName='UseShellExecute')]
    [ValidateNotNullOrEmpty()]
    [string]
    ${Verb},

    [switch]
    ${Wait},

    [ValidateNotNullOrEmpty()]
    [System.Diagnostics.ProcessWindowStyle]
    ${WindowStyle},

    [Parameter(ParameterSetName='Default')]
    [switch]
    ${UseNewEnvironment},

    [Parameter(ParameterSetName='AltDesktop')]
    [string]
    ${Desktop})

begin
{
}

process
{
}

end
{
}
<#

.ForwardHelpTargetName Start-Process
.ForwardHelpCategory Function

#>
 }