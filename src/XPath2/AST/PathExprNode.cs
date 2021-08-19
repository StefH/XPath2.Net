// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System.Collections.Generic;
using Wmhelp.XPath2.Iterator;

namespace Wmhelp.XPath2.AST
{
    /// <summary>
    /// This class is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public sealed class PathExprNode : AbstractNode
    {
        private bool _isOrderedSet;
        private readonly PathStep[] _path;

        public bool Unordered { get; set; }

        public PathStep FirstStep => _path[0];

        public PathExprNode(XPath2Context context, PathStep pathStep)
            : base(context)
        {
            List<PathStep> path = new List<PathStep>();
            for (PathStep curr = pathStep; curr != null; curr = curr.Next)
            {
                if (curr.type == XPath2ExprType.Expr)
                    Add(curr.node);
                path.Add(curr);
            }
            if (path.Count == 2 &&
                path[0].type == XPath2ExprType.DescendantOrSelf &&
                path[0].nodeTest == SequenceType.Node &&
                path[1].type == XPath2ExprType.Child)
                _path = new PathStep[] { new PathStep(path[1].nodeTest, XPath2ExprType.Descendant) };
            else
            {
                bool transform;
                do
                {
                    transform = false;
                    for (int k = 0; k < path.Count - 2; k++)
                        if (path[k].type == XPath2ExprType.DescendantOrSelf)
                        {
                            int s = k + 1;
                            List<ChildOverDescendantsNodeIterator.NodeTest> nodeTest =
                                new List<ChildOverDescendantsNodeIterator.NodeTest>();
                            for (; s < path.Count; s++)
                            {
                                if (path[s].type != XPath2ExprType.Child)
                                    break;
                                nodeTest.Add(new ChildOverDescendantsNodeIterator.NodeTest(path[s].nodeTest));
                            }
                            if (nodeTest.Count > 1)
                            {
                                int n = nodeTest.Count + 1;
                                while (n-- > 0)
                                    path.RemoveAt(k);
                                path.Insert(k, new PathStep(nodeTest.ToArray(), XPath2ExprType.ChildOverDescendants));
                                transform = true;
                                break;
                            }
                        }
                } while (transform);
                _path = path.ToArray();
            }
        }

        public override void Bind()
        {
            base.Bind();
            _isOrderedSet = IsOrderedSet();
        }

        public override bool IsContextSensitive()
        {
            if (_path[0].type == XPath2ExprType.Expr)
                return _path[0].node.IsContextSensitive();
            return true;
        }

        private bool IsOrderedSet()
        {
            for (int k = 0; k < _path.Length; k++)
            {
                XPath2ExprType exprType;
                if (_path[k].type == XPath2ExprType.Expr)
                {
                    if (k == 0)
                        continue;
                    FilterExprNode filterExpr = _path[k].node as FilterExprNode;
                    if (filterExpr == null)
                        return false;
                    PathExprNode pathExpr = filterExpr[0] as PathExprNode;
                    if (pathExpr == null)
                        return false;
                    exprType = pathExpr._path[0].type;
                }
                else
                    exprType = _path[k].type;
                switch (exprType)
                {
                    case XPath2ExprType.Expr:
                    case XPath2ExprType.Parent:
                    case XPath2ExprType.Ancestor:
                    case XPath2ExprType.AncestorOrSelf:
                    case XPath2ExprType.Preceding:
                    case XPath2ExprType.PrecedingSibling:
                        return false;

                    case XPath2ExprType.Descendant:
                    case XPath2ExprType.DescendantOrSelf:
                    case XPath2ExprType.Following:
                    case XPath2ExprType.ChildOverDescendants:
                        if (k < _path.Length - 1)
                            for (int s = k + 1; s < _path.Length; s++)
                            {
                                if (_path[s].type != XPath2ExprType.Attribute &&
                                    _path[s].type != XPath2ExprType.Namespace)
                                    return false;
                            }
                        break;
                }
            }
            return true;
        }

        public override object Execute(IContextProvider provider, object[] dataPool)
        {
            bool orderedSet = _isOrderedSet;
            bool special = provider != null &&
                           provider.Context.GetType().Name == "XPathDocumentNavigator";
            XPath2NodeIterator tail;
            if (_path[0].type == XPath2ExprType.Expr)
            {
                tail = XPath2NodeIterator.Create(_path[0].node.Execute(provider, dataPool));
                if (!(_path[0].node is OrderedBinaryOperatorNode))
                    orderedSet = orderedSet && tail.IsSingleIterator;
            }
            else
                tail = _path[0].Create(Context, dataPool,
                    XPath2NodeIterator.Create(CoreFuncs.ContextNode(provider)), special);
            for (int k = 1; k < _path.Length; k++)
                tail = _path[k].Create(Context, dataPool, tail, special);
            if (!orderedSet)
                return new DocumentOrderNodeIterator(tail);
            return tail;
        }

        public override XPath2ResultType GetReturnType(object[] dataPool)
        {
            return XPath2ResultType.NodeSet;
        }
    }
}