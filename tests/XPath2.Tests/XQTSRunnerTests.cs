#if NET5_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Wmhelp.XPath2;
using XPath2.TestRunner;
using XPath2.TestRunner.Utils;
using Xunit;

namespace XPath2.Tests
{
    [Collection("Sequential")]
    public class XQTSRunnerTests
    {
        const string uri = "https://github.com/StefH/XML-Query-Test-Suite-1.0/blob/main/XQTS_1_0_2.zip?raw=true";

        private readonly string _passedPath = Path.Combine(Environment.CurrentDirectory, "passed.txt");
        private readonly List<string> _expectedPassed = new List<string>();

        public XQTSRunnerTests()
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("XPath2.Tests.Results.passed.txt");

            using var reader = new StreamReader(stream);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                _expectedPassed.Add(line);
            }
        }

        [Fact]
        public void Run()
        {
            // 1. CLear FunctionTable else the XPath2.Extensions tests will mess up this test (e.g. Expressions/PrimaryExpr/FunctionCallExpr/K-FunctionCallExpr-25.xqx)
            // 2. Also force all tests to run sequential ([Collection("Sequential")])
            FunctionTable.Clear();

            // Arrange
            var parameter = $"{uri}|{Environment.CurrentDirectory}";

            var passedWriter = TextWriter.Synchronized(new StreamWriter(_passedPath));
            var errorWriter = TextWriter.Synchronized(new StreamWriter(Path.Combine(Environment.CurrentDirectory, "error.txt")));

            var runner = new XQTSRunner(Console.Out, passedWriter, errorWriter);

            // Act
            var result = runner.Run(parameter, RunType.Sequential);

            passedWriter.Flush();
            passedWriter.Close();

            errorWriter.Flush();
            errorWriter.Close();

            // Assert
            result.Total.Should().Be(15133);

            var passed = File.ReadAllLines(_passedPath).Where(line => !string.IsNullOrEmpty(line));
            var differences = _expectedPassed.Except(passed);

            if (GlobalizationUtils.UseNls())
            {
                // https://stackoverflow.com/questions/68848852/tolowerinvariant-from-a-kelvin-sign-%e2%84%aa-in-c-sharp-has-different-results
                differences.Should().BeEquivalentTo("caselessmatch04", "caselessmatch05", "caselessmatch06", "caselessmatch07");
            }
            else
            {
                differences.Should().BeEmpty();
            }
        }
    }
}
#endif