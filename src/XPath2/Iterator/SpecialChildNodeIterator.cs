﻿// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System.Xml.XPath;

namespace Wmhelp.XPath2.Iterator
{
    internal sealed class SpecialChildNodeIterator : SequentialAxisNodeIterator
    {
        private readonly XPathNodeType kind;

        public SpecialChildNodeIterator(XPath2Context context, object nodeTest, XPath2NodeIterator iter)
            : base(context, nodeTest, false, iter)
        {
            if (typeTest == null)
            {
                if (nameTest == null)
                    kind = XPathNodeType.All;
                else
                    kind = XPathNodeType.Element;
            }
            else
                kind = typeTest.GetNodeKind();
        }

        private SpecialChildNodeIterator(SpecialChildNodeIterator src)
        {
            AssignFrom(src);
            kind = src.kind;
        }

        public override XPath2NodeIterator Clone()
        {
            return new SpecialChildNodeIterator(this);
        }

        protected override bool MoveToFirst(XPathNavigator nav)
        {
            return nav.MoveToChild(kind);
        }

        protected override bool MoveToNext(XPathNavigator nav)
        {
            return nav.MoveToNext(kind);
        }
    }
}