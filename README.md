# PowerShell to C# Code Conversion

Orginally a feature of [PowerShell Pro Tools](https://docs.poshtools.com), this module provides the ability to convert between C# and PowerShell. No license required.

# Installation

```powershell
Install-Module CodeConversion
```

# Usage 

## Convert from C# to PowerShell

```powershell
Invoke-CodeConversion -CSharp 'class MyClass { 
    public object MyMethod()
    {
        return new Object();
    }
}'
<#
Outputs: 

function MyMethod
{
        return (New-Object -TypeName Object)
}

#>
```

## Convert from PowerShell to C#

```powershell
Invoke-CodeConversion -PowerShell 'Get-Process'
# Outputs: System.Diagnostics.Process.GetProcesses()
```
