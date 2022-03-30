# PowerShell to C# Code Conversion

Orginally a feature of [PowerShell Pro Tools](https://docs.poshtools.com), this module provides the ability to convert between C# and PowerShell. 

# Installation

```powershell
Install-Module CodeConversion
```

# Usage 

## Convert from C# to PowerShell

```powershell
ConvertTo-PowerShell -Code 'System.Diagnostics.Process.GetProcesses()'
# Outputs: Get-Process
```

## Convert from PowerShell to C#

```powershell
ConvertTo-CSharp -Code 'Get-Process'
# Outputs: System.Diagnostics.Process.GetProcesses()
```