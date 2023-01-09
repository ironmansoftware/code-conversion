
using System.IO;
using CodeConverter;
using CommandLine;
namespace CodeConversion
{
    public class Options
    {
        [Option('i', "InputFile", Required = true, HelpText = "File to convert.", SetName = "File")]
        public string InputFile { get; set; }

        [Option('o', "OutputPath", Required = true, HelpText = "Output file.", SetName = "File")]
        public string OutputFile { get; set; }

        [Option("PowerShell", Required = true, HelpText = "PowerShell script block to convert to C#", SetName = "PowerShell")]
        public string PowerShell { get; set; }

        [Option("CSharp", Required = true, HelpText = "C# code to convert to PowerShell", SetName = "CSharp")]
        public string CSharp { get; set; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                var converter = new Converter();

                if (!string.IsNullOrEmpty(o.InputFile))
                {
                    var output = o.OutputFile;
                    var fileInfo = new FileInfo(o.InputFile);
                    var contents = File.ReadAllText(o.InputFile);
                    var codeConverter = fileInfo.Extension == ".cs" ? CodeConverter.Common.Language.PowerShell : CodeConverter.Common.Language.CSharp;
                    var result = converter.Convert(contents, codeConverter);
                    File.WriteAllText(o.OutputFile, result);
                }
                else if (!string.IsNullOrEmpty(o.PowerShell))
                {
                    var result = converter.Convert(o.PowerShell, CodeConverter.Common.Language.CSharp);
                    System.Console.WriteLine(result);
                }
                else if (!string.IsNullOrEmpty(o.CSharp))
                {
                    var result = converter.Convert(o.CSharp, CodeConverter.Common.Language.PowerShell);
                    System.Console.WriteLine(result);
                }
            });
        }
    }
}