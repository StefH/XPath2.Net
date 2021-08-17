using System.Xml;
using FluentAssertions;
using Wmhelp.XPath2;
using Xunit;

namespace XPath2.Tests
{
    public class XQTSTests
    {
        [Fact]
        public void XPath2Evaluate_AdjDateTimeToTimezoneFunc_6()
        {
            var nav = new XmlDocument().CreateNavigator();
            var result2 = nav.XPath2Evaluate("timezone-from-dateTime(adjust-dateTime-to-timezone(xs:dateTime(\"2001-02-03T00:00:00\"))) eq implicit-timezone()");

            result2.Should().Be(true);
        }
    }
}