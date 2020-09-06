// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

namespace Wmhelp.XPath2.AST
{
    /// <summary>
    /// This class is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public sealed class OrExprNode : AbstractNode
    {
        public OrExprNode(XPath2Context context, object node1, object node2)
            : base(context)
        {
            Add(node1);
            Add(node2);
        }

        public override object Execute(IContextProvider provider, object[] dataPool)
        {
            if (CoreFuncs.BooleanValue(this[0].Execute(provider, dataPool)) == CoreFuncs.True ||
                CoreFuncs.BooleanValue(this[1].Execute(provider, dataPool)) == CoreFuncs.True)
                return CoreFuncs.True;
            return CoreFuncs.False;
        }

        public override XPath2ResultType GetReturnType(object[] dataPool)
        {
            return XPath2ResultType.Boolean;
        }
    }
}