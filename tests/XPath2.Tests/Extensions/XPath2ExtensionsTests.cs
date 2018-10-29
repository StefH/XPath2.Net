using Wmhelp.XPath2;
using Xunit;

namespace XPath2.Tests.Extensions
{
    [Collection("xpath2 extensions collection")]
    public class XPath2ExtensionsTests
    {
        private readonly XPath2TestFixture _fixture;

        public XPath2ExtensionsTests(XPath2TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void XPathExtensions_base64encode()
        {
            var result = _fixture.Navigator.XPath2Evaluate("base64encode('stef')");

            Assert.Equal("c3RlZg==", result);
        }

        [Fact]
        public void XPathExtensions_base64encode_with_encodings()
        {
            foreach (string e in new[] { "'utf-8'", "'ascii'" })
            {
                var result = _fixture.Navigator.XPath2Evaluate($"base64encode('stef', {e})");

                Assert.Equal("c3RlZg==", result);
            }
        }

        [Fact]
        public void XPathExtensions_base64encode_invalid_encoding()
        {
            var exception = Record.Exception(() => _fixture.Navigator.XPath2Evaluate("base64encode('stef', 'x')"));
            Assert.NotNull(exception);
            Assert.IsType<XPath2Exception>(exception);
            Assert.Equal("The value '\"x\"' is an invalid argument for constructor/cast Encoding.GetEncoding()", exception.Message);
        }

        [Fact]
        public void XPathExtensions_base64decode()
        {
            var result = _fixture.Navigator.XPath2Evaluate("base64decode('c3RlZg==')");

            Assert.Equal("stef", result);
        }

        [Fact]
        public void XPathExtensions_base64decode_with_fixPadding_true()
        {
            foreach (string b in new[] { "'true'", "true()" })
            {
                var result = _fixture.Navigator.XPath2Evaluate($"base64decode('c3RlZg', {b})");

                Assert.Equal("stef", result);
            }
        }

        [Fact]
        public void XPathExtensions_base64decode_with_encodings()
        {
            foreach (string e in new[] { "'utf-8'", "'ascii'" })
            {
                var result = _fixture.Navigator.XPath2Evaluate($"base64decode('c3RlZg==', {e})");

                Assert.Equal("stef", result);
            }
        }

        [Fact]
        public void XPathExtensions_base64decode_with_encoding_and_fixPadding_true()
        {
            foreach (string e in new[] { "utf-8", "ascii" })
            {
                foreach (string b in new[] { "'true'", "true()" })
                {
                    foreach (string str in new[] { "c3RlZg", "=c3RlZg=", "c3RlZg=======" })
                    {
                        var result = _fixture.Navigator.XPath2Evaluate($"base64decode('{str}', '{e}', {b})");

                        Assert.Equal("stef", result);
                    }
                }
            }
        }

        [Fact]
        public void XPathExtensions_base64decode_with_encoding_and_fixPadding_false()
        {
            foreach (string e in new[] { "'utf-8'", "'ascii'" })
            {
                foreach (string b in new[] { "'false'", "false()" })
                {
                    var result = _fixture.Navigator.XPath2Evaluate($"base64decode('c3RlZg==', {e}, {b})");

                    Assert.Equal("stef", result);
                }
            }
        }

        [Fact]
        public void XPathExtensions_base64decode_invalid_data_length()
        {
            var result = _fixture.Navigator.XPath2Evaluate("base64decode('c3RlZg')");

            Assert.Equal("stef", result);
        }

        [Fact]
        public void XPathExtensions_json_to_xml()
        {
            var result = _fixture.Navigator.XPath2Evaluate(@"string(json-to-xml('{ ""id"": 42, ""hello"": ""world"" }', 'r')/r/id)");

            Assert.Equal("42", result);
        }

        [Fact]
        public void XPathExtensions_json_to_xmlstring()
        {
            var result = _fixture.Navigator.XPath2Evaluate(@"json-to-xmlstring('{ ""id"": 42, ""hello"": ""world"" }', 'r')");

            Assert.Equal("<r>\r\n  <id>42</id>\r\n  <hello>world</hello>\r\n</r>", result);
        }
    }
}