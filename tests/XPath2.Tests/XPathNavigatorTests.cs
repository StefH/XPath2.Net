using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using FluentAssertions;
using Wmhelp.XPath2;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace XPath2.Tests;

[Collection("Sequential")]
public class XPathNavigatorTests
{
    private readonly XPathNavigator _navigator;

    public XPathNavigatorTests()
    {
        var doc = new XmlDocument();
        _navigator = doc.CreateNavigator()!;
    }

    private XmlDocument GetTodoListDoc()
    {
        return new XmlDocument
        {
            InnerXml = @"
                    <todo-list>
                        <todo-item id='a1'>abc</todo-item>
                        <todo-item id='a2'>def</todo-item>
                        <todo-item id='a3'>xyz</todo-item>
                    </todo-list>"
        };
    }

    private XmlDocument GetXHTMLSampleDoc()
    {
        var xhtml = @"<html xmlns='http://www.w3.org/1999/xhtml' lang='en' xml:lang='en'>
<head>
  <title>Example</title>
</head>
<body>
  <h1>Example</h1>
  <p>This is paragraph 1.</p>
  <p>This is paragraph 2.</p>
</body>
</html>";
        var doc = new XmlDocument();
        doc.LoadXml(xhtml);
        return doc;
    }

    //public void O()
    //{
    //    string q = @"
    //        for $d in distinct-values(doc(""order.xml"")//item/@dept)
    //        let $items := doc(""order.xml"")//item[@dept = $d]
    //        order by $d
    //        return <department code=""{$d}"">{
    //                 for $i in $items
    //                 order by $i/@num
    //                 return $i
    //               }</department>";
    //}

    [Fact]
    public void XPath2Evaluate_true()
    {
        var nav = GetTodoListDoc().CreateNavigator();
        var result = nav.XPath2Evaluate("boolean(/todo-list[count(todo-item) = 3])");

        Assert.True(true.Equals(result));
    }

    [Fact]
    public void XPath2Evaluate_false()
    {
        var nav = GetTodoListDoc().CreateNavigator();
        var result = nav.XPath2Evaluate("boolean(/todo-list[count(todo-item) = 4])");

        Assert.False(true.Equals(result));
    }

    [Fact]
    public void XPath2Evaluate_matches_true()
    {
        var result = _navigator.XPath2Evaluate("matches(\"abracadabra\", \"bra\")");

        Assert.Equal(true, result);
    }

    [Fact]
    public void XPath2Evaluate_matches_IgnoreCase_true()
    {
        var result = _navigator.XPath2Evaluate("matches(\"abracadabra\", \"BRa\", \"i\")");

        Assert.Equal(true, result);
    }

    [Fact]
    public void XPath2Evaluate_matches_false()
    {
        var result = _navigator.XPath2Evaluate("matches(\"abracadabra\", \"test\")");

        Assert.Equal(false, result);
    }

    [Fact]
    public void XPath2Evaluate_substring()
    {
        var result = _navigator.XPath2Evaluate("substring(null, 2)");

        Assert.Equal("", result);
    }

    [Fact]
    public void XPath2Evaluate_string_With_0_Arguments()
    {
        var result = _navigator.XPath2Evaluate("string()");

        Assert.Equal("", result);
    }

    [Fact]
    public void XPath2Evaluate_string_With_1_StringArgument()
    {
        var result = _navigator.XPath2Evaluate("string(\"x\")");

        Assert.Equal("x", result);
    }

    [Fact]
    public void XPath2Evaluate_string_With_1_IntegerArgument()
    {
        var result = _navigator.XPath2Evaluate("string(1)");

        Assert.Equal("1", result);
    }

    [Fact]
    public void XPath2Evaluate_string_On_Context_Node()
    {
        var doc = new XmlDocument { InnerXml = "<root>foo</root>" };

        var result = doc.XPath2Evaluate("string(root/string())") as string;
        Assert.Equal("foo", result);
    }

    [Fact]
    public void XPath2Evaluate_string_join_On_string_sequence()
    {
        // Arrange
        var doc = new XmlDocument { InnerXml = "<root><item>a</item><item>b</item><item>c</item></root>" };

        // Act
        var result = doc.XPath2Evaluate("string-join(root/item/string(), ',')") as string;

        // Assert
        Assert.Equal("a,b,c", result);
    }

    [Fact]
    public void XPath2Evaluate_number()
    {
        var xml = new XmlDocument
        {
            InnerXml = @"
                    <root num=' 123 '>
                        <numeric>456.78</numeric>
                    </root>"
        };

        Assert.Equal(123.0, xml.XPath2Evaluate("number(/root/@num)"));
        Assert.Equal(456.78, xml.XPath2Evaluate("number(/root/numeric)"));
    }

    [Fact]
    public void XPath2Evaluate_Arithmetic_With_DecimalConversion()
    {
        var xml1 = new XmlDocument { InnerXml = "<total taxableAmount=\"1.23\" taxAmount=\"7.89\" netAmountIncTax=\"9.12\" />" };
        var xml2 = new XmlDocument { InnerXml = "<total taxableAmount=\"1441.64\" taxAmount=\"273.91\" netAmountIncTax=\"1715.55\" />" };

        var add1 = xml1.XPath2Evaluate("/total/@taxableAmount + /total/@taxAmount");
        var equal1 = xml1.XPath2Evaluate("/total/@taxableAmount + /total/@taxAmount = /total/@netAmountIncTax");

        var add2 = xml2.XPath2Evaluate("xs:decimal(/total/@taxableAmount + /total/@taxAmount)");
        var equal2 = xml2.XPath2Evaluate("xs:decimal(/total/@taxableAmount + /total/@taxAmount) = xs:decimal(/total/@netAmountIncTax)");

        Assert.Equal(9.12, add1);
        Assert.Equal(true, equal1);

        Assert.Equal((decimal)1715.55, add2);
        Assert.Equal(true, equal2);
    }

    [Fact]
    public void TestDocumentNodeElementSelection()
    {
        var doc = GetTodoListDoc();

        var result1 = doc.XPath2SelectNodes("self::document-node(element(todo-list))");
        var result2 = doc.XPath2SelectNodes("self::document-node()");

        Assert.Equal(result2.Count, result1.Count);
    }

    [Fact]
    public void TestDescendantOrSelfDocumentNodeElementSelection()
    {
        var doc = GetTodoListDoc();

        var result1 = doc.XPath2SelectNodes("descendant-or-self::document-node(element(todo-list))");
        var result2 = doc.XPath2SelectNodes("descendant-or-self::document-node()");

        Assert.Equal(result2.Count, result1.Count);
    }

    [Fact]
    public void TestNoDocumentNodeOnChildAxis()
    {
        var doc = GetTodoListDoc();

        var result1 = doc.XPath2SelectNodes("/document-node(element(todo-list))");
        var result2 = doc.XPath2SelectNodes("/document-node()");

        Assert.Equal(result2.Count, result1.Count);
    }

    [Fact]
    public void TestDocumentNodeOnAncestorAxis()
    {
        var doc = GetTodoListDoc();

        var item1 = doc.XPath2SelectSingleNode("todo-list/todo-item");

        var result1 = item1.XPath2SelectNodes("ancestor::document-node(element(todo-list))");
        var result2 = item1.XPath2SelectNodes("ancestor::document-node()");

        Assert.Equal(result2.Count, result1.Count);
    }

    [Fact]
    public void XPath2SelectNodesWithDefaultNamespace()
    {
        var namespaceManager = new XmlNamespaceManager(new NameTable());
        namespaceManager.AddNamespace(string.Empty, "http://www.w3.org/1999/xhtml");

        var nodeList = GetXHTMLSampleDoc().XPath2SelectNodes("//p", namespaceManager);

        Assert.Equal(2, nodeList.Count);
    }

    [Fact]
    public void XPath2SelectNodesWithDefaultNamespaceElementTest()
    {
        var namespaceManager = new XmlNamespaceManager(new NameTable());
        namespaceManager.AddNamespace(string.Empty, "http://www.w3.org/1999/xhtml");

        var nodeList = GetXHTMLSampleDoc().XPath2SelectNodes("//element(p)", namespaceManager);

        Assert.Equal(2, nodeList.Count);
    }

    [Fact]
    public void XPath2SelectNodesWithPrefixForNamespace()
    {
        var namespaceManager = new XmlNamespaceManager(new NameTable());
        namespaceManager.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");

        var nodeList = GetXHTMLSampleDoc().XPath2SelectNodes("//xhtml:p", namespaceManager);

        Assert.Equal(2, nodeList.Count);
    }

    [Fact]
    public void XPath2SelectNodesWithPrefixForNamespaceElementTest()
    {
        var namespaceManager = new XmlNamespaceManager(new NameTable());
        namespaceManager.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");

        var nodeList = GetXHTMLSampleDoc().XPath2SelectNodes("//element(xhtml:p)", namespaceManager);

        Assert.Equal(2, nodeList.Count);
    }

    [Fact]
    public void BindingEmptyPrefixShouldNotBreakFunctionLookup()
    {
        var todoList = GetTodoListDoc();

        var namespaceManager = new XmlNamespaceManager(new NameTable());
        namespaceManager.AddNamespace(string.Empty, "http://example.com/ns1");

        var result = todoList.XPath2Evaluate("count(/*/*)", namespaceManager);

        Assert.Equal(3, result);
    }

    // https://www.w3.org/TR/xpath-functions/#func-round
    [Theory]
    [InlineData("2.51", 3)]
    [InlineData("2.5", 3)]
    [InlineData("2.4999", 2)]
    [InlineData("+0", 0)]
    [InlineData("0", 0)]
    [InlineData("-0", 0)]
    [InlineData("-2.4999", -2)]
    [InlineData("-2.5", -2)]
    [InlineData("-2.51", -3)]
    [InlineData("-3.5", -3)]
    public void XPath2Evaluate_round(string value, object expected)
    {
        // Arrange
        string xpath = "round(number(value))";
        var xml = new XmlDocument { InnerXml = $"<value>{value}</value>" };
        var nav = xml.CreateNavigator()!;

        // Act
        var result = nav.XPath2Evaluate(xpath);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("INF", double.PositiveInfinity)]
    [InlineData("-INF", double.NegativeInfinity)]
    [InlineData("NaN", double.NaN)]
    public void XPath2Evaluate_round_EdgeCases(string value, object expected)
    {
        // Arrange
        string xpath = "round(xs:double(value))";
        var xml = new XmlDocument { InnerXml = $"<value>{value}</value>" };
        var nav = xml.CreateNavigator()!;

        // Act
        var result = nav.XPath2Evaluate(xpath);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void XPath2Evaluate_AttributeSelection()
    {
        var nav = GetTodoListDoc().CreateNavigator()!;
        var result1 = nav.XPath2Evaluate("count(//@id)");
        var result2 = nav.XPath2Evaluate("count(//attribute::attribute(id))");

        result1.Should().Be(3);
        result2.Should().Be(3);
    }

    [Fact]
    public void XPath2Evaluate_FullPathWithAttributeSelection()
    {
        var nav = GetTodoListDoc().CreateNavigator()!;
        var result1 = nav.XPath2Evaluate("count(/todo-list/todo-item/@id)");
        var result2 = nav.XPath2Evaluate("count(/todo-list/todo-item/attribute::attribute(id))");

        result1.Should().Be(3);
        result2.Should().Be(3);
    }

    /// <summary>
    /// - https://learn.microsoft.com/en-us/dotnet/api/system.xml.xpath.xpathnodeiterator.count?view=net-7.0
    /// - Count = Gets the index of the last node in the selected set of nodes.
    /// </summary>
    [Fact]
    public void Issue59_XDocumentParse_XPath2SelectNodes_Count_Returns_TheIndexOfTheLastNode()
    {
        // Arrange
        var xml = GetXml();

        var navigator = XDocument.Parse(xml).CreateNavigator();

        // Act 1
        var brandSet = navigator.XPath2SelectNodes("/report/brand");

        // Assert 1
        brandSet.Count.Should().Be(4);

        // Act 2
        var brandCount = (int)navigator.XPath2Evaluate("count(/report/brand)");

        // Assert 2
        brandCount.Should().Be(5);
        
        // Act 3
        var highVolumeBrandSet = navigator.XPath2SelectNodes("/report/brand[units > 20000]");

        // Assert 3
        highVolumeBrandSet.Count.Should().Be(1);

        // Act 4
        var highVolumeBrandCount = (int)navigator.XPath2Evaluate("count(/report/brand[units > 20000])");

        // Assert 4
        highVolumeBrandCount.Should().Be(2);
    }

    [Fact]
    public void Issue59_XDocumentParse_XPath2SelectNodes_CountMethod_Returns_Number_Of_Items_In_Expression()
    {
        // Arrange
        var xml = GetXml();

        var navigator = XDocument.Parse(xml).CreateNavigator();

        // Act 1
        var brandSet = navigator.XPath2SelectNodes("/report/brand");
        var brandCount = (int)navigator.XPath2Evaluate("count(/report/brand)");

        // Act 2
        var highVolumeBrandSet = navigator.XPath2SelectNodes("/report/brand[units > 20000]");
        var highVolumeBrandCount = (int)navigator.XPath2Evaluate("count(/report/brand[units > 20000])");

        // Assert 1
        brandCount.Should().Be(5);
        brandSet.Should().HaveCount(5);

        // Assert 2
        highVolumeBrandCount.Should().Be(2);
        highVolumeBrandSet.Should().HaveCount(2);
    }

    [Fact]
    public void Issue59_XmlDocumentInnerXml_XPath2SelectNodes_CountMethod_Returns_Number_Of_Items_In_Expression()
    {
        // Arrange
        var doc = new XmlDocument { InnerXml = GetXml() };

        var navigator = doc.CreateNavigator()!;

        // Act 1
        var brandSet = navigator.XPath2SelectNodes("/report/brand");
        var brandCount = (int)navigator.XPath2Evaluate("count(/report/brand)");

        // Act 2
        var highVolumeBrandSet = navigator.XPath2SelectNodes("/report/brand[units > 20000]");
        var highVolumeBrandCount = (int)navigator.XPath2Evaluate("count(/report/brand[units > 20000])");

        // Assert 1
        brandCount.Should().Be(5);
        brandSet.Should().HaveCount(5);

        // Assert 2
        highVolumeBrandCount.Should().Be(2);
        highVolumeBrandSet.Should().HaveCount(2);
    }

    //var doc = new XmlDocument { InnerXml = "<root>foo</root>" };

    private string GetXml() => @"<?xml version=""1.0"" encoding=""utf-8""?>
<!-- chocolate.xml -->
<report month=""8"" year=""2006"">
    <title>Chocolate bar sales</title>
    <brand>
        <name>a</name>
        <units>27408</units>
    </brand>
    <brand>
        <name>b</name>
        <units>8203</units>
    </brand>
    <brand>
        <name>c</name>
        <units>22101</units>
    </brand>
    <brand>
        <name>d</name>
        <units>14336</units>
    </brand>
    <brand>
        <name>e</name>
        <units>19268</units>
    </brand>
</report>";
}