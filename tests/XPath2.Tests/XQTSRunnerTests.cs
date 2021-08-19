#if NET5_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Wmhelp.XPath2;
using XPath2.TestRunner;
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

            Console.WriteLine("CultureInfo.InvariantCulture = {0}", CultureInfo.InvariantCulture);
            Console.WriteLine("CultureInfo.InvariantCulture.Name = {0}", CultureInfo.InvariantCulture.Name);
            Console.WriteLine("CultureInfo.InvariantCulture.CultureTypes = {0}", CultureInfo.InvariantCulture.CultureTypes);
            Console.WriteLine("CultureInfo.InvariantCulture.DisplayName = {0}", CultureInfo.InvariantCulture.DisplayName);
            Console.WriteLine("CultureInfo.InvariantCulture.TwoLetterISOLanguageName = {0}", CultureInfo.InvariantCulture.TwoLetterISOLanguageName);
            Console.WriteLine("CultureInfo.InvariantCulture.ThreeLetterISOLanguageName = {0}", CultureInfo.InvariantCulture.ThreeLetterISOLanguageName);

            Console.WriteLine("CurrentCulture   = {0}", Thread.CurrentThread.CurrentCulture);
            Console.WriteLine("CurrentUICulture = {0}", Thread.CurrentThread.CurrentUICulture);

            var kelvinSign = "K";
            Console.WriteLine("{0} - {1}=>ToLower={2} - {3}=>ToLowerInvariant={4}",
                kelvinSign,
                kelvinSign.ToLower(), kelvinSign.ToLower() == "k",
                kelvinSign.ToLowerInvariant(), kelvinSign.ToLowerInvariant() == "k");
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
            // result.Passed.Should().Be(12958);

            var passed = File.ReadAllLines(_passedPath).Where(line => !string.IsNullOrEmpty(line));
            var differences = _expectedPassed.Except(passed);
            differences.Should().BeEmpty();
        }
    }
}
#endif