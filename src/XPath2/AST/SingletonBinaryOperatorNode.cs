// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System.Xml.XPath;

namespace Wmhelp.XPath2.AST
{
    /// <summary>
    /// This class is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public sealed class SingletonBinaryOperatorNode : BinaryOperatorNode
    {
        public SingletonBinaryOperatorNode(XPath2Context context, BinaryOperator action, object node1, object node2,
            XPath2ResultType resultType)
            : base(context, action, node1, node2, resultType)
        {
        }

        public override object Execute(IContextProvider provider, object[] dataPool)
        {
            XPathNavigator value1 = CoreFuncs.NodeValue(this[0].Execute(provider, dataPool), false);
            if (value1 != null)
            {
                XPathNavigator value2 = CoreFuncs.NodeValue(this[1].Execute(provider, dataPool), false);
                if (value2 != null)
                    return _binaryOper(provider, value1, value2);
            }
            return Undefined.Value;
        }
    }
}