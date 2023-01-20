using System.Xml;
using FluentAssertions;
using Wmhelp.XPath2;
using Xunit;

namespace XPath2.Tests;

public class XmlDocumentTests
{
    // Issue #58
    [Theory]
    [InlineData("max(/R/A/number(@id))")]
    [InlineData("max(/R/A/@id)")]
    public void XmlDocument_XPath2Evaluate_MaxFromNumber(string xpath)
    {
        // Arrange
        var doc = GetIssue58();

        // Act
        var result = doc.XPath2Evaluate(xpath);

        // Assert
        result.Should().Be(2);
    }

    // Issue #58
    [Theory]
    [InlineData("/R/A[@id=2]")]
    [InlineData("/R/A[@id='2']")]
    [InlineData("/R/A[@id=\"2\"]")]
    public void XmlDocument_SelectNodes_By_Attribute(string xpath)
    {
        // Arrange
        var doc = GetIssue58();

        // Act
        var result = doc.SelectNodes(xpath);

        // Assert
        result.Should().HaveCount(1);
        result![0].OuterXml.Should().Be("<A id=\"2\" />");
    }

    // Issue #58
    [Fact(Skip = "System.Xml.XPath.XPathException : Namespace Manager or XsltContext needed. This query has a prefix, variable, or user-defined function.")]
    public void XmlDocument_SelectNodes_By_AttributeEqualsMaxId_Version1()
    {
        // Arrange
        var doc = GetIssue58();

        // Act
        var result = doc.SelectNodes("/R/A[@id=max(/R/A/@id)]");

        // Assert
        result.Should().HaveCount(1);
        result![0].OuterXml.Should().Be("<A id=\"2\" />");
    }

    // Issue #58
    [Fact]
    public void XmlDocument_SelectNodes_By_AttributeEqualsMaxId_Version2()
    {
        // Arrange
        var doc = GetIssue58();

        // Act
        var result = doc.SelectNodes("/R/A[not(/R/A/@id > @id)]");

        // Assert
        result.Should().HaveCount(1);
        result![0].OuterXml.Should().Be("<A id=\"2\" />");
    }

    private static XmlDocument GetIssue58()
    {
        return new XmlDocument
        {
            InnerXml = @"<R><A id=""1""/><A id=""2""/></R>"
        };
    }
}