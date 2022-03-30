using CodeConverter.Common;
using CodeConverter.CSharp;
using CodeConverter.PowerShell;
using Newtonsoft.Json;
using PowerShellToolsPro.CodeConversion.PowerShell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace CodeConverter.Test
{
    public class LanguageTests : IClassFixture<Fixture>
	{
		private Fixture _fixture;
        public LanguageTests(Fixture fixture)
        {
			_fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(Fixture.TestData), MemberType = typeof(Fixture))]
        public void TestLanguages(ConversionTestCase testCase)
        {
            var source = ReadTestData(testCase.TestDirectory, testCase.Name, testCase.SyntaxTreeVisitor.Language);
            var target = ReadTestData(testCase.TestDirectory, testCase.Name, testCase.CodeWriter.Language);

            var ast = testCase.SyntaxTreeVisitor.Visit(source);
            var actual = testCase.CodeWriter.Write(ast).Trim();

            Assert.Equal(target, actual);

			var testResult = new TestCaseResult
			{
				Category = testCase.Category,
				Description = testCase.Description,
				Name = testCase.Name,
				Source = testCase.SyntaxTreeVisitor.Language,
				Target = testCase.CodeWriter.Language,
				SourceContent = source,
				TargetContent = target
			};

			_fixture.AddTestCase(testResult);
        }

        private string GetLanguageExtension(Language language)
        {
            switch (language)
            {
                case Language.CSharp:
                    return ".cs";
                case Language.PowerShell:
                    return ".ps1";
                case Language.PowerShell5:
                    return ".ps1";
            }

            throw new NotImplementedException();
        }

        private string ReadTestData(string testDirectory, string testName, Language language)
        {
            var languageName = Enum.GetName(typeof(Language), language);
            var extension = GetLanguageExtension(language);

            var testData = $"{testDirectory}\\{languageName}\\{testName}{extension}";

            if (!File.Exists(testData))
            {
                throw new Exception($"No test data found at {testData}");
            }

            return File.ReadAllText(testData);
        }
    }

    public class Fixture : IDisposable
    {
		private List<TestCaseResult> _results = new List<TestCaseResult>();

        public Fixture()
        {
			foreach (var syntaxTreeVisitor in SyntaxTreeVisitors)
			{
				foreach (var codeWriter in CodeWriters)
				{
					if (syntaxTreeVisitor.Language == codeWriter.Language)
					{
						continue;
					}

					var jsonPath = Path.Combine(GetTestDirectory(), @"..\..\..\..\..\testresults.json");
					File.Delete(jsonPath);
				}
			}
        }

		public void AddTestCase(TestCaseResult result)
		{
			_results.Add(result);
		}

        public void Dispose()
        {
			var json = JsonConvert.SerializeObject(_results);
			File.WriteAllText(Path.Combine(GetTestDirectory(), @"..\..\..\..\..\testresults.json"), json);

		}

        public static string GetTestDirectory()
        {
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            return Path.GetDirectoryName(codeBasePath);
        }

		public static IEnumerable<ISyntaxTreeVisitor> SyntaxTreeVisitors { get; private set; }
		public static IEnumerable<CodeWriter> CodeWriters { get; private set; }
		static Fixture()
		{
			SyntaxTreeVisitors = new ISyntaxTreeVisitor[]
			{
				new PowerShellSyntaxTreeVisitor(),
				new CSharpSyntaxTreeVisitor()
			};

			CodeWriters = new CodeWriter[]
			{
				new PowerShellCodeWriter(),
                new PowerShell5CodeWriter(),
                new CSharpCodeWriter()
			};

			_data = new List<object[]>();

			foreach (var syntaxTreeVisitor in SyntaxTreeVisitors)
			{
				foreach (var codeWriter in CodeWriters)
				{
					if (syntaxTreeVisitor.Language == codeWriter.Language)
					{
						continue;
					}

					var sourceLanguage = Enum.GetName(typeof(Language), syntaxTreeVisitor.Language);
					var targetLanguage = Enum.GetName(typeof(Language), codeWriter.Language);

					var testDirectory = Fixture.GetTestDirectory();
					testDirectory = Path.Combine(Path.Combine(testDirectory, "Languages"), sourceLanguage + "To" + targetLanguage);

                    if (!Directory.Exists(testDirectory))
                    {
                        continue;
                    }

					var testManifestFile = Path.Combine(testDirectory, "tests.json");
					var testManifestContent = File.ReadAllText(testManifestFile);
					var testManifest = JsonConvert.DeserializeObject<TestManifest>(testManifestContent);

					foreach (var testCase in testManifest.TestCases)
					{
						_data.Add(new[] { new ConversionTestCase(testCase.Name, testCase.Description, testCase.Category, syntaxTreeVisitor, codeWriter, testDirectory) });
					}
				}
			}
		}

		private static readonly List<object[]> _data;
		public static IEnumerable<object[]> TestData
		{
			get { return _data; }
		}
	}

    public class TestManifest
    {
        public Language Source { get; set; }
        public Language Target { get; set; }
        public List<TestCase> TestCases { get; set; }

    }

    public class TestCase
    {
        public string Name { get; set; }
        public string Description { get; set; }
		public string Category { get; set; }
    }

	public class TestCaseResult
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Category { get; set; }
		public string SourceContent { get; set; }
		public string TargetContent { get; set; }
		public Language Source { get; set; }
		public Language Target { get; set; }
	}

    public class ConversionTestCase : IXunitSerializable
    {
        public ConversionTestCase()
        {

        }
        public ConversionTestCase(string name, string description, string category, ISyntaxTreeVisitor syntaxTreeVisitor, CodeWriter codeWriter, string testDirectory)
        {
            Name = name;
            Description = description;
            SyntaxTreeVisitor = syntaxTreeVisitor;
            CodeWriter = codeWriter;
            TestDirectory = testDirectory;
			Category = category;
        }

        public string Name { get; set; }
        public string Description { get; set; }
		public string Category { get; set; }
        public ISyntaxTreeVisitor SyntaxTreeVisitor { get; set; }
        public CodeWriter CodeWriter { get; set; }
        public string TestDirectory { get; set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Name = info.GetValue<string>(nameof(Name));
            Description = info.GetValue<string>(nameof(Description));
			Category = info.GetValue<string>(nameof(Category));
			var type = Type.GetType(info.GetValue<string>(nameof(SyntaxTreeVisitor)));
            SyntaxTreeVisitor = Activator.CreateInstance(type) as ISyntaxTreeVisitor;
            type = Type.GetType(info.GetValue<string>(nameof(CodeWriter)));
            CodeWriter = Activator.CreateInstance(type) as CodeWriter;
            TestDirectory = info.GetValue<string>(nameof(TestDirectory));
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Name), Name);
            info.AddValue(nameof(Description), Description);
			info.AddValue(nameof(Category), Category);
			info.AddValue(nameof(SyntaxTreeVisitor), SyntaxTreeVisitor.GetType().AssemblyQualifiedName);
            info.AddValue(nameof(CodeWriter), CodeWriter.GetType().AssemblyQualifiedName);
            info.AddValue(nameof(TestDirectory), TestDirectory);
        }

        public override string ToString()
        {
			var sourceLanguage = Enum.GetName(typeof(Language), SyntaxTreeVisitor.Language);
			var targetLanguage = Enum.GetName(typeof(Language), CodeWriter.Language);

			return $"{sourceLanguage} -> {targetLanguage}: {Name}";
        }
    }
}
