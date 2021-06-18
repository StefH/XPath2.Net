using System.Xml;
using System.Xml.XPath;
using JetBrains.Annotations;
using Wmhelp.XPath2.MS;

namespace Wmhelp.XPath2.Extensions
{
    public static class FunctionTableExtensions
    {
        /// <summary>
        /// Extend the XPath2 FunctionTable with:
        /// - json-to-xml
        /// - json-to-xmlstring
        /// </summary>
        /// <param name="functionTable">The function table.</param>
        public static void AddJsonToXml([NotNull] this FunctionTable functionTable)
        {
            XPathNavigator JsonStringToXPathNavigator(XPath2Context context, IContextProvider provider, object[] args)
            {
                string value = CoreFuncs.CastToStringExactOne(context, args[0]);
                string root = args.Length == 2 ? CoreFuncs.CastToStringOptional(context, args[1]) : null;

                string dynamicRootObject;
                XmlNode xmlDoc = Json2XmlUtils.Json2XmlNode(value, out dynamicRootObject, root);

                return xmlDoc?.CreateNavigator();
            }

            string JsonStringToXmlString(XPath2Context context, IContextProvider provider, object[] args)
            {
                var nav = JsonStringToXPathNavigator(context, provider, args);

                return nav != null ? nav.InnerXml : string.Empty;
            }

            // json-to-xml with no root element
            functionTable.Add(XmlReservedNs.NsXQueryFunc, "json-to-xml", 1, XPath2ResultType.Navigator, JsonStringToXPathNavigator);

            // json-to-xml with specified root element
            functionTable.Add(XmlReservedNs.NsXQueryFunc, "json-to-xml", 2, XPath2ResultType.Navigator, JsonStringToXPathNavigator);

            // json-to-xmlstring with no root element
            functionTable.Add(XmlReservedNs.NsXQueryFunc, "json-to-xmlstring", 1, XPath2ResultType.String, JsonStringToXmlString);

            // json-to-xmlstring with specified root element
            functionTable.Add(XmlReservedNs.NsXQueryFunc, "json-to-xmlstring", 2, XPath2ResultType.String, JsonStringToXmlString);
        }
    }
}