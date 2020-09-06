// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System;
using System.Collections.Generic;
using Wmhelp.XPath2.Iterator;

namespace Wmhelp.XPath2.AST
{
    public enum XPath2ExprType
    {
        Child,
        Descendant,
        Attribute,
        Self,
        DescendantOrSelf,
        FollowingSibling,
        Following,
        Parent,
        Ancestor,
        PrecedingSibling,
        Preceding,
        AncestorOrSelf,
        Namespace,
        PositionFilter,
        ChildOverDescendants,
        Expr
    };

    /// <summary>
    /// This class is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public class PathStep
    {
        public readonly object nodeTest;
        public readonly XPath2ExprType type;
        public readonly AbstractNode node;

        public PathStep Next { get; private set; }

        public PathStep(AbstractNode node)
        {
            nodeTest = null;
            type = XPath2ExprType.Expr;
            this.node = node;
        }

        public PathStep(object nodeTest, XPath2ExprType type)
        {
            this.nodeTest = nodeTest;
            this.type = type;
            node = null;
        }

        public PathStep(XPath2ExprType type)
            : this(null, type)
        {
        }

        public void AddLast(PathStep pathStep)
        {
            PathStep last = this;
            while (last.Next != null)
                last = last.Next;
            last.Next = pathStep;
        }

        public XPath2NodeIterator Create(XPath2Context context, object[] dataPool, XPath2NodeIterator baseIter, bool special)
        {
            switch (type)
            {
                case XPath2ExprType.Attribute:
                    return new AttributeNodeIterator(context, nodeTest, baseIter);
                case XPath2ExprType.Child:
                    {
                        if (special && nodeTest != SequenceType.Node)
                            return new SpecialChildNodeIterator(context, nodeTest, baseIter);
                        return new ChildNodeIterator(context, nodeTest, baseIter);
                    }
                case XPath2ExprType.Descendant:
                    {
                        if (special && nodeTest != SequenceType.Node)
                            return new SpecialDescendantNodeIterator(context, nodeTest, false, baseIter);
                        return new DescendantNodeIterator(context, nodeTest, false, baseIter);
                    }
                case XPath2ExprType.DescendantOrSelf:
                    {
                        if (special && nodeTest != SequenceType.Node)
                            return new SpecialDescendantNodeIterator(context, nodeTest, true, baseIter);
                        return new DescendantNodeIterator(context, nodeTest, true, baseIter);
                    }
                case XPath2ExprType.Ancestor:
                    return new AncestorNodeIterator(context, nodeTest, false, baseIter);
                case XPath2ExprType.AncestorOrSelf:
                    return new AncestorNodeIterator(context, nodeTest, true, baseIter);
                case XPath2ExprType.Following:
                    return new FollowingNodeIterator(context, nodeTest, baseIter);
                case XPath2ExprType.FollowingSibling:
                    return new FollowingSiblingNodeIterator(context, nodeTest, baseIter);
                case XPath2ExprType.Parent:
                    return new ParentNodeIterator(context, nodeTest, baseIter);
                case XPath2ExprType.Preceding:
                    return new PrecedingNodeIterator(context, nodeTest, baseIter);
                case XPath2ExprType.PrecedingSibling:
                    return new PrecedingSiblingNodeIterator(context, nodeTest, baseIter);
                case XPath2ExprType.Namespace:
                    return new NamespaceNodeIterator(context, nodeTest, baseIter);
                case XPath2ExprType.Self:
                    return new SelfNodeIterator(context, nodeTest, baseIter);
                case XPath2ExprType.Expr:
                    return new ExprNodeIterator(context, node, dataPool, baseIter);
                case XPath2ExprType.PositionFilter:
                    return new PositionFilterNodeIterator(Convert.ToInt32(nodeTest), baseIter);
                case XPath2ExprType.ChildOverDescendants:
                    return new ChildOverDescendantsNodeIterator(context,
                        (ChildOverDescendantsNodeIterator.NodeTest[])nodeTest, baseIter);
                default:
                    return null;
            }
        }

        public static PathStep Create(XPath2Context context, object node)
        {
            PathStep res = node as PathStep;
            if (res != null)
                return res;
            PathExprNode pathExpr = node as PathExprNode;
            if (pathExpr != null)
                return pathExpr.FirstStep;
            return new PathStep(AbstractNode.Create(context, node));
        }

        public static PathStep CreateFilter(XPath2Context context, object node, List<Object> predicateList)
        {
            if (predicateList.Count == 1)
            {
                AbstractNode predicate = AbstractNode.Create(context, predicateList[0]);
                ValueNode numexpr = predicate as ValueNode;
                if (numexpr != null && numexpr.Content is Integer)
                {
                    PathStep res = Create(context, node);
                    res.AddLast(new PathStep(numexpr.Content, XPath2ExprType.PositionFilter));
                    return res;
                }
            }
            AbstractNode filterExpr = new FilterExprNode(context, node, predicateList);
            return new PathStep(filterExpr);
        }
    }
}

