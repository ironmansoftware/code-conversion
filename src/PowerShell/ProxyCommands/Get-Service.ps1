function Get-Service { [CmdletBinding(DefaultParameterSetName='Default', HelpUri='http://go.microsoft.com/fwlink/?LinkID=113332', RemotingCapability='SupportedByCommand')]
param(
    [Parameter(ParameterSetName='Default', Position=0, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]
    [Alias('ServiceName')]
    [string[]]
    ${Name},

    [Parameter(ValueFromPipelineByPropertyName=$true)]
    [Alias('Cn')]
    [ValidateNotNullOrEmpty()]
    [string[]]
    ${ComputerName},

    [Alias('DS')]
    [switch]
    ${DependentServices},

    [Alias('SDO','ServicesDependedOn')]
    [switch]
    ${RequiredServices},

    [Parameter(ParameterSetName='DisplayName', Mandatory=$true)]
    [string[]]
    ${DisplayName},

    [ValidateNotNullOrEmpty()]
    [string[]]
    ${Include},

    [ValidateNotNullOrEmpty()]
    [string[]]
    ${Exclude},

    [Parameter(ParameterSetName='InputObject', ValueFromPipeline=$true)]
    [ValidateNotNullOrEmpty()]
    [System.ServiceProcess.ServiceController[]]
    ${InputObject})

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

.ForwardHelpTargetName Microsoft.PowerShell.Management\Get-Service
.ForwardHelpCategory Cmdlet

#>
 }