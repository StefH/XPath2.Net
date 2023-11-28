using System.Xml;
using System.Xml.XPath;
using Wmhelp.XPath2;
using Wmhelp.XPath2.Extensions;
using Xunit;

namespace XPath2.Tests.Extensions;

[Collection("Sequential")]
public class XPath2ExtensionsTests
{
    private readonly XPathNavigator _navigator;

    public XPath2ExtensionsTests()
    {
        var doc = new XmlDocument();
        _navigator = doc.CreateNavigator();

        FunctionTable.Inst.AddAllExtensions();

        // Adding the extensions again should not throw exception
        FunctionTable.Inst.AddAllExtensions();
    }

    [Fact]
    public void XPathExtensions_base64encode()
    {
        var result = _navigator.XPath2Evaluate("base64encode('stef')");

        Assert.Equal("c3RlZg==", result);
    }

    [Fact]
    public void XPathExtensions_base64encode_with_encodings()
    {
        foreach (var e in new[] { "'utf-8'", "'ascii'" })
        {
            var result = _navigator.XPath2Evaluate($"base64encode('stef', {e})");

            Assert.Equal("c3RlZg==", result);
        }
    }

    [Fact]
    public void XPathExtensions_base64encode_invalid_encoding()
    {
        var exception = Record.Exception(() => _navigator.XPath2Evaluate("base64encode('stef', 'x')"));
        Assert.NotNull(exception);
        Assert.IsType<XPath2Exception>(exception);
        Assert.Equal("The value '\"x\"' is an invalid argument for constructor/cast Encoding.GetEncoding()", exception.Message);
    }

    [Fact]
    public void XPathExtensions_base64decode()
    {
        var result = _navigator.XPath2Evaluate("base64decode('c3RlZg==')");

        Assert.Equal("stef", result);
    }

    [Fact]
    public void XPathExtensions_base64decode_with_fixPadding_true()
    {
        foreach (var b in new[] { "'true'", "true()" })
        {
            var result = _navigator.XPath2Evaluate($"base64decode('c3RlZg', {b})");

            Assert.Equal("stef", result);
        }
    }

    [Fact]
    public void XPathExtensions_base64decode_with_encodings()
    {
        foreach (var e in new[] { "'utf-8'", "'ascii'" })
        {
            var result = _navigator.XPath2Evaluate($"base64decode('c3RlZg==', {e})");

            Assert.Equal("stef", result);
        }
    }

    [Fact]
    public void XPathExtensions_base64decode_with_encoding_and_fixPadding_true()
    {
        foreach (var e in new[] { "utf-8", "ascii" })
        {
            foreach (var b in new[] { "'true'", "true()" })
            {
                foreach (var str in new[] { "c3RlZg", "=c3RlZg=", "c3RlZg=======" })
                {
                    var result = _navigator.XPath2Evaluate($"base64decode('{str}', '{e}', {b})");

                    Assert.Equal("stef", result);
                }
            }
        }
    }

    [Fact]
    public void XPathExtensions_base64decode_with_encoding_and_fixPadding_false()
    {
        foreach (var e in new[] { "'utf-8'", "'ascii'" })
        {
            foreach (var b in new[] { "'false'", "false()" })
            {
                var result = _navigator.XPath2Evaluate($"base64decode('c3RlZg==', {e}, {b})");

                Assert.Equal("stef", result);
            }
        }
    }

    [Fact]
    public void XPathExtensions_base64decode_invalid_data_length()
    {
        var result = _navigator.XPath2Evaluate("base64decode('c3RlZg')");

        Assert.Equal("stef", result);
    }

    [Fact]
    public void XPathExtensions_json_to_xml()
    {
        var result = _navigator.XPath2Evaluate(@"string(json-to-xml('{ ""id"": 42, ""hello"": ""world"" }', 'r')/r/id)");

        Assert.Equal("42", result);
    }

    [Fact]
    public void XPathExtensions_json_to_xmlstring()
    {
        var result = _navigator.XPath2Evaluate(@"json-to-xmlstring('{ ""id"": 42, ""hello"": ""world"" }', 'r')");

        Assert.Equal("<r>\r\n  <id>42</id>\r\n  <hello>world</hello>\r\n</r>", result);
    }
}