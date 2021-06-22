// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.XPath;

namespace Wmhelp.XPath2
{
    public interface IContextProvider
    {
        XPathItem Context { get; }

        int CurrentPosition { get; }

        int LastPosition { get; }
    }

    internal sealed class ContextProvider : IContextProvider
    {
        private readonly XPath2NodeIterator m_iter;

        public ContextProvider(object value)
        {
            m_iter = XPath2NodeIterator.Create(value);
        }

        public ContextProvider(XPath2NodeIterator iter)
        {
            m_iter = iter;
        }

        public XPath2NodeIterator Iterator => m_iter;

        public bool MoveNext()
        {
            return m_iter.MoveNext();
        }

        #region IContextProvider Members

        public XPathItem Context => m_iter.Current;

        public int CurrentPosition => m_iter.CurrentPosition + 1;

        public int LastPosition => m_iter.Count;

        #endregion
    }


    [DebuggerDisplay("{curr}")]
    [DebuggerTypeProxy(typeof(XPath2NodeIteratorDebugView))]
    public abstract class XPath2NodeIterator : IEnumerable, IEnumerable<XPathItem>
#if !NETSTANDARD
        , ICloneable
#endif
    {
        internal int count = -1;
        private XPathItem curr;
        private int pos;
        private bool iteratorStarted;
        private bool iteratorFinished;

        public XPath2NodeIterator()
        {
        }

        public abstract XPath2NodeIterator Clone();

        public virtual int Count
        {
            get
            {
                if (count == -1)
                {
                    count = 0;
                    XPath2NodeIterator iter = Clone();
                    while (iter.MoveNext())
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public virtual bool IsEmpty
        {
            get
            {
                XPath2NodeIterator iter = Clone();
                if (!iter.MoveNext())
                {
                    return true;
                }

                return false;
            }
        }

        public virtual bool IsSingleIterator
        {
            get
            {
                XPath2NodeIterator iter = Clone();
                if (iter.MoveNext() && !iter.MoveNext())
                {
                    return true;
                }

                return false;
            }
        }

        public virtual bool IsRange => false;

        public XPathItem Current
        {
            get
            {
                if (!iteratorStarted)
                {
                    throw new InvalidOperationException();
                }

                return curr;
            }
        }

        public int CurrentPosition
        {
            get
            {
                if (!iteratorStarted)
                {
                    throw new InvalidOperationException();
                }

                return pos;
            }
        }

        public virtual int SequentialPosition => CurrentPosition + 1;

        public virtual void ResetSequentialPosition()
        {
            return;
        }

        public bool IsStarted => iteratorStarted;

        public virtual bool IsFinished => iteratorFinished;

        public bool MoveNext()
        {
            if (!iteratorStarted)
            {
                Init();
                pos = -1;
                iteratorStarted = true;
            }

            XPathItem item = NextItem();
            if (item != null)
            {
                pos++;
                curr = item;
                return true;
            }

            iteratorFinished = true;
            return false;
        }

        public virtual List<XPathItem> ToList()
        {
            XPath2NodeIterator iter = Clone();

            var res = new List<XPathItem>();
            while (iter.MoveNext())
            {
                res.Add(iter.Current.Clone());
            }

            return res;
        }

        public abstract XPath2NodeIterator CreateBufferedIterator();

        public override string ToString()
        {
            var items = ToList();

            return items.Any() ? string.Join(", ", items.Select(x => x.ToString()).ToArray()) : "<empty>";
        }

        protected virtual void Init()
        {
        }

        protected abstract XPathItem NextItem();

        public static XPath2NodeIterator Create(object value)
        {
            if (value == Undefined.Value)
            {
                return EmptyIterator.Shared;
            }

            if (value is XPath2NodeIterator iter)
            {
                return iter.Clone();
            }

            if (!(value is XPathItem item))
            {
                item = new XPath2Item(value);
            }

            return new SingleIterator(item);
        }

#if !NETSTANDARD
        #region ICloneable Members
        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
#endif

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        #endregion

        #region IEnumerable<XPathItem> Members

        IEnumerator<XPathItem> IEnumerable<XPathItem>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        #endregion

        private class Enumerator : IEnumerator, IEnumerator<XPathItem>
        {
            private XPath2NodeIterator current;
            private bool iterationStarted;
            private readonly XPath2NodeIterator original;

            public Enumerator(XPath2NodeIterator iter)
            {
                original = iter.Clone();
            }

            public object Current
            {
                get
                {
                    if (!iterationStarted || current == null)
                    {
                        throw new InvalidOperationException();
                    }

                    return current.Current;
                }
            }

            [DebuggerStepThrough]
            public bool MoveNext()
            {
                if (!iterationStarted)
                {
                    current = original.Clone();
                    iterationStarted = true;
                }

                if (current != null && current.MoveNext())
                {
                    return true;
                }

                current = null;
                return false;
            }

            public void Reset()
            {
                iterationStarted = false;
            }

            #region IEnumerator<XPathItem> Members

            XPathItem IEnumerator<XPathItem>.Current
            {
                get
                {
                    if (!iterationStarted || current == null)
                        throw new InvalidOperationException();
                    return current.Current;
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                return;
            }

            #endregion
        }

        public class SingleIterator : XPath2NodeIterator
        {
            private readonly XPathItem _item;

            public SingleIterator(XPathItem item)
            {
                _item = item;
            }

            public override XPath2NodeIterator Clone()
            {
                return new SingleIterator(_item.Clone());
            }

            public override bool IsSingleIterator => true;

            protected override XPathItem NextItem()
            {
                if (CurrentPosition == -1)
                {
                    return _item;
                }

                return null;
            }

            public override XPath2NodeIterator CreateBufferedIterator()
            {
                return Clone();
            }
        }

        internal class XQueryNodeIteratorDebugView
        {
            private readonly XPath2NodeIterator iter;

            public XQueryNodeIteratorDebugView(XPath2NodeIterator iter)
            {
                this.iter = iter.Clone();
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public XPathItem[] Items
            {
                get
                {
                    var res = new List<XPathItem>();
                    foreach (XPathItem item in iter)
                    {
                        if (res.Count == 10)
                        {
                            break;
                        }

                        res.Add(item.Clone());
                    }

                    return res.ToArray();
                }
            }

            public XPathItem Current => iter.curr;

            public int CurrentPosition => iter.pos;
        }
    }

    internal class XPath2NodeIteratorDebugView
    {
        private readonly XPath2NodeIterator iter;

        public XPath2NodeIteratorDebugView(XPath2NodeIterator iter)
        {
            this.iter = iter;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public XPathItem[] Items
        {
            get
            {
                var res = new List<XPathItem>();
                foreach (XPathItem item in iter)
                {
                    if (res.Count == 10)
                    {
                        break;
                    }

                    res.Add(item.Clone());
                }

                return res.ToArray();
            }
        }

        public XPathItem Current => iter.Current;

        public int CurrentPosition => iter.CurrentPosition;
    }
}