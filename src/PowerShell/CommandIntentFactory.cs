using CodeConverter.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;
using Newtonsoft.Json;
using PowerShellToolsPro.CodeConversion.PowerShell;

namespace CodeConverter.PowerShell
{
    public class CommandIntentFactory
    {
        private static Dictionary<string, CommandIntentMapping> _commandIntentMappings;
        static CommandIntentFactory()
        {
            _commandIntentMappings = new Dictionary<string, CommandIntentMapping>();
            const string jsonMappings = "CodeConversion.PowerShell.CommandIntentMappings.json";
            var assembly = typeof(CommandIntentFactory).Assembly;

            CommandIntentMapping[] mappings;
            using (var stream = assembly.GetManifestResourceStream(jsonMappings))
            using (var reader = new StreamReader(stream))
            {
                var mappingsJson = reader.ReadToEnd();
                mappings = JsonConvert.DeserializeObject<CommandIntentMapping[]>(mappingsJson);
            }

            if (mappings == null) return;

            foreach (var mapping in mappings)
            {
                _commandIntentMappings.Add(mapping.CommandName.ToUpper(), mapping);
            }
        }

        public Intent DetermineCommandIntent(Invocation node)
        {
            var name = node.Expression as IdentifierName;
            if (name == null) return null;

            if (name.Name.Equals("Add-Content", StringComparison.OrdinalIgnoreCase))
            {
                return ProcessAddContent(node);
            }
            else if (name.Name.Equals("Get-Service", StringComparison.OrdinalIgnoreCase))
            {
                return ProcessGetService(node);
            }
            else if (name.Name.Equals("Out-File", StringComparison.OrdinalIgnoreCase))
            {
                return ProcessOutFile(node);
            }
            else if (name.Name.Equals("Start-Process", StringComparison.OrdinalIgnoreCase))
            {
                return ProcessStartProcess(node);
            }
            else if (name.Name.Equals("Write-Host", StringComparison.OrdinalIgnoreCase))
            {
                return ProcessWriteHost(node);
            }
            else
            {
                return ProcessGenericMappedIntent(name.Name, node);
            }
        }

        private Intent ProcessAddContent(Invocation node)
        {
            var filePath = GetParameter("Path", node);
            if (filePath == null) return null;
            var value = GetParameter("Value", node);
            if (value == null) return null;

            return new WriteFileIntent(node)
            {
                Append = new Argument("Append", null),
                Content = value.Expression,
                FilePath = filePath.Expression
            };
        }

        private Intent ProcessGetService(Invocation node)
        {
            var name = GetParameter("Name", node);

            return new GetServiceIntent(node)
            {
                Name = name
            };
        }

        private Intent ProcessOutFile(Invocation node)
        {
            var filePath = GetParameter("FilePath", node);
            if (filePath == null) return null;
            var append = GetParameter("Append", node);
            var content = GetParameter("InputObject", node);
            if (content == null) return null;

            return new WriteFileIntent(node)
            {
                Append = append,
                Content = content.Expression,
                FilePath = filePath.Expression
            };
        }

        private Intent ProcessStartProcess(Invocation node)
        {
            var filePath = GetParameter("FilePath", node);
            if (filePath == null) return null;

            var argumentList = GetParameter("ArgumentList", node);
            if (argumentList == null) return null;

            return new StartProcessIntent(node)
            {
                FilePath = filePath,
                Arguments = argumentList
            };
        }

        private Intent ProcessWriteHost(Invocation node)
        {
            var obj = GetParameter("Object", node);
            if (obj == null) return null;

            return new WriteHostIntent(node)
            {
                Object = obj
            };
        }

        private Intent ProcessGenericMappedIntent(string name, Invocation node)
        {
            if (!_commandIntentMappings.ContainsKey(name.ToUpper()))
            {
                return null;
            }

            var mapping = _commandIntentMappings[name.ToUpper()];

            var genericIntent = new GenericIntent(node);
            genericIntent.Name = mapping.Intent;
            genericIntent.Arguments = new Dictionary<string, Node>();

            foreach (var parameter in mapping.ParameterMappings)
            {
                var argument = GetParameter(parameter.Key, node);
                if (argument == null) continue;

                genericIntent.Arguments.Add(parameter.Value, argument);
            }

            return genericIntent;
        }

        private Argument GetParameter(string name, Invocation node)
        {
            return node.Arguments.Arguments.Cast<Argument>().FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class ParameterFinder
    {
        public static Dictionary<string, object> FindBoundParameters(CommandAst commandAst)
        {
            var commandName = commandAst.GetCommandName();

            var assembly = typeof(ParameterFinder).Assembly;
            var resourceName = "CodeConversion.PowerShell.GetBoundParameters.ps1";

            string getBoundParameterScript;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                getBoundParameterScript = reader.ReadToEnd();
            }

            resourceName = $"CodeConversion.PowerShell.ProxyCommands.{commandName}.ps1";
            string proxyCommand = null;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        proxyCommand = reader.ReadToEnd();
                    }
                }
            }

            try
            {
                var result = new Dictionary<string, object>();
                using (var powerShell = System.Management.Automation.PowerShell.Create())
                {
                    powerShell.AddScript(getBoundParameterScript);
                    powerShell.Invoke();
                    powerShell.AddCommand("Get-BoundParameter");
                    powerShell.AddParameter("commandAst", commandAst);

                    if (proxyCommand != null)
                        powerShell.AddParameter("proxyCommand", proxyCommand);

                    var psobject = powerShell.Invoke();
                    if (powerShell.HadErrors)
                    {
                        return null;
                    }

                    foreach (var param in psobject.Select(m => m.BaseObject).Cast<Hashtable>())
                    {
                        result.Add(param["Name"] as string, param["Ast"]);
                    }
                }

                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}
