using System.Xml;
using System.Xml.XPath;
using FluentAssertions;
using Wmhelp.XPath2;
using Xunit;

namespace XPath2.Tests
{
    public class XPathNavigatorTests
    {
        private readonly XPathNavigator _navigator;

        public XPathNavigatorTests()
        {
            var doc = new XmlDocument();
            _navigator = doc.CreateNavigator();
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

            string result = doc.XPath2Evaluate("string(root/string())") as string;
            Assert.Equal("foo", result);
        }

        [Fact]
        public void XPath2Evaluate_string_join_On_string_sequence()
        {
            var doc = new XmlDocument { InnerXml = "<root><item>a</item><item>b</item><item>c</item></root>" };

            string result = doc.XPath2Evaluate("string-join(root/item/string(), ',')") as string;

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
            var nav = xml.CreateNavigator();

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
            var nav = xml.CreateNavigator();

            // Act
            var result = nav.XPath2Evaluate(xpath);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void XPath2Evaluate_AttributeSelection()
        {
            var nav = GetTodoListDoc().CreateNavigator();
            var result1 = nav.XPath2Evaluate("count(//@id)");
            var result2 = nav.XPath2Evaluate("count(//attribute::attribute(id))");

            result1.Should().Be(3);
            result2.Should().Be(3);
        }

        [Fact]
        public void XPath2Evaluate_FullPathWithAttributeSelection()
        {
            var nav = GetTodoListDoc().CreateNavigator();
            var result1 = nav.XPath2Evaluate("count(/todo-list/todo-item/@id)");
            var result2 = nav.XPath2Evaluate("count(/todo-list/todo-item/attribute::attribute(id))");

            result1.Should().Be(3);
            result2.Should().Be(3);
        }
    }
}