using System;
using System.Xml;
using System.Xml.XPath;
using Wmhelp.XPath2;
using Wmhelp.XPath2.Extensions;

namespace XPath2.Tests.Extensions
{
    public class XPath2TestFixture : IDisposable
    {
        public readonly XPathNavigator Navigator;

        public XPath2TestFixture()
        {
            var doc = new XmlDocument();
            Navigator = doc.CreateNavigator();

            FunctionTable.Inst.AddAllExtensions();

            // Adding the extensions again should not throw exception
            FunctionTable.Inst.AddAllExtensions();
        }

        public void Dispose()
        {
        }
    }
}