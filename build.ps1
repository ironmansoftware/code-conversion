param($Configuration = 'Debug')

Push-Location $PSScriptRoot
dotnet build -c $Configuration
New-ModuleManifest -Path "$PSScriptRoot\src\bin\$Configuration\netstandard2.0\CodeConversion.psd1" -Author 'Adam Driscoll' -CompanyName 'Ironman Software' -ModuleVersion '1.0.0' -Description 'Convert between PowerShell and C#' -RootModule "CodeConversion.dll" -CmdletsToExport @("ConvertTo-CSharp", "ConvertTo-PowerShell")
Pop-Location 