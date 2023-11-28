using FluentAssertions;
using Wmhelp.XPath2;
using Xunit;

namespace XPath2.Tests;

[Collection("Sequential")]
public class CoreFuncsTests
{
    [Theory]
    [InlineData("&nbsp;", " ")]
    [InlineData("&lt;", "<")]
    [InlineData("&gt;", ">")]
    [InlineData("&amp;", "&")]
    [InlineData("&quot;", "\"")]
    [InlineData("&apos;", "'")]
    [InlineData("&cent;", "¢")]
    [InlineData("&pound;", "£")]
    [InlineData("&yen;", "¥")]
    [InlineData("&euro;", "€")]
    [InlineData("&copy;", "©")]
    [InlineData("&reg;", "®")]
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

    [Theory]
    [InlineData(null, null, true)]
    [InlineData(true, null, false)]
    [InlineData(null, true, false)]
    [InlineData("x", "y", false)]
    [InlineData("x", "x", true)]
    [InlineData(1, 2, false)]
    [InlineData(1, 1, true)]
    public void OperatorEq(object? value1, object? value2, bool expected)
    {
        // Act
        var result = CoreFuncs.OperatorEq(value1, value2);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, null, false)]
    [InlineData(true, null, true)]
    [InlineData(null, true, false)]
    [InlineData("x", "y", false)]
    [InlineData("y", "x", true)]
    [InlineData(1, 2, false)]
    [InlineData(2, 1, true)]
    public void OperatorGt(object? value1, object? value2, bool expected)
    {
        // Act
        var result = CoreFuncs.OperatorGt(value1, value2);

        // Assert
        result.Should().Be(expected);
    }
}