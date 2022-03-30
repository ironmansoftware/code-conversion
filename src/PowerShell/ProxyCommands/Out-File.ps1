function Out-File { [CmdletBinding(DefaultParameterSetName='ByPath', SupportsShouldProcess=$true, ConfirmImpact='Medium', HelpUri='http://go.microsoft.com/fwlink/?LinkID=113363')]
param(
    [Parameter(ParameterSetName='ByPath', Mandatory=$true, Position=0)]
    [string]
    ${FilePath},

    [Parameter(ParameterSetName='ByLiteralPath', Mandatory=$true, ValueFromPipelineByPropertyName=$true)]
    [Alias('PSPath')]
    [string]
    ${LiteralPath},

    [Parameter(Position=1)]
    [ValidateNotNullOrEmpty()]
    [ValidateSet('unknown','string','unicode','bigendianunicode','utf8','utf7','utf32','ascii','default','oem')]
    [string]
    ${Encoding},

    [switch]
    ${Append},

    [switch]
    ${Force},

    [Alias('NoOverwrite')]
    [switch]
    ${NoClobber},

    [ValidateRange(2, 2147483647)]
    [int]
    ${Width},

    [switch]
    ${NoNewline},

    [Parameter(ValueFromPipeline=$true)]
    [psobject]
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

.ForwardHelpTargetName Microsoft.PowerShell.Utility\Out-File
.ForwardHelpCategory Cmdlet

#>
 }