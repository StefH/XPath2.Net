using System.Collections.Generic;
using System.Xml;
using Wmhelp.XPath2;

namespace XQTSRunConsole
{
    struct PreparedXPath
    {
        public XPath2Expression expression;
        public IContextProvider provider;
        public IDictionary<XmlQualifiedName, object> vars;

        public object Evaluate()
        {
            return expression.Evaluate(provider, vars);
        }

        public XPath2ResultType GetResultType()
        {
            return expression.GetResultType(vars);
        }
    }
}