using System.Xml;
using FluentAssertions;
using Wmhelp.XPath2;
using Xunit;

namespace XPath2.Tests
{
    public class XPath2NodeIteratorTests
    {
        [Fact]
        public void ToString_With_NoValue_Should_Return_Empty()
        {
            // Arrange
            var doc = GetXHTMLSampleDoc();
            var navigator = doc.CreateNavigator();

            // Act
            var nodeIterator = (XPath2NodeIterator)navigator.XPath2Evaluate("/start/node2/subnodeX");
            var text = nodeIterator.ToString();

            // Assert
            text.Should().Be("<empty>");
        }

        [Fact]
        public void ToString_With_SingleValue_Should_Return_SingleStringValue()
        {
            // Arrange
            var doc = GetXHTMLSampleDoc();
            var navigator = doc.CreateNavigator();

            // Act
            var nodeIterator = (XPath2NodeIterator) navigator.XPath2Evaluate("/start/node2/subnode1");
            var text = nodeIterator.ToString();

            // Assert
            text.Should().Be("Value2");
        }

        [Fact]
        public void ToString_With_MultieValue_Should_Return_StringValueCommaSeparated()
        {
            // Arrange
            var doc = GetXHTMLSampleDoc();
            var navigator = doc.CreateNavigator();

            // Act
            var nodeIterator = (XPath2NodeIterator)navigator.XPath2Evaluate("/start/node2/subnode2");
            var text = nodeIterator.ToString();

            // Assert
            text.Should().Be("Value3a, Value3b");
        }

        private XmlDocument GetXHTMLSampleDoc()
        {
            string xml = "<start>"
                     + "<node1>Value1</node1>"
                     + "<node2>"
                     + "<subnode1>Value2</subnode1>"
                     + "<subnode2>Value3a</subnode2>"
                     + "<subnode2>Value3b</subnode2>"
                     + "</node2>"
                     + "</start>";

            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc;
        }
    }
}