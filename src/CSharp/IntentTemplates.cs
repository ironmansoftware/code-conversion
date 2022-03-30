using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerShellToolsPro.CodeConversion.CSharp
{
	public class IntentTemplates
	{
		private static readonly IEnumerable<IIntentWriter> IntentWriters;

		static IntentTemplates()
		{
			IntentWriters = new IIntentWriter[]
			{
				new StartServiceIntent(),
				new StopServiceIntent(),
				new PauseServiceIntent(),
				new RestartServiceIntent(),
				new GetCimInstanceIntent(),
				new StartSleepIntent(),
				new NewVariableIntent(),
				new TestPathIntent(),
				new RemoveVariableIntent(),
				new WriteErrorIntent(),
				new GetProcessIntent(),
				new WhereIntent(),
				new SelectIntent() ,
				new SortIntent(),
				new ForEachIntent(),
				new WriteVerboseIntent()
			};
		}

		public string GetCodeForIntent(string intent, Dictionary<string, string> arguments)
		{
			var intentWriter = IntentWriters.FirstOrDefault(m => m.CanWrite(intent, arguments));
			if (intentWriter == null) return string.Empty;

			return intentWriter.Write(arguments);
		}
	}

	public interface IIntentWriter
	{
		bool CanWrite(string intentName, Dictionary<string, string> arguments);
		string Write(Dictionary<string, string> arguments);
	}

	public abstract class BaseIntentWriter : IIntentWriter
	{
		public abstract string Intent { get; }

		public bool CanWrite(string intentName, Dictionary<string, string> arguments)
		{
			return Intent.Equals(intentName, StringComparison.OrdinalIgnoreCase);
		}

		public abstract string Write(Dictionary<string, string> arguments);
	}

	public class StartServiceIntent : BaseIntentWriter
	{
		public override string Intent { get { return "StartService"; } }
		public override string Write(Dictionary<string, string> arguments)
		{
			return $@"System.ServiceProcess.ServiceController sc = new System.ServiceProcess.ServiceController({arguments["Name"]});
sc.Start();";
		}
	}

	public class StopServiceIntent : BaseIntentWriter
	{
		public override string Intent { get { return "StopService"; } }
		public override string Write(Dictionary<string, string> arguments)
		{
			return $@"System.ServiceProcess.ServiceController sc = new System.ServiceProcess.ServiceController({arguments["Name"]});
sc.Stop();";
		}
	}

	public class PauseServiceIntent : BaseIntentWriter
	{
		public override string Intent { get { return "PauseService"; } }
		public override string Write(Dictionary<string, string> arguments)
		{
			return $@"System.ServiceProcess.ServiceController sc = new System.ServiceProcess.ServiceController({arguments["Name"]});
sc.Pause();";
		}
	}

	public class RestartServiceIntent : BaseIntentWriter
	{
		public override string Intent { get { return "RestartService"; } }
		public override string Write(Dictionary<string, string> arguments)
		{
			return $@"System.ServiceProcess.ServiceController sc = new System.ServiceProcess.ServiceController({arguments["Name"]});
sc.Stop();
sc.Start();";
		}
	}

	public class StartSleepIntent : BaseIntentWriter
	{
		public override string Intent { get { return "StartSleep"; } }
		public override string Write(Dictionary<string, string> arguments)
		{
			int milliseconds = 0;
			if (arguments.ContainsKey("Seconds"))
			{
				int value;
				var secondsString = arguments["Seconds"];
				int.TryParse(secondsString, out value);
				milliseconds = value * 1000;
			}

			if (arguments.ContainsKey("Milliseconds"))
			{
				var msString = arguments["Milliseconds"];
				int.TryParse(msString, out milliseconds);
			}

			return $"System.Threading.Thread.Sleep({milliseconds})";
		}
	}

	public class GetCimInstanceIntent : IIntentWriter
	{
		public bool CanWrite(string intentName, Dictionary<string, string> arguments)
		{
			return intentName.Equals("GetCimInstance", StringComparison.OrdinalIgnoreCase) && arguments.ContainsKey("Class");
		}

		public string Write(Dictionary<string, string> arguments)
		{
			var computerName = "localhost";
			if (arguments.ContainsKey("ComputerName"))
			{
				computerName = arguments["ComputerName"];
			}

			var ns = "CIM_V2";
			if (arguments.ContainsKey("Namespace"))
			{
				ns = arguments["Namespace"];
			}

			var scopeString = $"\"\\\\\\\\\" + {computerName} + \"\\\\\" + {ns}";

			var connectionOptions = "ConnectionOptions connectionOptions = new ConnectionOptions()";
			if (arguments.ContainsKey("Authentication"))
			{
				connectionOptions += Environment.NewLine + $"connectionOptions.Authentication = {arguments["Authentication"]};";
			}
			if (arguments.ContainsKey("Locale"))
			{
				connectionOptions += Environment.NewLine + $"connectionOptions.Locale = {arguments["Locale"]};";
			}
			if (arguments.ContainsKey("Authority"))
			{
				connectionOptions += Environment.NewLine + $"connectionOptions.Authority = {arguments["Authority"]};";
			}
			if (arguments.ContainsKey("EnableAllPrivileges"))
			{
				connectionOptions += Environment.NewLine + $"connectionOptions.EnablePrivileges = true;";
			}
			if (arguments.ContainsKey("Impersonation"))
			{
				connectionOptions += Environment.NewLine + $"connectionOptions.EnablePrivileges =  {arguments["Impersonation"]};";
			}
			if (arguments.ContainsKey("Credential"))
			{
				connectionOptions += Environment.NewLine + $"connectionOptions.UserName = \"PSCredential.UserName\";";
				connectionOptions += Environment.NewLine + $"connectionOptions.SecurePassword = \"PSCredential.Password\";";
			}

			var queryString = GetQueryString(arguments);

			return
				$@"{connectionOptions}
ManagementScope scope = new ManagementScope({scopeString}, connectionOptions);
scope.Connect();
ObjectQuery objectQuery = new ObjectQuery({queryString});
ObjectQuery query2 = objectQuery;
using (var searcher = new ManagementObjectSearcher(scope, query, options))
{{
	searcher.Get();
}}";
		}

		internal string GetQueryString(Dictionary<string, string> arguments)
		{
			var stringBuilder = new StringBuilder("\"select \" + ");

			if (arguments.ContainsKey("Properties"))
			{
				stringBuilder.Append("string.Join(',', "+ arguments["Properties"].TrimEnd(';') + ")");
			}
			else
			{
				stringBuilder.Append("\"*\"");
			}

			stringBuilder.Append(" + \" from \" + ");
			stringBuilder.Append(arguments["Class"]);
			if (arguments.ContainsKey("Filter"))
			{
				stringBuilder.Append(" + \" where \" + ");
				stringBuilder.Append(arguments["Filter"]);
			}
			return stringBuilder.ToString();
		}
	}

	public class NewVariableIntent : BaseIntentWriter
	{
		public override string Intent { get { return "NewVariable"; } }
		public override string Write(Dictionary<string, string> arguments)
		{
			var name = arguments["Name"];
			name = name.Trim('\"');

			return $"var {name} = {arguments["Value"]};";
		}
	}

	public class TestPathIntent : BaseIntentWriter
	{
		public override string Intent { get { return "TestPath"; } }
		public override string Write(Dictionary<string, string> arguments)
		{
			var path = arguments["Path"];

			return $"(System.IO.File.Exist({path}) || System.IO.Directory.Exists({path}))";
		}
	}

	public class RemoveVariableIntent : BaseIntentWriter
	{
		public override string Intent { get { return "RemoveVariable"; } }
		public override string Write(Dictionary<string, string> arguments)
		{
			var name = arguments["Name"];
			name = name.Trim('\"');

			return $"{name} = null";
		}
	}

	public class WriteErrorIntent : BaseIntentWriter
	{
		public override string Intent { get { return "WriteError"; } }
		public override string Write(Dictionary<string, string> arguments)
		{
			var message = arguments["Message"];
			return $"System.Console.Error.WriteLine({message})";
		}
	}

	public class WriteVerboseIntent : BaseIntentWriter
	{
		public override string Intent { get { return "WriteVerbose"; } }
		public override string Write(Dictionary<string, string> arguments)
		{
			var message = arguments["Message"];
			return $"System.Diagnostics.Debug.WriteLine({message})";
		}
	}

	public class GetProcessIntent : BaseIntentWriter
	{
		public override string Intent { get { return "GetProcess"; } }
		public override string Write(Dictionary<string, string> arguments)
		{
			if (arguments.ContainsKey("Id"))
			{
				return $"System.Diagnostics.Process.GetProcessById({arguments["Id"]})";
			}

			if (arguments.ContainsKey("Name"))
			{
				return $"System.Diagnostics.Process.GetProcessesByName({arguments["Name"]})";
			}

			return $"System.Diagnostics.Process.GetProcesses()";
		}
	}

	public class WhereIntent : BaseIntentWriter
	{
		public override string Intent { get { return "Where"; } }
		public override string Write(Dictionary<string, string> arguments)
		{
			if (arguments.ContainsKey("Property") && arguments.ContainsKey("Value"))
			{
				var comparison = string.Empty;
				if (arguments.ContainsKey("Equal"))
				{
					comparison = "==";
				}

				return $"Where(m => m.{arguments["Property"].Trim('\"')} {comparison} {arguments["Value"]})";
			}

			return string.Empty;
		}
	}

	public class SelectIntent : BaseIntentWriter
	{
		public override string Intent { get { return "Select"; } }
		public override string Write(Dictionary<string, string> arguments)
		{
			if (arguments.ContainsKey("Property"))
			{
				return $"Select(m => m.{arguments["Property"].Trim('\"')})";
			}

			return string.Empty;
		}
	}


	public class SortIntent : BaseIntentWriter
	{
		public override string Intent { get { return "Sort"; } }
		public override string Write(Dictionary<string, string> arguments)
		{
			string sort;

			if (arguments.ContainsKey("Descending"))
			{
				sort = $"OrderByDescending(m => m";
			}
			else
			{
				sort = $"OrderBy(m => m";
			}

			if (arguments.ContainsKey("Property") && !string.IsNullOrEmpty(arguments["Property"]))
			{
				sort += "." + arguments["Property"].Trim('\"');
			}

			sort += ")";

			return sort;
		}
	}

	public class ForEachIntent : BaseIntentWriter
	{
		public override string Intent { get { return "ForEach"; } }
		public override string Write(Dictionary<string, string> arguments)
		{
			if (arguments.ContainsKey("Process"))
			{
				return $"ToList().ForEach(_ => {{ {arguments["Process"]} }})";
			}

			return string.Empty;
		}
	}
}
