param($Configuration = 'Debug')

$Path = Join-Path $PSScriptRoot "output" 
Remove-Item -Path $Path -Recurse -Force -ErrorAction SilentlyContinue

Push-Location $PSScriptRoot
New-Item -Path $Path -ItemType Directory

@("win7-x64", 'linux-x64') | ForEach-Object {
    dotnet publish -c $Configuration -o (Join-Path $Path $_) -r $_
}

Copy-Item "CodeConversion.psm1" $Path

$Parameters = @{
    Path              = "$Path\CodeConversion.psd1"
    Author            = 'Adam Driscoll'
    CompanyName       = 'Ironman Software'
    ModuleVersion     = '2.0.0'
    Description       = 'Convert between PowerShell and C#'
    RootModule        = "CodeConversion.psm1"
    FunctionsToExport = @("Invoke-CodeConversion")
    ProjectUri        = 'https://github.com/ironmansoftware/code-conversion'
    LicenseUri        = 'https://github.com/ironmansoftware/code-conversion/blob/main/LICENSE'

}

New-ModuleManifest @Parameters

Pop-Location 