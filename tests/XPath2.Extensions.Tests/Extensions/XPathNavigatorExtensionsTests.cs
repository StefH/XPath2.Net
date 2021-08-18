using System.Xml;
using Wmhelp.XPath2;
using Xunit;

namespace XPath2.Tests.Extensions
{
    [Collection("Sequential")]
    public class XPathNavigatorExtensionsTests
    {
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

        [Fact]
        public void XPath2Select_WithVariable_Works()
        {
            var nav = GetOrders().CreateNavigator();
            var nodeIt = nav.XPath2Select("//item[@quantity = $q]", new { q = 1 });

            Assert.Equal(5, nodeIt.Count);
        }
    }
}
