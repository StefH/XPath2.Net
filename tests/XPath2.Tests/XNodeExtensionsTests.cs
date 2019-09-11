using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using Wmhelp.XPath2;
using Xunit;

namespace XPath2.Tests
{
    public class XNodeExtensionsTests
    {
        [Fact]
        public void Given_MultipleElements_XPath2Select_ShouldReturnAllMatches()
        {
            // Arrange
            const string xml = @"
            <pictures>
                <picture>
                    <picture_url>url0</picture_url>
                </picture>
                <picture>
                    <picture_url>url1</picture_url>
                </picture>
                <picture>
                    <picture_url>url2</picture_url>
                </picture>
            </pictures>";

            // Act
            XContainer c = XDocument.Parse(xml);
            var result = c.XPath2Select("//pictures/picture/picture_url").Cast<XElement>().ToArray();

            // Assert
            result[0].Value.Should().Be("url0");
            result[1].Value.Should().Be("url1");
            result[2].Value.Should().Be("url2");
        }
    }
}