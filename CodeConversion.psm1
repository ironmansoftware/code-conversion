if ($IsLinux) {
    $Path = [IO.Path]::Join($PSScriptRoot, "linux-64", "CodeConversion")
}
else {
    $Path = [IO.Path]::Join($PSScriptRoot, "win7-x64", "CodeConversion.exe")
}

function Invoke-CodeConversion {
    param(
        [Parameter(Mandatory, Position = 0, ParameterSetName = "File")]
        [string]$InputFile,
        [Parameter(Mandatory, Position = 1, ParameterSetName = "File")]
        [string]$OutputFile,
        [Parameter(Mandatory, Position = 0, ParameterSetName = "CSharp")]
        [string]$CSharp,
        [Parameter(Mandatory, Position = 0, ParameterSetName = "PowerShell")]
        [string]$PowerShell
    )

    if ($PSCmdlet.ParameterSetName -eq 'File') {

        & $Path --InputFile $InputFile --OutputFile $OutputFile
    }
    elseif ($PSCmdlet.ParameterSetName -eq 'PowerShell') {
        & $Path --PowerShell $PowerShell
    } 
    elseif ($PSCmdlet.ParameterSetName -eq 'CSharp') {
        & $Path --CSharp $CSharp
    } 

}