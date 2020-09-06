// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2014, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

namespace Wmhelp.XPath2.AST
{
    /// <summary>
    /// This class is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public sealed class RangeNode : AbstractNode
    {
        public RangeNode(XPath2Context context, object node1, object node2)
            : base(context)
        {
            Add(node1);
            Add(node2);
        }

        public override object Execute(IContextProvider provider, object[] dataPool)
        {
            return CoreFuncs.GetRange(this[0].Execute(provider, dataPool),
                this[1].Execute(provider, dataPool));
        }

        public override XPath2ResultType GetReturnType(object[] dataPool)
        {
            return XPath2ResultType.NodeSet;
        }

        internal override XPath2ResultType GetItemType(object[] dataPool)
        {
            return XPath2ResultType.Number;
        }
    }
}