using System.Xml;
using System.Xml.XPath;

// ReSharper disable once CheckNamespace
namespace Wmhelp.XPath2
{
    public static class XPathNavigatorExtensions
    {
        public static object XPath2Evaluate(this XPathNavigator nav, string xpath2)
        {
            return XPath2Evaluate(nav, xpath2, null);
        }

        public static object XPath2Evaluate(this XPathNavigator nav, string xpath2, IXmlNamespaceResolver nsResolver)
        {
            return XPath2Evaluate(nav, xpath2, nsResolver, null);
        }

        public static object XPath2Evaluate(this XPathNavigator nav, string xpath2, IXmlNamespaceResolver nsResolver, object arg)
        {
            return XPath2Evaluate(nav, XPath2Expression.Compile(xpath2, nsResolver), arg);
        }

        public static object XPath2Evaluate(this XPathNavigator nav, XPath2Expression expr)
        {
            return XPath2Evaluate(nav, expr, null);
        }

        public static object XPath2Evaluate(this XPathNavigator nav, XPath2Expression expr, object arg)
        {
            return expr.EvaluateWithProperties(new NodeProvider(nav), arg);
        }

        public static XPath2NodeIterator XPath2Select(this XPathNavigator nav, string xpath)
        {
            return XPath2Select(nav, xpath, null);
        }

        public static XPath2NodeIterator XPath2Select(this XPathNavigator nav, string xpath, object arg)
        {
            return XPath2Select(nav, XPath2Expression.Compile(xpath, null), arg);
        }

        public static XPath2NodeIterator XPath2Select(this XPathNavigator nav, string xpath, IXmlNamespaceResolver resolver)
        {
            return XPath2Select(nav, XPath2Expression.Compile(xpath, resolver), null);
        }

        public static XPath2NodeIterator XPath2Select(this XPathNavigator nav, string xpath, IXmlNamespaceResolver resolver, object arg)
        {
            return XPath2Select(nav, XPath2Expression.Compile(xpath, resolver), arg);
        }

        public static XPath2NodeIterator XPath2Select(this XPathNavigator nav, XPath2Expression expr, object arg)
        {
            return XPath2NodeIterator.Create(XPath2Evaluate(nav, expr, arg));
        }

        public static XPathNodeIterator XPath2SelectNodes(this XPathNavigator nav, string xpath)
        {
            return XPath2SelectNodes(nav, xpath, null);
        }

        public static XPathNodeIterator XPath2SelectNodes(this XPathNavigator nav, XPath2Expression expr)
        {
            return XPath2SelectNodes(nav, expr, null);
        }

        public static XPathNodeIterator XPath2SelectNodes(this XPathNavigator nav, XPath2Expression expr, object arg)
        {
            return new XPathNodeIteratorAdapter(XPath2Select(nav, expr, arg));
        }

        public static XPathNodeIterator XPath2SelectNodes(this XPathNavigator nav, string xpath, IXmlNamespaceResolver resolver)
        {
            return XPath2SelectNodes(nav, XPath2Expression.Compile(xpath, resolver));
        }

        public static XPathNavigator XPath2SelectSingleNode(this XPathNavigator nav, string xpath)
        {
            return XPath2SelectSingleNode(nav, XPath2Expression.Compile(xpath));
        }

        public static XPathNavigator XPath2SelectSingleNode(this XPathNavigator nav, XPath2Expression expression)
        {
            return XPath2SelectSingleNode(nav, expression, null);
        }

        public static XPathNavigator XPath2SelectSingleNode(this XPathNavigator nav, XPath2Expression expression, object arg)
        {
            XPath2NodeIterator iter = nav.XPath2Select(expression, arg);
            if (iter.MoveNext() && iter.Current.IsNode)
                return (XPathNavigator)iter.Current;
            return null;
        }

        public static XPathNavigator XPath2SelectSingleNode(this XPathNavigator nav, string xpath, IXmlNamespaceResolver resolver)
        {
            return XPath2SelectSingleNode(nav, XPath2Expression.Compile(xpath, resolver));
        }
    }
}