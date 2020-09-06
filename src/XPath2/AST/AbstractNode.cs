// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Wmhelp.XPath2.AST
{
    /// <summary>
    /// This class is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public abstract class AbstractNode : IEnumerable<AbstractNode>
    {
        private AbstractNode _parent = null;
        private List<AbstractNode> _childs = null;
        private readonly XPath2Context _context = null;

        public AbstractNode(XPath2Context context)
        {
            _context = context;
        }

        public int Count
        {
            get
            {
                if (_childs == null)
                    return 0;
                return _childs.Count;
            }
        }

        public AbstractNode Parent => _parent;

        public XPath2Context Context => _context;

        public bool IsLeaf => _childs == null;

        public AbstractNode this[int index]
        {
            get
            {
                if (_childs == null)
                    throw new IndexOutOfRangeException();
                return _childs[index];
            }
            set
            {
                if (_childs == null)
                    throw new IndexOutOfRangeException();
                _childs[index] = value;
            }
        }

        public void Add(AbstractNode node)
        {
            if (node._parent != null)
                throw new ArgumentException("node");
            node._parent = this;
            if (_childs == null)
                _childs = new List<AbstractNode>();
            _childs.Add(node);
        }

        public void Add(object arg)
        {
            Add(Create(Context, arg));
        }

        public void AddRange(IEnumerable<Object> nodes)
        {
            foreach (Object node in nodes)
                Add(node);
        }

        public virtual void Bind()
        {
            if (_childs != null)
            {
                foreach (AbstractNode node in _childs)
                    node.Bind();
            }
        }

        public static AbstractNode Create(XPath2Context context, object value)
        {
            PathStep pathStep = value as PathStep;
            if (pathStep != null)
                return new PathExprNode(context, pathStep);
            AbstractNode node = value as AbstractNode;
            if (node == null)
                node = new ValueNode(context, value);
            return node;
        }

        public void TraverseSubtree(Action<AbstractNode> action)
        {
            if (_childs != null)
                foreach (AbstractNode node in _childs)
                {
                    action(node);
                    node.TraverseSubtree(action);
                }
        }

        public virtual bool IsContextSensitive()
        {
            if (_childs != null)
            {
                foreach (AbstractNode node in _childs)
                    if (node.IsContextSensitive())
                        return true;
            }
            return false;
        }

        public abstract object Execute(IContextProvider provider, object[] dataPool);

        public virtual XPath2ResultType GetReturnType(object[] dataPool)
        {
            return XPath2ResultType.Any;
        }

        internal virtual XPath2ResultType GetItemType(object[] dataPool)
        {
            return GetReturnType(dataPool);
        }

        public virtual bool IsEmptySequence()
        {
            return false;
        }

        #region IEnumerable<AbstractNode> Members

        public IEnumerator<AbstractNode> GetEnumerator()
        {
            if (_childs == null)
                throw new InvalidOperationException();
            return _childs.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_childs == null)
                throw new InvalidOperationException();
            return _childs.GetEnumerator();
        }

        #endregion
    }
}