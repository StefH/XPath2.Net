// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System.Xml;
using System.Xml.XPath;

namespace Wmhelp.XPath2.AST
{
    /// <summary>
    /// This class is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public sealed class ForNode : AbstractNode
    {
        private readonly Tokenizer.VarName _varName;
        private NameBinder.ReferenceLink _varRef;

        public ForNode(XPath2Context context, Tokenizer.VarName varName, object expr)
            : base(context)
        {
            _varName = varName;
            Add(expr);
        }

        public void AddTail(object expr)
        {
            if (Count == 1)
                Add(expr);
            else
                ((ForNode) this[1]).AddTail(expr);
        }

        public override void Bind()
        {
            this[0].Bind();
            XmlQualifiedName qname = QNameParser.Parse(_varName.ToString(),
                Context.NamespaceManager, Context.NameTable);
            _varRef = Context.RunningContext.NameBinder.PushVar(qname);
            this[1].Bind();
            Context.RunningContext.NameBinder.PopVar();
        }

        public override object Execute(IContextProvider provider, object[] dataPool)
        {
            return new ForIterator(this, provider, dataPool,
                XPath2NodeIterator.Create(this[0].Execute(provider, dataPool)));
        }

        public override XPath2ResultType GetReturnType(object[] dataPool)
        {
            return XPath2ResultType.NodeSet;
        }

        private bool MoveNext(IContextProvider provider, object[] dataPool, XPathItem curr, out object res)
        {
            if (curr.IsNode)
                _varRef.Set(dataPool, curr);
            else
                _varRef.Set(dataPool, curr.TypedValue);
            res = this[1].Execute(provider, dataPool);
            if (res != Undefined.Value)
                return true;
            return false;
        }

        private sealed class ForIterator : XPath2NodeIterator
        {
            private readonly ForNode owner;
            private readonly IContextProvider provider;
            private readonly object[] dataPool;
            private readonly XPath2NodeIterator baseIter;
            private XPath2NodeIterator iter;
            private XPath2NodeIterator childIter;

            public ForIterator(ForNode owner, IContextProvider provider, object[] dataPool, XPath2NodeIterator baseIter)
            {
                this.owner = owner;
                this.provider = provider;
                this.dataPool = dataPool;
                this.baseIter = baseIter;
            }

            public override XPath2NodeIterator Clone()
            {
                return new ForIterator(owner, provider, dataPool, baseIter);
            }

            public override XPath2NodeIterator CreateBufferedIterator()
            {
                return new BufferedNodeIterator(this);
            }

            protected override void Init()
            {
                iter = baseIter.Clone();
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
                    if (!iter.MoveNext())
                        return null;
                    object res;
                    if (owner.MoveNext(provider, dataPool, iter.Current, out res))
                    {
                        childIter = res as XPath2NodeIterator;
                        if (childIter == null)
                        {
                            XPathItem item = res as XPathItem;
                            if (item != null)
                                return item;
                            return new XPath2Item(res);
                        }
                    }
                }
            }
        }
    }
}