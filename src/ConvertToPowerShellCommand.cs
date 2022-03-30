using System;
using System.IO;
using System.Management.Automation;
using CodeConverter;
using CodeConverter.Common;

namespace PowerShellToolsPro.Cmdlets
{
	[Cmdlet(VerbsData.ConvertTo, "PowerShell")]
	public class ConvertToPowerShellCommand : PSCmdlet
	{
		[Parameter(Mandatory = true, ParameterSetName = "PathByPipeline", ValueFromPipeline = true)]
		public FileInfo CSharpFile { get; set; }

		[Parameter(Mandatory = true, ParameterSetName = "Path")]
		public string CSharpFilePath { get; set; }

		[Parameter(Mandatory = true, ParameterSetName = "Text", ValueFromPipeline = true)]
		public string CSharpCode { get; set; }


		protected override void ProcessRecord()
		{
			var c = new Converter();

			if (this.ParameterSetName == "Path")
			{
				ProviderInfo info;
				var paths = this.GetResolvedProviderPathFromPSPath(CSharpFilePath, out info);

				if (info.Name != "FileSystem")
				{
					throw new Exception("Only FileSystem provider is supported.");
				}

				foreach (var path in paths)
				{
					if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
					{
						WriteError(new ErrorRecord(new Exception(string.Format("Unsupported path specified {0}", path)), "", ErrorCategory.InvalidData, path));
						continue;
					}

					var content = File.ReadAllText(path);

					var powerShell = c.Convert(content, Language.PowerShell5);
					WriteObject(powerShell);
				}
			}

			if (ParameterSetName == "PathByPipeline")
			{
				if (!CSharpFile.Exists)
				{
					throw new Exception(string.Format("File {0} not found.", CSharpFile.FullName));
				}

				if (!CSharpFile.Extension.Equals(".CS", StringComparison.OrdinalIgnoreCase))
				{
					throw new Exception(string.Format("Extension {0} not supported. .cs required.", CSharpFile.Extension));
				}

				var text = File.ReadAllText(CSharpFile.FullName);

				var powerShell = c.Convert(text, Language.PowerShell5);
				WriteObject(powerShell);
			}

			if (this.ParameterSetName == "Text")
			{
				var powerShell = c.Convert(CSharpCode, Language.PowerShell5);
				WriteObject(powerShell);
			}
		}

	}
}
