using Wmhelp.XPath2.Extensions.Core;

namespace Wmhelp.XPath2.Extensions
{
    public static class FunctionTableExtensions
    {
        /// <summary>
        /// Extend the XPath2 FunctionTable with:
        /// - generate-id
        /// - base64encode
        /// - base64decode
        /// - json-to-xml
        /// - json-to-xmlstring
        /// </summary>
        /// <param name="functionTable">The function table.</param>
        public static void AddAllExtensions(this FunctionTable functionTable)
        {
            functionTable.AddCoreExtensions();
            functionTable.AddJsonToXml();
        }
    }
}