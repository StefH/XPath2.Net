// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System.Xml.XPath;
using Wmhelp.XPath2.MS;
using Wmhelp.XPath2.Properties;

namespace Wmhelp.XPath2.Iterator
{
    internal sealed class ChildOverDescendantsNodeIterator : XPath2NodeIterator
    {
        public class NodeTest
        {
            public XmlQualifiedNameTest NameTest;
            public SequenceType TypeTest;

            public NodeTest(object test)
            {
                if (test is XmlQualifiedNameTest qualifiedNameTest)
                {
                    NameTest = qualifiedNameTest;
                }
                else if (test is SequenceType type && !Equals(type, SequenceType.Node))
                {
                    TypeTest = type;
                }
            }
        }

        private readonly XPathNodeType _kind;
        private readonly XPath2Context _context;
        private readonly NodeTest[] _nodeTest;
        private readonly NodeTest _lastTest;
        private readonly XPath2NodeIterator _iterator;
        private XPathNavigator _current;

        public ChildOverDescendantsNodeIterator(XPath2Context context, NodeTest[] nodeTest, XPath2NodeIterator iterator)
        {
            _context = context;
            _nodeTest = nodeTest;
            _iterator = iterator;
            _lastTest = nodeTest[nodeTest.Length - 1];
            _kind = XPathNodeType.All;

            if (_lastTest.NameTest != null || (_lastTest.TypeTest != null && _lastTest.TypeTest.GetNodeKind() == XPathNodeType.Element))
            {
                _kind = XPathNodeType.Element;
            }
        }

        private ChildOverDescendantsNodeIterator(ChildOverDescendantsNodeIterator src)
        {
            _context = src._context;
            _nodeTest = src._nodeTest;
            _iterator = src._iterator.Clone();
            _lastTest = src._lastTest;
            _kind = src._kind;
        }

        public override XPath2NodeIterator Clone()
        {
            return new ChildOverDescendantsNodeIterator(this);
        }

        public override XPath2NodeIterator CreateBufferedIterator()
        {
            return new BufferedNodeIterator(Clone());
        }

        private bool TestItem(XPathNavigator navigator, NodeTest nodeTest)
        {
            XmlQualifiedNameTest nameTest = nodeTest.NameTest;
            SequenceType typeTest = nodeTest.TypeTest;
            if (nameTest != null)
            {
                return (navigator.NodeType == XPathNodeType.Element || navigator.NodeType == XPathNodeType.Attribute) &&
                       (nameTest.IsNamespaceWildcard || nameTest.Namespace == navigator.NamespaceURI) &&
                       (nameTest.IsNameWildcard || nameTest.Name == navigator.LocalName);
            }

            if (typeTest != null)
            {
                return typeTest.Match(navigator, _context);
            }

            return true;
        }

        private int _depth;
        private bool _accept;
        private XPathNavigator _navigator;
        private int _sequentialPosition;

        protected override XPathItem NextItem()
        {
            MoveNextIter:
            if (!_accept)
            {
                if (!_iterator.MoveNext())
                {
                    return null;
                }

                if (!(_iterator.Current is XPathNavigator current))
                {
                    throw new XPath2Exception("XPTY0019", Resources.XPTY0019, _iterator.Current.Value);
                }

                if (_current == null || !_current.MoveTo(current))
                {
                    _current = current.Clone();
                }

                _sequentialPosition = 0;
                _accept = true;
            }

            MoveToFirstChild:
            if (_current.MoveToChild(_kind))
            {
                _depth++;
                goto TestItem;
            }

            MoveToNext:
            if (_depth == 0)
            {
                _accept = false;
                goto MoveNextIter;
            }

            // https://github.com/StefH/XPath2.Net/issues/27
            // 'true' if the System.Xml.XPath.XPathNavigator is successful moving to the next sibling node
            // 'false' if there are no more siblings or if the System.Xml.XPath.XPathNavigator is currently positioned on an attribute node.
            bool moveToNextIsSuccessful = false;
            try
            {
                moveToNextIsSuccessful = _current.MoveToNext(_kind);
            }
            catch
            {
                // Just catch, moveToNextIsSuccessful will be set to 'false'
            }

            if (!moveToNextIsSuccessful)
            {
                _current.MoveToParent();
                _depth--;
                goto MoveToNext;
            }

            TestItem:
            if (_depth < _nodeTest.Length || !TestItem(_current, _lastTest))
            {
                goto MoveToFirstChild;
            }

            if (_navigator == null || !_navigator.MoveTo(_current))
            {
                _navigator = _current.Clone();
            }

            for (int k = _nodeTest.Length - 2; k >= 0; k--)
            {
                if (!(_navigator.MoveToParent() && TestItem(_navigator, _nodeTest[k])))
                {
                    goto MoveToFirstChild;
                }
            }

            _sequentialPosition++;
            return _current;
        }

        public override int SequentialPosition => _sequentialPosition;

        public override void ResetSequentialPosition()
        {
            _accept = false;
        }
    }
}