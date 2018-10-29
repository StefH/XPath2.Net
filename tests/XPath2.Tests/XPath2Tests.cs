using System.Xml;
using Wmhelp.XPath2;
using Xunit;

namespace XPath2.Tests
{
    [Collection("xpath2 collection")]
    public class XPath2Tests
    {
        private readonly XPath2TestFixture _fixture;

        public XPath2Tests(XPath2TestFixture fixture)
        {
            _fixture = fixture;
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

        private XmlDocument GetOrders()
        {
            return new XmlDocument
            {
                InnerXml = @"
                    <order num='00299432' date='2015-09-15' cust='0221A'>
                        <item dept='WMN' num='557' quantity='1' color='navy'/>
                        <item dept='ACC' num='563' quantity='1'/>
                        <item dept='ACC' num='443' quantity='2'/>
                        <item dept='MEN' num='784' quantity='1' color='white'/>
                        <item dept='MEN' num='784' quantity='1' color='gray'/>
                        <item dept='WMN' num='557' quantity='1' color='black'/>
                    </order>"
            };
        }

        public void O()
        {
            string q = @"
                for $d in distinct-values(doc(""order.xml"")//item/@dept)
                let $items := doc(""order.xml"")//item[@dept = $d]
                order by $d
                return <department code=""{$d}"">{
                         for $i in $items
                         order by $i/@num
                         return $i
                       }</department>";
        }

        [Fact]
        public void XPath2_Evaluate_true()
        {
            var nav = GetTodoListDoc().CreateNavigator();
            var result = nav.XPath2Evaluate("boolean(/todo-list[count(todo-item) = 3])");

            Assert.True(true.Equals(result));
        }

        [Fact]
        public void XPath2_Evaluate_false()
        {
            var nav = GetTodoListDoc().CreateNavigator();
            var result = nav.XPath2Evaluate("boolean(/todo-list[count(todo-item) = 4])");

            Assert.False(true.Equals(result));
        }

        [Fact]
        public void XPath2_matches_true()
        {
            var result = _fixture.Navigator.XPath2Evaluate("matches(\"abracadabra\", \"bra\")");

            Assert.Equal(true, result);
        }

        [Fact]
        public void XPath2_matches_IgnoreCase_true()
        {
            var result = _fixture.Navigator.XPath2Evaluate("matches(\"abracadabra\", \"BRa\", \"i\")");

            Assert.Equal(true, result);
        }

        [Fact]
        public void XPath2_matches_false()
        {
            var result = _fixture.Navigator.XPath2Evaluate("matches(\"abracadabra\", \"test\")");

            Assert.Equal(false, result);
        }

        [Fact]
        public void XPath2_substring()
        {
            var result = _fixture.Navigator.XPath2Evaluate("substring(null, 2)");

            Assert.Equal("", result);
        }

        [Fact]
        public void XPath2_number()
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
    }
}