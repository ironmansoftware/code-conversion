using System.Collections.Generic;

namespace PowerShellToolsPro.CodeConversion.PowerShell
{
	public class CommandIntentMapping
	{
		public string CommandName { get; set; }
		public string Intent { get; set; }
		public Dictionary<string,string> ParameterMappings { get; set; }
	}
}
