using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Wmhelp.XPath2
{
    public static class XNodeExtensions
    {
        public static IEnumerable<T> XPath2Select<T>(this XNode node, string xpath)
            where T : XObject
        {
            return XPath2Select<T>(node, xpath, null);
        }

        public static IEnumerable<T> XPath2Select<T>(this XNode node, string xpath, IXmlNamespaceResolver nsResolver)
            where T : XObject
        {
            return XPath2Select<T>(node, xpath, nsResolver, null);
        }

        public static IEnumerable<T> XPath2Select<T>(this XNode node, string xpath, object arg)
            where T : XObject
        {
            return XPath2Select<T>(node, xpath, null, arg);
        }

        public static IEnumerable<T> XPath2Select<T>(this XNode node, string xpath, IXmlNamespaceResolver nsResolver, object arg)
            where T : XObject
        {
            return XPath2Select<T>(node, XPath2Expression.Compile(xpath, nsResolver), arg);
        }

        public static IEnumerable<T> XPath2Select<T>(this XNode node, XPath2Expression expression, object arg)
            where T : XObject
        {
            XPathNavigator navigator = node.CreateNavigator();
            XPath2NodeIterator nodeIterator = navigator.XPath2Select(expression, arg);
            foreach (XPathItem item in nodeIterator)
                if (item.IsNode)
                {
                    XPathNavigator curr = (XPathNavigator)item;
                    XObject o = (XObject)curr.UnderlyingObject;
                    if (!(o is T))
                        throw new InvalidOperationException($"Unexpected evaluation {o.GetType()}");
                    yield return (T)o;
                }
                else
                    throw new InvalidOperationException($"Unexpected evaluation {item.TypedValue.GetType()}");
        }

        public static IEnumerable<object> XPath2Select(this XNode node, string xpath, IXmlNamespaceResolver nsResolver = null)
        {
            return XPath2Select(node, xpath, nsResolver, null);
        }

        public static IEnumerable<object> XPath2Select(this XNode node, string xpath, object arg)
        {
            return XPath2Select(node, xpath, null, arg);
        }

        public static IEnumerable<object> XPath2Select(this XNode node, string xpath, IXmlNamespaceResolver nsResolver, object arg)
        {
            return XPath2Select(node, XPath2Expression.Compile(xpath, nsResolver), arg);
        }

        public static IEnumerable<object> XPath2Select(this XNode node, XPath2Expression expression, object arg)
        {
            XPathNavigator navigator = node.CreateNavigator();
            XPath2NodeIterator nodeIterator = navigator.XPath2Select(expression, arg);

            foreach (XPathItem item in nodeIterator)
            {
                if (item.IsNode)
                {
                    XPathNavigator currentNavigator = (XPathNavigator)item;
                    yield return currentNavigator.UnderlyingObject;
                }
                else
                {
                    yield return item.TypedValue;
                }
            }
        }

        public static T XPath2SelectOne<T>(this XNode node, string xpath)
            where T : XObject
        {
            return XPath2SelectOne<T>(node, xpath, null);
        }

        public static T XPath2SelectOne<T>(this XNode node, string xpath, IXmlNamespaceResolver nsResolver)
            where T : XObject
        {
            return XPath2SelectOne<T>(node, xpath, nsResolver, null);
        }

        public static T XPath2SelectOne<T>(this XNode node, string xpath, object arg)
            where T : XObject
        {
            return XPath2SelectOne<T>(node, xpath, null, arg);
        }

        public static T XPath2SelectOne<T>(this XNode node, string xpath, IXmlNamespaceResolver nsResolver, object arg)
            where T : XObject
        {
            return XPath2SelectOne<T>(node, XPath2Expression.Compile(xpath, nsResolver), arg);
        }

        public static T XPath2SelectOne<T>(this XNode node, XPath2Expression expression, object arg)
            where T : XObject
        {
            return XPath2Select<T>(node, expression, arg).FirstOrDefault();
        }

        public static object XPath2SelectOne(this XNode node, string xpath, object arg)
        {
            return XPath2SelectOne(node, xpath, null, arg);
        }

        public static object XPath2SelectOne(this XNode node, string xpath, IXmlNamespaceResolver nsResolver = null, object arg = null)
        {
            return XPath2SelectOne(node, XPath2Expression.Compile(xpath, nsResolver), arg);
        }

        public static object XPath2SelectOne(this XNode node, XPath2Expression expression, object arg)
        {
            return XPath2Select(node, expression, arg).FirstOrDefault();
        }

        public static IEnumerable<XElement> XPath2SelectElements(this XNode node, string xpath, object arg)
        {
            return XPath2SelectElements(node, xpath, null, arg);
        }

        public static IEnumerable<XElement> XPath2SelectElements(this XNode node, string xpath, IXmlNamespaceResolver nsResolver = null, object arg = null)
        {
            return XPath2SelectElements(node, XPath2Expression.Compile(xpath, nsResolver), arg);
        }

        public static IEnumerable<XElement> XPath2SelectElements(this XNode node, XPath2Expression expression)
        {
            return XPath2SelectElements(node, expression, null);
        }

        public static IEnumerable<XElement> XPath2SelectElements(this XNode node, XPath2Expression expression, object arg)
        {
            return XPath2Select<XElement>(node, expression, arg);
        }

        public static XElement XPath2SelectElement(this XNode node, string xpath, object arg)
        {
            return XPath2SelectElement(node, xpath, null, arg);
        }

        public static XElement XPath2SelectElement(this XNode node, string xpath, IXmlNamespaceResolver nsResolver = null, object arg = null)
        {
            return XPath2SelectElement(node, XPath2Expression.Compile(xpath, nsResolver), arg);
        }

        public static XElement XPath2SelectElement(this XNode node, XPath2Expression expression, object arg)
        {
            return XPath2SelectOne<XElement>(node, expression, arg);
        }

        public static IEnumerable<object> XPath2SelectValues(this XNode node, string xpath, object arg)
        {
            return XPath2SelectValues(node, xpath, null, arg);
        }

        public static IEnumerable<object> XPath2SelectValues(this XNode node, string xpath, IXmlNamespaceResolver nsResolver = null, object arg = null)
        {
            return XPath2SelectValues(node, XPath2Expression.Compile(xpath, nsResolver), arg);
        }

        public static IEnumerable<object> XPath2SelectValues(this XNode node, XPath2Expression expr, object arg = null)
        {
            XPathNavigator nav = node.CreateNavigator();
            XPath2NodeIterator iter = XPath2NodeIterator.Create(nav.XPath2Evaluate(expr, arg));
            while (iter.MoveNext())
                yield return iter.Current.GetTypedValue();
        }
    }
}