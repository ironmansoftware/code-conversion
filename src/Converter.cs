using CodeConverter.Common;
using CodeConverter.CSharp;
using CodeConverter.PowerShell;
using PowerShellToolsPro.CodeConversion.PowerShell;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeConverter
{
    /// <summary>
    /// Factory for converting different types of languages.
    /// </summary>
    public class Converter
    {
        private IEnumerable<ISyntaxTreeVisitor> _parsers;
        private IEnumerable<CodeWriter> _codeWriters;

        public Converter()
        {
            _parsers = new List<ISyntaxTreeVisitor>
            {
                new CSharpSyntaxTreeVisitor(),
				new PowerShellSyntaxTreeVisitor()
            };

            _codeWriters = new List<CodeWriter>
            {
                new PowerShellCodeWriter(),
                new PowerShell5CodeWriter(),
                new CSharpCodeWriter()
            };
        }

        public string Convert(string code, Language to)
        {
            var fromLanguage = to == Language.CSharp ? Language.PowerShell : Language.CSharp;

            //TODO: If there are ever more than 2 languages we can support this
            //var fromLanguage = DetectLanguage(code);
            //if (fromLanguage == Language.Unknown)
            //{
            //    return null;
            //}

            return Convert(code, fromLanguage, to);
        }

        /// <summary>
        /// Convert from one language to another. This method will throw an exception if the conversion is not possible.
        /// </summary>
        /// <param name="code">Source language code.</param>
        /// <param name="from">Source language</param>
        /// <param name="to">Target langauge</param>
        /// <returns></returns>
        public string Convert(string code, Language from, Language to)
        {
            var parser = _parsers.FirstOrDefault(m => m.Language == from);
            if (parser == null)
            {
                throw new NotImplementedException($"Parser for {Enum.GetName(typeof(Language), from)} is not implemented. Submit an issue on GitHub: https://github.com/codeconverter/sdk");
            }

            var writer = _codeWriters.FirstOrDefault(m => m.Language == to);
            if (writer == null)
            {
                throw new NotImplementedException($"Code writer for {Enum.GetName(typeof(Language), to)} is not implemented. Submit an issue on GitHub: https://github.com/codeconverter/sdk");
            }

            var ast = parser.Visit(code);
            return writer.Write(ast);
        }

        public Language DetectLanguage(string code)
        {
            foreach (var parser in _parsers)
            {
                try
                {
                    parser.Visit(code);
                }
                catch
                {
                    continue;
                }

                return parser.Language;
            }

            return Language.Unknown;
        }
    }
}
