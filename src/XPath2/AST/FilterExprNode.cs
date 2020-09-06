// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.XPath;
using Wmhelp.XPath2.Properties;
using Wmhelp.XPath2.Proxy;

namespace Wmhelp.XPath2.AST
{
    /// <summary>
    /// This class is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public sealed class FilterExprNode : AbstractNode
    {
        private bool m_contextSensitive;

        public FilterExprNode(XPath2Context context, object src, List<Object> nodes)
            : base(context)
        {
            Add(src);
            AddRange(nodes);
        }

        private IEnumerable<XPathItem> CreateEnumerator(object[] dataPool, AbstractNode expr,
            XPath2NodeIterator baseIter)
        {
            XPath2NodeIterator iter = baseIter.Clone();
            ValueNode numexpr = expr as ValueNode;
            if (numexpr != null && numexpr.Content is Integer)
            {
                Integer pos = (Integer) numexpr.Content;
                foreach (XPathItem item in iter)
                {
                    if (pos == 1)
                    {
                        yield return item;
                        break;
                    }
                    else
                        pos--;
                }
            }
            else
            {
                ContextProvider provider = new ContextProvider(iter);
                object res = Undefined.Value;
                while (iter.MoveNext())
                {
                    if (m_contextSensitive || res == Undefined.Value)
                        res = expr.Execute(provider, dataPool);
                    if (res == Undefined.Value)
                    {
                        if (!m_contextSensitive)
                            break;
                        continue;
                    }
                    XPath2NodeIterator iter2 = res as XPath2NodeIterator;
                    XPathItem item;
                    if (iter2 != null)
                    {
                        iter2 = iter2.Clone();
                        if (!iter2.MoveNext())
                            continue;
                        item = iter2.Current.Clone();
                        if (!item.IsNode && iter2.MoveNext())
                            throw new XPath2Exception("FORG0006", Resources.FORG0006, "fn:boolean()",
                                new SequenceType(XmlTypeCode.AnyAtomicType, XmlTypeCardinality.OneOrMore));
                    }
                    else
                    {
                        item = res as XPathItem;
                        if (item == null)
                            item = new XPath2Item(res);
                    }
                    if (item.IsNode)
                        yield return iter.Current;
                    else
                    {
                        if (ValueProxy.IsNumeric(item.ValueType))
                        {
                            if (CoreFuncs.OperatorEq(iter.CurrentPosition + 1, item.GetTypedValue()) == CoreFuncs.True)
                            {
                                yield return iter.Current;
                                if (!m_contextSensitive)
                                    break;
                            }
                        }
                        else if (CoreFuncs.GetBooleanValue(item))
                            yield return iter.Current;
                    }
                }
            }
        }

        public override bool IsContextSensitive()
        {
            return this[0].IsContextSensitive();
        }

        public override void Bind()
        {
            base.Bind();
            m_contextSensitive = this[1].IsContextSensitive();
        }

        public override object Execute(IContextProvider provider, object[] dataPool)
        {
            XPath2NodeIterator iter = XPath2NodeIterator.Create(this[0].Execute(provider, dataPool));
            for (int k = 1; k < Count; k++)
                iter = new NodeIterator(CreateEnumerator(dataPool, this[k], iter));
            return iter;
        }

        public override XPath2ResultType GetReturnType(object[] dataPool)
        {
            return XPath2ResultType.NodeSet;
        }
    }
}