// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System.Linq;
using System.Xml.XPath;

namespace Wmhelp.XPath2.AST
{
    /// <summary>
    /// This class is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public sealed class ExprNode : AbstractNode
    {
        public ExprNode(XPath2Context context, AbstractNode node)
            : base(context)
        {
            Add(node);
        }

        public ExprNode(XPath2Context context, object node)
            : base(context)
        {
            Add(node);
        }

        public override object Execute(IContextProvider provider, object[] dataPool)
        {
            if (Count == 1)
                return this[0].Execute(provider, dataPool);
            return new ExprIterator(this, provider, dataPool);
        }

        public override XPath2ResultType GetReturnType(object[] dataPool)
        {
            if (Count == 1)
                return this[0].GetReturnType(dataPool);
            return XPath2ResultType.NodeSet;
        }

        internal override XPath2ResultType GetItemType(object[] dataPool)
        {
            XPath2ResultType resType = XPath2ResultType.Any;
            foreach (AbstractNode child in this)
            {
                if (!child.IsEmptySequence())
                {
                    XPath2ResultType itemType = child.GetItemType(dataPool);
                    if (itemType != resType)
                    {
                        if (resType != XPath2ResultType.Any)
                            return XPath2ResultType.Any;
                        resType = itemType;
                    }
                }
            }
            return resType;
        }

        private sealed class ExprIterator : XPath2NodeIterator
        {
            private readonly IContextProvider provider;
            private readonly object[] dataPool;
            private readonly AbstractNode[] nodes;
            private XPath2NodeIterator childIter;
            private int index = 0;

            private ExprIterator(AbstractNode[] nodes, IContextProvider provider, object[] dataPool)
            {
                this.nodes = nodes;
                this.provider = provider;
                this.dataPool = dataPool;
            }

            public ExprIterator(ExprNode owner, IContextProvider provider, object[] dataPool)
            {
                nodes = owner.ToArray();
                this.provider = provider;
                this.dataPool = dataPool;
            }

            public override XPath2NodeIterator Clone()
            {
                return new ExprIterator(nodes, provider, dataPool);
            }

            public override XPath2NodeIterator CreateBufferedIterator()
            {
                return new BufferedNodeIterator(this);
            }

            protected override XPathItem NextItem()
            {
                while (true)
                {
                    if (childIter != null)
                    {
                        if (childIter.MoveNext())
                            return childIter.Current;
                        else
                            childIter = null;
                    }
                    if (index == nodes.Length)
                        return null;
                    object res = nodes[index++].Execute(provider, dataPool);
                    if (res != Undefined.Value)
                    {
                        childIter = res as XPath2NodeIterator;
                        if (childIter == null)
                        {
                            XPathItem item = res as XPathItem;
                            if (item == null)
                                return new XPath2Item(res);
                            return item;
                        }
                    }
                }
            }
        }
    }
}