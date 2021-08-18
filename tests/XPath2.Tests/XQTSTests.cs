#if NET5_0_OR_GREATER
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

        /// <summary>
        /// Call of matches() with "i" flag and Kelvin sign.
        /// 
        /// A character range (production charRange in the XSD 1.0 grammar, replaced by productions charRange and singleChar in XSD 1.1) represents the set containing all the characters that it would match in the absence of the "i" flag, together with their case-variants. For example, the regular expression "[A-Z]" will match all the letters A-Z and all the letters a-z. It will also match certain other characters such as #x212A (KELVIN SIGN), since fn:lower-case("#x212A") is "k".
        /// This rule applies also to a character range used in a character class subtraction (charClassSub): thus[A - Z -[IO]] will match characters such as "A", "B", "a", and "b", but will not match "I", "O", "i", or "o".
        /// </summary>
        [Fact]
        public void XPath2Evaluate_Functions_AllStringFunc_MatchStringFunc_MatchesFunc_caselessmatch04()
        {
            using (new FakeLocalTimeZone(TimeZoneInfo.Utc))
            {
                var nav = new XmlDocument().CreateNavigator();
                var result = nav.XPath2Evaluate("matches('&#x212A;', '[A-Z]', 'i') (: Kelvin sign :)");

                result.Should().Be(true);
            }
        }
    }
}
#endif