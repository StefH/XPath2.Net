#if NET5_0_OR_GREATER
using System;
using System.IO;
using FluentAssertions;
using XPath2.TestRunner;
using Xunit;

namespace XPath2.Tests
{
    public class XQTSRunnerTests
    {
        const string uri = "https://github.com/StefH/XML-Query-Test-Suite-1.0/blob/main/XQTS_1_0_2.zip?raw=true";

        private readonly XQTSRunner _runner;

        public XQTSRunnerTests()
        {
            var passedWriter = TextWriter.Synchronized(new StreamWriter(Path.Combine(Environment.CurrentDirectory, "passed.txt")));
            var errorWriter = TextWriter.Synchronized(new StreamWriter(Path.Combine(Environment.CurrentDirectory, "error.txt")));

            var error = Path.Combine(Environment.CurrentDirectory, "error.txt");
            _runner = new XQTSRunner(Console.Out, passedWriter, errorWriter);
        }

        [Fact]
        public void Run()
        {
            // Arrange
            var parameter = $"{uri}|{Environment.CurrentDirectory}";

            // Act
            var result = _runner.Run(parameter, RunType.Sequential);

            // Assert
            result.Total.Should().Be(15133);
            result.Passed.Should().Be(12958);
        }
    }
}
#endif