// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

namespace Wmhelp.XPath2.AST
{
    /// <summary>
    /// This class is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public sealed class OrderedBinaryOperatorNode : BinaryOperatorNode
    {
        public OrderedBinaryOperatorNode(XPath2Context context, BinaryOperator action, object node1, object node2,
            XPath2ResultType resultType)
            : base(context, action, node1, node2, resultType)
        {
        }
    }
}