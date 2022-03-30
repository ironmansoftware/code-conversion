using System;
using System.IO;
using System.Management.Automation;
using CodeConverter;
using CodeConverter.Common;

namespace PowerShellToolsPro.Cmdlets
{
	[Cmdlet(VerbsData.ConvertTo, "CSharp")]
	public class ConvertToCSharpCommand : PSCmdlet
	{
		[Parameter(Mandatory = true, ParameterSetName = "PathByPipeline", ValueFromPipeline = true)]
		public FileInfo PowerShellScriptFile { get; set; }

		[Parameter(Mandatory = true, ParameterSetName = "Path")]
		public string PowerShellScriptPath { get; set; }

		[Parameter(Mandatory = true, ParameterSetName = "Text", ValueFromPipeline = true)]
		public string PowerShellScript { get; set; }

		[Parameter(Mandatory = true, ParameterSetName = "ScriptBlock", ValueFromPipeline = true)]
		public ScriptBlock ScriptBlock { get; set; }

		protected override void ProcessRecord()
		{
			var c = new Converter();

			if (this.ParameterSetName == "Path")
			{
				ProviderInfo info;
				var paths = this.GetResolvedProviderPathFromPSPath(PowerShellScriptPath, out info);

				if (info.Name != "FileSystem")
				{
					throw new Exception("Only FileSystem provider is supported.");
				}

				foreach (var path in paths)
				{
					if (!path.EndsWith("ps1", StringComparison.OrdinalIgnoreCase) && 
						!path.EndsWith("psm1", StringComparison.OrdinalIgnoreCase))
					{
						WriteError(new ErrorRecord(new Exception(string.Format("Unsupported path specified {0}", path)), "", ErrorCategory.InvalidData, path));
						continue;
					}

					var content = File.ReadAllText(path);

					var powerShell = c.Convert(content, Language.CSharp);
					WriteObject(powerShell);
				}
			}

			if (this.ParameterSetName == "PathByPipeline")
			{
				if (!PowerShellScriptFile.Exists)
				{
					throw new Exception("File not found.");
				}

				if (!PowerShellScriptFile.Extension.Equals(".ps1", StringComparison.OrdinalIgnoreCase) &&
				    !PowerShellScriptFile.Extension.Equals(".psm1", StringComparison.OrdinalIgnoreCase))
				{
					throw new Exception("Only PS1 and PSM1 files are supported.");
				}

				var text = File.ReadAllText(PowerShellScriptFile.FullName);

				var powerShell = c.Convert(text, Language.CSharp);
				WriteObject(powerShell);
			}

			if (this.ParameterSetName == "Text")
			{
				var powerShell = c.Convert(PowerShellScript, Language.CSharp);
				WriteObject(powerShell);
			}

			if (this.ParameterSetName == "ScriptBlock")
			{
				var powerShell = c.Convert(ScriptBlock.ToString(), Language.CSharp);
				WriteObject(powerShell);
			}
		}
	}
}
