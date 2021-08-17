using System;
using System.Xml;
using FluentAssertions;
using Wmhelp.XPath2;
using XPath2.TestRunner.Utils;
using Xunit;

namespace XPath2.Tests
{
    public class XQTSTests
    {
        /// <summary>
        /// Test that the implicit timezone in the dynamic context is used if $timezone is empty; indirectly also tests context stability.
        /// </summary>
        [Fact]
        public void XPath2Evaluate_AdjDateTimeToTimezoneFunc_6()
        {
            using (new FakeLocalTimeZone(TimeZoneInfo.Utc))
            {
                var nav = new XmlDocument().CreateNavigator();
                var result = nav.XPath2Evaluate("timezone-from-dateTime(adjust-dateTime-to-timezone(xs:dateTime(\"2001-02-03T00:00:00\"))) eq implicit-timezone()");

                result.Should().Be(true);
            }
        }
    }
}