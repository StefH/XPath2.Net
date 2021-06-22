using System.Xml;
using FluentAssertions;
using Wmhelp.XPath2;
using Xunit;

namespace XPath2.Tests
{
    public class XPath2NodeIteratorTests
    {
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

        private XmlDocument GetXHTMLSampleDoc()
        {
            string xml = "<start>"
                     + "<node1>Value1</node1>"
                     + "<node2>"
                     + "<subnode1>Value2</subnode1>"
                     + "</node2>"
                     + "</start>";

            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc;
        }
    }
}