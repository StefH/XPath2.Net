using FluentAssertions;
using Wmhelp.XPath2;
using Xunit;

namespace XPath2.Tests;

[Collection("Sequential")]
public class CoreFuncsTests
{
    [Theory]
    //[InlineData("&nbsp;", " ")]
    [InlineData("&lt;", "<")]
    [InlineData("&gt;", ">")]
    [InlineData("&amp;", "&")]
    [InlineData("&quot;", "\"")]
    [InlineData("&apos;", "'")]
    //[InlineData("&cent;", "¢")]
    //[InlineData("&pound;", "£")]
    //[InlineData("&yen;", "¥")]
    //[InlineData("&euro;", "€")]
    //[InlineData("&copy;", "©")]
    //[InlineData("&reg;", "®")]
    public void NormalizeStringValue_With_HtmlEncodedValue_Should_Return_UnencodedStringValue(string encoded, string expected)
    {
        // Act
        var result = CoreFuncs.NormalizeStringValue(encoded, false, true);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("a&#768;", "à")]
    [InlineData("O&#771;", "Õ")]
    public void NormalizeStringValue_With_Diacritical_Should_Return_UnencodedStringValue(string encoded, string expected)
    {
        // Act
        var result = CoreFuncs.NormalizeStringValue(encoded, false, true);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("\r")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void NormalizeStringValue_With_SpecialValue_ForAttribute_Should_Return_Space(string value)
    {
        // Act
        var result = CoreFuncs.NormalizeStringValue(value, true, true);

        // Assert
        result.Should().Be(" ");
    }
}