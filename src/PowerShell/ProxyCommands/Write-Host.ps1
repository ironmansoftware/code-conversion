function Write-Host { 
[CmdletBinding(HelpUri='http://go.microsoft.com/fwlink/?LinkID=113426', RemotingCapability='None')]
param(
    [Parameter(Position=0, ValueFromPipeline=$true, ValueFromRemainingArguments=$true)]
    [System.Object]
    ${Object},

    [switch]
    ${NoNewline},

    [System.Object]
    ${Separator},

    [System.ConsoleColor]
    ${ForegroundColor},

    [System.ConsoleColor]
    ${BackgroundColor})

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

.ForwardHelpTargetName Microsoft.PowerShell.Utility\Write-Host
.ForwardHelpCategory Cmdlet

#>
 }