// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System.Collections.Generic;
using System.Xml;
using Wmhelp.XPath2.Properties;

namespace Wmhelp.XPath2
{
    /// <summary>
    /// This delegate is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public delegate void ChangeValueAction(NameBinder.ReferenceLink sender, object[] dataPool);

    /// <summary>
    /// This class is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public class NameBinder
    {
        public class ReferenceLink
        {
            private readonly int _index;

            internal ReferenceLink(int index)
            {
                _index = index;
            }

            public object Get(object[] dataPool)
            {
                return dataPool[_index];
            }

            public void Set(object[] dataPool, object value)
            {
                dataPool[_index] = value;
                if (OnChange != null)
                    OnChange(this, dataPool);
            }

            public event ChangeValueAction OnChange;
        }

        private int _slotIndex = 0;
        private readonly List<NameSlot> _slots = new List<NameSlot>();

        public int Length => _slotIndex;

        public ReferenceLink PushVar(XmlQualifiedName name)
        {
            ReferenceLink id = NewReference();
            _slots.Add(new NameSlot(id, name));
            return id;
        }

        public void PopVar()
        {
            _slots.RemoveAt(_slots.Count - 1);
        }

        public ReferenceLink VarIndexByName(XmlQualifiedName name)
        {
            for (int k = _slots.Count - 1; k >= 0; k--)
                if (_slots[k].name.Equals(name))
                    return _slots[k].id;
            throw new XPath2Exception("XPST0008", Resources.XPST0008, name.ToString());
        }

        public ReferenceLink NewReference()
        {
            return new ReferenceLink(_slotIndex++);
        }

        private class NameSlot
        {
            public readonly ReferenceLink id;
            public readonly XmlQualifiedName name;

            public NameSlot(ReferenceLink id, XmlQualifiedName name)
            {
                this.id = id;
                this.name = name;
            }
        }
    }
}