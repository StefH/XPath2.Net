using System.Xml;
using System.Xml.XPath;
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
        public void XPath2Evaluate_number()
        {
            var xml = new XmlDocument
            {
                InnerXml = @"
                    <root num=' 123 '>
                        <numeric>456.78
                        </numeric>
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
    }
}