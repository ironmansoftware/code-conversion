using CodeConverter.Common;
using System.Collections.Generic;
using PowerShellToolsPro.CodeConversion.CSharp;


namespace CodeConverter.CSharp
{
	public class CSharpIntentVisitor : IntentVisitor
    {
		public override Node VisitWriteFileIntent(WriteFileIntent intent)
		{
			var fileClass = new IdentifierName("File");
			var method = "WriteAllText";
			var arguments = new List<Argument>();

			if (intent.Append != null)
			{
				method = "AppendAllText";
			}

			var memberAccess = new MemberAccess(fileClass, method);

			arguments.Add(new Argument(intent.FilePath));
			arguments.Add(new Argument(intent.Content));

			return new Invocation(memberAccess, new ArgumentList(arguments));
		}

		public override Node VisitWriteHostIntent(WriteHostIntent intent)
		{
			var consoleClass = new IdentifierName("Console");
			var memberAccess = new MemberAccess(consoleClass, "WriteLine");
			return new Invocation(memberAccess, new ArgumentList(new Argument(intent.Object)));
		}

		public override Node VisitStartProcessIntent(StartProcessIntent intent)
		{
			var processCreation = new ObjectCreation("Process", null);
			var processVariable = new VariableDeclaration("Process", new VariableDeclarator("process", processCreation));

			var processInfoCreation = new ObjectCreation("ProcessStartInfo", null);
			var processInfoVariable = new VariableDeclaration("ProcessStartInfo", new VariableDeclarator("startInfo", processInfoCreation));

			var setFileName = new MemberAccess(new IdentifierName("startInfo"), "FileName");
			var filePathAssignment = new Assignment(setFileName, intent.FilePath);

			var setArguments = new MemberAccess(new IdentifierName("startInfo"), "Arguments");
			var argumentsAssignment = new Assignment(setArguments, intent.Arguments);

			var setStartInfo = new MemberAccess(new IdentifierName("process"), "StartInfo");
			var startInfoAssignment = new Assignment(setStartInfo, new IdentifierName("startInfo"));

			var start = new Invocation(new MemberAccess(new IdentifierName("process"), "Start"), new ArgumentList());

			return new Block(processVariable, processInfoVariable, filePathAssignment, argumentsAssignment, startInfoAssignment, start);
		}

	    public override Node VisitGenericIntent(GenericIntent intent)
	    {
		    var arguments = new Dictionary<string, string>();
		    foreach (var argument in intent.Arguments)
		    {
				var writer = new CSharpCodeWriter();
			    var result = writer.Write(argument.Value);

			    arguments.Add(argument.Key, result);
		    }
			var intentTemplates = new IntentTemplates();

		    var code = intentTemplates.GetCodeForIntent(intent.Name, arguments);
			return new RawCode(code);
	    }
	}
}
