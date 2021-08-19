// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using Wmhelp.XPath2.MS;
using Wmhelp.XPath2.Value;

namespace Wmhelp.XPath2
{
    public enum XmlTypeCardinality
    {
        One,
        ZeroOrOne,
        OneOrMore,
        ZeroOrMore
    }

    public class SequenceType
    {
        public XmlTypeCode TypeCode { get; set; }

        public XmlQualifiedNameTest NameTest { get; set; }

        public XmlTypeCardinality Cardinality { get; set; }

        public XmlSchemaType SchemaType { get; set; }

        public XmlSchemaElement SchemaElement { get; set; }

        public XmlSchemaAttribute SchemaAttribute { get; set; }

        public bool Nillable { get; set; }

        public Type ParameterType { get; set; }

        public Type ItemType { get; }

        public bool IsNode { get; }

        public SequenceType(XmlTypeCode typeCode)
            : this(typeCode, XmlQualifiedNameTest.Wildcard)
        {
        }

        public SequenceType(XmlTypeCode typeCode, XmlTypeCardinality cardinality)
            : this(typeCode, XmlQualifiedNameTest.Wildcard)
        {
            Cardinality = cardinality;
        }

        public SequenceType(XmlTypeCode typeCode, XmlTypeCardinality cardinality, Type clrType)
            : this(typeCode, XmlQualifiedNameTest.Wildcard)
        {
            Cardinality = cardinality;
            ParameterType = clrType;
            IsNode = TypeCodeIsNodeType(TypeCode);
            if (TypeCode != XmlTypeCode.Item && !IsNode)
                SchemaType = XmlSchemaType.GetBuiltInSimpleType(TypeCode);
        }

        public SequenceType(XmlTypeCode typeCode, XmlQualifiedNameTest nameTest)
        {
            TypeCode = typeCode;
            Cardinality = XmlTypeCardinality.One;
            NameTest = nameTest;
            IsNode = TypeCodeIsNodeType(TypeCode);
            if (TypeCode != XmlTypeCode.Item && !IsNode)
                SchemaType = XmlSchemaType.GetBuiltInSimpleType(TypeCode);
            ItemType = TypeCodeToItemType(TypeCode, SchemaType);
        }

        public SequenceType(XmlTypeCode typeCode, XmlQualifiedNameTest nameTest, XmlSchemaType schemaType, bool isNillable = false)
        {
            TypeCode = typeCode;
            Cardinality = XmlTypeCardinality.One;
            NameTest = nameTest;
            SchemaType = schemaType;
            Nillable = isNillable;
            IsNode = TypeCodeIsNodeType(TypeCode);
            ItemType = TypeCodeToItemType(TypeCode, SchemaType);
        }

        public SequenceType(XmlSchemaElement schemaElement)
        {
            TypeCode = XmlTypeCode.Element;
            SchemaElement = schemaElement;
            IsNode = true;
            ItemType = TypeCodeToItemType(TypeCode, SchemaType);
        }

        public SequenceType(XmlSchemaAttribute schemaAttribute)
        {
            TypeCode = XmlTypeCode.Attribute;
            SchemaAttribute = schemaAttribute;
            IsNode = true;
            ItemType = TypeCodeToItemType(TypeCode, SchemaType);
        }


        public SequenceType(XmlSchemaType schemaType, XmlTypeCardinality cardinality, Type clrType)
        {
            TypeCode = schemaType.TypeCode;
            SchemaType = schemaType;
            Cardinality = cardinality;
            ParameterType = clrType;
            IsNode = TypeCodeIsNodeType(TypeCode);
            ItemType = TypeCodeToItemType(TypeCode, SchemaType);
        }

        public SequenceType(Type clrType, XmlTypeCardinality cardinality)
        {
            TypeCode = GetXmlTypeCode(clrType);
            if (TypeCode != XmlTypeCode.Item && !IsNode)
                SchemaType = XmlSchemaType.GetBuiltInSimpleType(TypeCode);
            ParameterType = clrType;
            Cardinality = cardinality;
            IsNode = TypeCodeIsNodeType(TypeCode);
            ItemType = TypeCodeToItemType(TypeCode, SchemaType);
        }

        public SequenceType(XmlTypeCode typeCode, IXmlSchemaInfo schemaInfo, Type clrType)
        {
            TypeCode = typeCode;
            NameTest = XmlQualifiedNameTest.Wildcard;
            Cardinality = XmlTypeCardinality.One;
            SchemaType = schemaInfo.SchemaType;
            SchemaElement = schemaInfo.SchemaElement;
            SchemaAttribute = schemaInfo.SchemaAttribute;
            ParameterType = clrType;
            IsNode = TypeCodeIsNodeType(TypeCode);
            ItemType = TypeCodeToItemType(TypeCode, SchemaType);
        }

        public SequenceType(SequenceType src)
        {
            TypeCode = src.TypeCode;
            NameTest = src.NameTest;
            Cardinality = src.Cardinality;
            SchemaType = src.SchemaType;
            SchemaElement = src.SchemaElement;
            SchemaAttribute = src.SchemaAttribute;
            Nillable = src.Nillable;
            ParameterType = src.ParameterType;
            IsNode = src.IsNode;
            ItemType = src.ItemType;
        }

        public Type ValueType
        {
            get
            {
                if (Cardinality == XmlTypeCardinality.ZeroOrMore ||
                    Cardinality == XmlTypeCardinality.OneOrMore)
                    return typeof(XPath2NodeIterator);
                if (IsNode)
                    return typeof(XPathNavigator);
                if (Cardinality == XmlTypeCardinality.One)
                    return ItemType;
                return typeof(Object);
            }
        }

        public Type AtomizedValueType
        {
            get
            {
                if (IsNode)
                {
                    switch (TypeCode)
                    {
                        case XmlTypeCode.Text:
                        case XmlTypeCode.ProcessingInstruction:
                        case XmlTypeCode.Comment:
                        case XmlTypeCode.UntypedAtomic:
                            return typeof(UntypedAtomic);

                        default:
                            if (SchemaType != null)
                                return SchemaType.Datatype.ValueType;
                            else if (SchemaElement != null)
                            {
                                if (SchemaElement.ElementSchemaType != null &&
                                    SchemaElement.ElementSchemaType.Datatype != null)
                                    return SchemaElement.ElementSchemaType.Datatype.ValueType;
                            }
                            else if (SchemaAttribute != null)
                            {
                                if (SchemaAttribute.AttributeSchemaType != null &&
                                    SchemaAttribute.AttributeSchemaType.Datatype != null)
                                    return SchemaAttribute.AttributeSchemaType.Datatype.ValueType;
                            }
                            return typeof(UntypedAtomic);
                    }
                }
                else
                    return ItemType;
            }
        }

        public bool IsNumeric
        {
            get
            {
                switch (TypeCode)
                {
                    case XmlTypeCode.Decimal:
                    case XmlTypeCode.Float:
                    case XmlTypeCode.Double:
                    case XmlTypeCode.Integer:
                    case XmlTypeCode.NonPositiveInteger:
                    case XmlTypeCode.NegativeInteger:
                    case XmlTypeCode.Long:
                    case XmlTypeCode.Int:
                    case XmlTypeCode.Short:
                    case XmlTypeCode.Byte:
                    case XmlTypeCode.NonNegativeInteger:
                    case XmlTypeCode.UnsignedLong:
                    case XmlTypeCode.UnsignedInt:
                    case XmlTypeCode.UnsignedShort:
                    case XmlTypeCode.UnsignedByte:
                    case XmlTypeCode.PositiveInteger:
                        return true;
                }
                return false;
            }
        }

        public bool IsUntypedAtomic => TypeCode == XmlTypeCode.UntypedAtomic;

        private bool MatchName(XPathNavigator nav, XPath2Context context)
        {
            return (NameTest.IsNamespaceWildcard || NameTest.Namespace == nav.NamespaceURI) &&
                   (NameTest.IsNameWildcard || NameTest.Name == nav.LocalName);
        }

        public XPathNodeType GetNodeKind()
        {
            switch (TypeCode)
            {
                case XmlTypeCode.Item:
                    return XPathNodeType.All;
                case XmlTypeCode.Document:
                    return XPathNodeType.Root;
                case XmlTypeCode.Element:
                    return XPathNodeType.Element;
                case XmlTypeCode.Attribute:
                    return XPathNodeType.Attribute;
                case XmlTypeCode.Namespace:
                    return XPathNodeType.Namespace;
                case XmlTypeCode.Text:
                    return XPathNodeType.Text;
                case XmlTypeCode.Comment:
                    return XPathNodeType.Comment;
                case XmlTypeCode.ProcessingInstruction:
                    return XPathNodeType.ProcessingInstruction;
                default:
                    throw new InvalidOperationException("GetNodeKind()");
            }
        }

        public bool Match(XPathItem item, XPath2Context context)
        {
            switch (TypeCode)
            {
                case XmlTypeCode.None:
                    return false;

                case XmlTypeCode.Item:
                    return true;

                case XmlTypeCode.Node:
                    return item.IsNode;

                case XmlTypeCode.AnyAtomicType:
                    return !item.IsNode;

                case XmlTypeCode.UntypedAtomic:
                    return !item.IsNode && item.GetSchemaType() == XmlSchema.UntypedAtomic;

                case XmlTypeCode.Document:
                    {
                        XPathNavigator nav = item as XPathNavigator;
                        if (nav != null)
                        {
                            if (nav.NodeType == XPathNodeType.Root)
                            {
                                XPathNavigator cur = nav.Clone();
                                if (SchemaElement == null)
                                {
                                    if (cur.MoveToChild(XPathNodeType.Element) && MatchName(cur, context))
                                    {
                                        if (SchemaType == null || SchemaType == XmlSchema.UntypedAtomic)
                                            return true;
                                        IXmlSchemaInfo schemaInfo = cur.SchemaInfo;
                                        if (schemaInfo != null)
                                        {
                                            if (XmlSchemaType.IsDerivedFrom(schemaInfo.SchemaType, SchemaType, XmlSchemaDerivationMethod.Empty))
                                                return !schemaInfo.IsNil || Nillable;
                                        }
                                        else
                                            return XmlSchemaType.IsDerivedFrom(XmlSchema.UntypedAtomic, SchemaType, XmlSchemaDerivationMethod.Empty);
                                    }
                                }
                                else
                                {
                                    if (!cur.MoveToChild(XPathNodeType.Element))
                                        return false;
                                    IXmlSchemaInfo schemaInfo = cur.SchemaInfo;
                                    if (schemaInfo != null)
                                        return schemaInfo.SchemaElement.QualifiedName == SchemaElement.QualifiedName;
                                }
                            }
                        }
                    }
                    break;

                case XmlTypeCode.Element:
                    {
                        XPathNavigator nav = item as XPathNavigator;
                        if (nav != null && nav.NodeType == XPathNodeType.Element)
                        {
                            if (SchemaElement == null)
                            {
                                if (MatchName(nav, context))
                                {
                                    if (SchemaType == null || SchemaType == XmlSchema.UntypedAtomic)
                                        return true;
                                    IXmlSchemaInfo schemaInfo = nav.SchemaInfo;
                                    if (schemaInfo != null)
                                    {
                                        if (XmlSchemaType.IsDerivedFrom(schemaInfo.SchemaType, SchemaType,
                                            XmlSchemaDerivationMethod.Empty))
                                            return !schemaInfo.IsNil || Nillable;
                                    }
                                    else
                                        return XmlSchemaType.IsDerivedFrom(XmlSchema.UntypedAtomic, SchemaType,
                                            XmlSchemaDerivationMethod.Empty);
                                }
                            }
                            else
                            {
                                IXmlSchemaInfo schemaInfo = nav.SchemaInfo;
                                if (schemaInfo != null)
                                    return schemaInfo.SchemaElement.QualifiedName == SchemaElement.QualifiedName;
                            }
                        }
                    }
                    break;

                case XmlTypeCode.Attribute:
                    {
                        XPathNavigator nav = item as XPathNavigator;
                        if (nav != null && nav.NodeType == XPathNodeType.Attribute)
                        {
                            if (SchemaAttribute == null)
                            {
                                if (MatchName(nav, context))
                                {
                                    if (SchemaType == null || SchemaType == XmlSchema.UntypedAtomic)
                                        return true;
                                    IXmlSchemaInfo schemaInfo = nav.SchemaInfo;
                                    if (schemaInfo == null)
                                        return XmlSchemaType.IsDerivedFrom(XmlSchema.UntypedAtomic, SchemaType,
                                            XmlSchemaDerivationMethod.Empty);
                                    else
                                        return XmlSchemaType.IsDerivedFrom(schemaInfo.SchemaType, SchemaType,
                                            XmlSchemaDerivationMethod.Empty);
                                }
                            }
                            else
                            {
                                IXmlSchemaInfo schemaInfo = nav.SchemaInfo;
                                if (schemaInfo != null)
                                    return schemaInfo.SchemaAttribute.QualifiedName == SchemaAttribute.QualifiedName;
                            }
                        }
                    }
                    break;

                case XmlTypeCode.ProcessingInstruction:
                    {
                        XPathNavigator nav = item as XPathNavigator;
                        if (nav != null)
                            return (nav.NodeType == XPathNodeType.ProcessingInstruction &&
                                    (NameTest.IsNameWildcard || NameTest.Name == nav.Name));
                    }
                    break;

                case XmlTypeCode.Comment:
                    {
                        XPathNavigator nav = item as XPathNavigator;
                        if (nav != null)
                            return nav.NodeType == XPathNodeType.Comment;
                    }
                    break;

                case XmlTypeCode.Text:
                    {
                        XPathNavigator nav = item as XPathNavigator;
                        if (nav != null)
                            return nav.NodeType == XPathNodeType.Text ||
                                   nav.NodeType == XPathNodeType.SignificantWhitespace;
                    }
                    break;

                case XmlTypeCode.PositiveInteger:
                    switch (item.GetSchemaType().TypeCode)
                    {
                        case XmlTypeCode.Byte:
                        case XmlTypeCode.Short:
                        case XmlTypeCode.Int:
                        case XmlTypeCode.Long:
                        case XmlTypeCode.Integer:
                            return (decimal)item.ValueAs(typeof(Decimal)) > 0;
                    }
                    break;

                case XmlTypeCode.NegativeInteger:
                    switch (item.GetSchemaType().TypeCode)
                    {
                        case XmlTypeCode.Byte:
                        case XmlTypeCode.Short:
                        case XmlTypeCode.Int:
                        case XmlTypeCode.Long:
                        case XmlTypeCode.Integer:
                            return (decimal)item.ValueAs(typeof(Decimal)) < 0;
                    }
                    break;

                case XmlTypeCode.NonPositiveInteger:
                    switch (item.GetSchemaType().TypeCode)
                    {
                        case XmlTypeCode.Byte:
                        case XmlTypeCode.Short:
                        case XmlTypeCode.Int:
                        case XmlTypeCode.Long:
                        case XmlTypeCode.Integer:
                            return (decimal)item.ValueAs(typeof(Decimal)) <= 0;
                    }
                    break;

                case XmlTypeCode.NonNegativeInteger:
                    switch (item.GetSchemaType().TypeCode)
                    {
                        case XmlTypeCode.Byte:
                        case XmlTypeCode.Short:
                        case XmlTypeCode.Int:
                        case XmlTypeCode.Long:
                        case XmlTypeCode.Integer:
                            return (decimal)item.ValueAs(typeof(Decimal)) >= 0;

                        case XmlTypeCode.UnsignedByte:
                        case XmlTypeCode.UnsignedShort:
                        case XmlTypeCode.UnsignedInt:
                        case XmlTypeCode.UnsignedLong:
                            return true;
                    }
                    break;

                case XmlTypeCode.Integer:
                    switch (item.GetSchemaType().TypeCode)
                    {
                        case XmlTypeCode.Byte:
                        case XmlTypeCode.Short:
                        case XmlTypeCode.Int:
                        case XmlTypeCode.Long:
                        case XmlTypeCode.Integer:
                        case XmlTypeCode.UnsignedByte:
                        case XmlTypeCode.UnsignedShort:
                        case XmlTypeCode.UnsignedInt:
                        case XmlTypeCode.UnsignedLong:
                            return true;

                        case XmlTypeCode.Decimal:
                            decimal value = (decimal)item.ValueAs(typeof(Decimal));
                            return value == Math.Truncate(value);
                    }
                    break;

                case XmlTypeCode.Entity:
                    return (item.GetSchemaType().TypeCode == XmlTypeCode.String) ||
                           (item.GetSchemaType().TypeCode == XmlTypeCode.Entity);

                default:
                    {
                        if (item.XmlType != null)
                            return XmlSchemaType.IsDerivedFrom(item.XmlType, SchemaType, XmlSchemaDerivationMethod.Empty);
                    }
                    break;
            }
            return false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            switch (TypeCode)
            {
                case XmlTypeCode.AnyAtomicType:
                    sb.Append("AnyAtomicType");
                    break;

                case XmlTypeCode.UntypedAtomic:
                    sb.Append("UntypedAtomic");
                    break;

                case XmlTypeCode.None:
                    sb.Append("empty-sequence()");
                    break;

                case XmlTypeCode.Item:
                    sb.Append("item()");
                    break;

                case XmlTypeCode.Node:
                    sb.Append("node()");
                    break;

                case XmlTypeCode.Document:
                    sb.Append("document-node(");
                    if (SchemaElement == null)
                    {
                        if (!NameTest.IsWildcard || SchemaType != null)
                        {
                            sb.Append("element(");
                            sb.Append(NameTest);
                            if (SchemaType != null)
                            {
                                sb.Append(",");
                                sb.Append(SchemaType);
                                if (Nillable)
                                    sb.Append("?");
                            }
                            else
                                sb.Append(", xs:untyped");
                            sb.Append(")");
                        }
                    }
                    else
                    {
                        sb.Append("schema-element(");
                        sb.Append(SchemaElement.QualifiedName);
                        sb.Append(")");
                    }
                    sb.Append(")");
                    break;

                case XmlTypeCode.Element:
                    if (SchemaElement == null)
                    {
                        sb.Append("element(");
                        sb.Append(NameTest);
                        if (SchemaType != null)
                        {
                            sb.Append(",");
                            sb.Append(SchemaType);
                            if (Nillable)
                                sb.Append("?");
                        }
                        else
                            sb.Append(", xs:untyped");
                    }
                    else
                    {
                        sb.Append("schema-element(");
                        sb.Append(SchemaElement.QualifiedName);
                    }
                    sb.Append(")");
                    break;

                case XmlTypeCode.Attribute:
                    if (SchemaAttribute == null)
                    {
                        sb.Append("attribute(");
                        sb.Append(NameTest);
                        if (SchemaType != null)
                        {
                            sb.Append(",");
                            sb.Append(SchemaType);
                        }
                    }
                    else
                    {
                        sb.Append("schema-attribute(");
                        sb.Append(SchemaAttribute.QualifiedName);
                    }
                    sb.Append(")");
                    break;

                case XmlTypeCode.ProcessingInstruction:
                    sb.Append("processing-instruction(");
                    break;

                case XmlTypeCode.Comment:
                    sb.Append("comment()");
                    break;

                case XmlTypeCode.Text:
                    sb.Append("text()");
                    break;

                case XmlTypeCode.String:
                    sb.Append("xs:string");
                    break;

                case XmlTypeCode.Boolean:
                    sb.Append("xs:boolean");
                    break;

                case XmlTypeCode.Decimal:
                    sb.Append("xs:decimal");
                    break;

                case XmlTypeCode.Float:
                    sb.Append("xs:float");
                    break;

                case XmlTypeCode.Double:
                    sb.Append("xs:double");
                    break;

                case XmlTypeCode.Duration:
                    sb.Append("xs:Duration");
                    break;

                case XmlTypeCode.DateTime:
                    sb.Append("xs:dateTime");
                    break;

                case XmlTypeCode.Time:
                    sb.Append("xs:time");
                    break;

                case XmlTypeCode.Date:
                    sb.Append("xs:date");
                    break;

                case XmlTypeCode.GYearMonth:
                    sb.Append("xs:gYearMonth");
                    break;

                case XmlTypeCode.GYear:
                    sb.Append("xs:gYear");
                    break;

                case XmlTypeCode.GMonthDay:
                    sb.Append("xs:gMonthDay");
                    break;

                case XmlTypeCode.GDay:
                    sb.Append("xs:gDay");
                    break;

                case XmlTypeCode.GMonth:
                    sb.Append("xs:gMonth");
                    break;

                case XmlTypeCode.HexBinary:
                    sb.Append("xs:hexBinary");
                    break;

                case XmlTypeCode.Base64Binary:
                    sb.Append("xs:base64Binary");
                    break;

                case XmlTypeCode.AnyUri:
                    sb.Append("xs:anyURI");
                    break;

                case XmlTypeCode.QName:
                    sb.Append("xs:QName");
                    break;

                case XmlTypeCode.Notation:
                    sb.Append("xs:NOTATION");
                    break;

                case XmlTypeCode.NormalizedString:
                    sb.Append("xs:normalizedString");
                    break;

                case XmlTypeCode.Token:
                    sb.Append("xs:token");
                    break;

                case XmlTypeCode.Language:
                    sb.Append("xs:language");
                    break;

                case XmlTypeCode.NmToken:
                    if (SchemaType == XmlSchema.NMTOKENS)
                        sb.Append("xs:NMTOKENS");
                    else
                        sb.Append("xs:NMTOKEN");
                    break;

                case XmlTypeCode.Name:
                    sb.Append("xs:Name");
                    break;

                case XmlTypeCode.NCName:
                    sb.Append("xs:NCName");
                    break;

                case XmlTypeCode.Id:
                    sb.Append("xs:ID");
                    break;

                case XmlTypeCode.Idref:
                    if (SchemaType == XmlSchema.IDREFS)
                        sb.Append("xs:IDREFS");
                    else
                        sb.Append("xs:IDREF");
                    break;

                case XmlTypeCode.Entity:
                    if (SchemaType == XmlSchema.ENTITIES)
                        sb.Append("xs:ENTITYS");
                    else
                        sb.Append("xs:ENTITY");
                    break;

                case XmlTypeCode.Integer:
                    sb.Append("xs:integer");
                    break;

                case XmlTypeCode.NonPositiveInteger:
                    sb.Append("xs:nonPositiveInteger");
                    break;

                case XmlTypeCode.NegativeInteger:
                    sb.Append("xs:negativeInteger");
                    break;

                case XmlTypeCode.Long:
                    sb.Append("xs:long");
                    break;

                case XmlTypeCode.Int:
                    sb.Append("xs:int");
                    break;

                case XmlTypeCode.Short:
                    sb.Append("xs:short");
                    break;

                case XmlTypeCode.Byte:
                    sb.Append("xs:byte");
                    break;

                case XmlTypeCode.NonNegativeInteger:
                    sb.Append("xs:nonNegativeInteger");
                    break;

                case XmlTypeCode.UnsignedLong:
                    sb.Append("xs:unsignedLong");
                    break;

                case XmlTypeCode.UnsignedInt:
                    sb.Append("xs:unsignedInt");
                    break;

                case XmlTypeCode.UnsignedShort:
                    sb.Append("xs:unsignedShort");
                    break;

                case XmlTypeCode.UnsignedByte:
                    sb.Append("xs:unsignedByte");
                    break;

                case XmlTypeCode.PositiveInteger:
                    sb.Append("xs:positiveInteger");
                    break;

                case XmlTypeCode.DayTimeDuration:
                    sb.Append("xs:dayTimeDuration");
                    break;

                case XmlTypeCode.YearMonthDuration:
                    sb.Append("xs:yearMonthDuration");
                    break;

                default:
                    sb.Append("[]");
                    break;
            }
            switch (Cardinality)
            {
                case XmlTypeCardinality.OneOrMore:
                    sb.Append("+");
                    break;

                case XmlTypeCardinality.ZeroOrMore:
                    sb.Append("*");
                    break;

                case XmlTypeCardinality.ZeroOrOne:
                    sb.Append("?");
                    break;
            }
            return sb.ToString();
        }

        public static XmlTypeCode GetXmlTypeCode(object value)
        {
            if (value is XPathNavigator)
            {
                XPathNavigator nav = (XPathNavigator)value;
                switch (nav.NodeType)
                {
                    case XPathNodeType.Attribute:
                        return XmlTypeCode.Attribute;
                    case XPathNodeType.Comment:
                        return XmlTypeCode.Comment;
                    case XPathNodeType.Element:
                        return XmlTypeCode.Element;
                    case XPathNodeType.Namespace:
                        return XmlTypeCode.Namespace;
                    case XPathNodeType.ProcessingInstruction:
                        return XmlTypeCode.ProcessingInstruction;
                    case XPathNodeType.Root:
                        return XmlTypeCode.Document;
                    case XPathNodeType.SignificantWhitespace:
                    case XPathNodeType.Whitespace:
                    case XPathNodeType.Text:
                        return XmlTypeCode.Text;
                    default:
                        return XmlTypeCode.None;
                }
            }
            if (value is XPathItem)
            {
                XPathItem item = (XPathItem)value;
                if (item.XmlType == null)
                    return XmlTypeCode.UntypedAtomic;
                return item.XmlType.TypeCode;
            }
            return GetXmlTypeCode(value.GetType());
        }

        public static bool TypeCodeIsNodeType(XmlTypeCode typeCode)
        {
            switch (typeCode)
            {
                case XmlTypeCode.Node:
                case XmlTypeCode.Element:
                case XmlTypeCode.Attribute:
                case XmlTypeCode.Document:
                case XmlTypeCode.Comment:
                case XmlTypeCode.Text:
                case XmlTypeCode.ProcessingInstruction:
                    return true;
            }
            return false;
        }

        public static Type TypeCodeToItemType(XmlTypeCode typeCode, XmlSchemaType schemaType)
        {
            switch (typeCode)
            {
                case XmlTypeCode.Boolean:
                    return typeof(Boolean);
                case XmlTypeCode.Short:
                    return typeof(Int16);
                case XmlTypeCode.Int:
                    return typeof(Int32);
                case XmlTypeCode.Long:
                    return typeof(Int64);
                case XmlTypeCode.UnsignedShort:
                    return typeof(UInt16);
                case XmlTypeCode.UnsignedInt:
                    return typeof(UInt32);
                case XmlTypeCode.UnsignedLong:
                    return typeof(UInt64);
                case XmlTypeCode.Byte:
                    return typeof(SByte);
                case XmlTypeCode.UnsignedByte:
                    return typeof(Byte);
                case XmlTypeCode.Float:
                    return typeof(Single);
                case XmlTypeCode.Decimal:
                    return typeof(Decimal);
                case XmlTypeCode.Integer:
                case XmlTypeCode.PositiveInteger:
                case XmlTypeCode.NegativeInteger:
                case XmlTypeCode.NonPositiveInteger:
                case XmlTypeCode.NonNegativeInteger:
                    return typeof(Integer);
                case XmlTypeCode.Double:
                    return typeof(Double);
                case XmlTypeCode.DateTime:
                    return typeof(DateTimeValue);
                case XmlTypeCode.Date:
                    return typeof(DateValue);
                case XmlTypeCode.Time:
                    return typeof(TimeValue);
                case XmlTypeCode.AnyUri:
                    return typeof(AnyUriValue);
                case XmlTypeCode.String:
                case XmlTypeCode.NormalizedString:
                case XmlTypeCode.Token:
                case XmlTypeCode.Language:
                case XmlTypeCode.Name:
                case XmlTypeCode.NCName:
                case XmlTypeCode.Id:
                case XmlTypeCode.Idref:
                    if (schemaType == XmlSchema.IDREFS)
                        return typeof(IDREFSValue);
                    else
                        return typeof(String);
                case XmlTypeCode.NmToken:
                    if (schemaType == XmlSchema.NMTOKENS)
                        return typeof(NMTOKENSValue);
                    else
                        return typeof(String);
                case XmlTypeCode.Entity:
                    if (schemaType == XmlSchema.ENTITIES)
                        return typeof(ENTITIESValue);
                    else
                        return typeof(String);
                case XmlTypeCode.UntypedAtomic:
                    return typeof(UntypedAtomic);
                case XmlTypeCode.Duration:
                    return typeof(DurationValue);
                case XmlTypeCode.DayTimeDuration:
                    return typeof(DayTimeDurationValue);
                case XmlTypeCode.YearMonthDuration:
                    return typeof(YearMonthDurationValue);
                case XmlTypeCode.GYearMonth:
                    return typeof(GYearMonthValue);
                case XmlTypeCode.GYear:
                    return typeof(GYearValue);
                case XmlTypeCode.GMonth:
                    return typeof(GMonthValue);
                case XmlTypeCode.GMonthDay:
                    return typeof(GMonthDayValue);
                case XmlTypeCode.GDay:
                    return typeof(GDayValue);
                case XmlTypeCode.QName:
                    return typeof(QNameValue);
                case XmlTypeCode.HexBinary:
                    return typeof(HexBinaryValue);
                case XmlTypeCode.Base64Binary:
                    return typeof(Base64BinaryValue);
                default:
                    return typeof(Object);
            }
        }

        public static XmlTypeCode GetXmlTypeCode(Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case System.TypeCode.Boolean:
                    return XmlTypeCode.Boolean;
                case System.TypeCode.Int16:
                    return XmlTypeCode.Short;
                case System.TypeCode.Int32:
                    return XmlTypeCode.Int;
                case System.TypeCode.Int64:
                    return XmlTypeCode.Long;
                case System.TypeCode.UInt16:
                    return XmlTypeCode.UnsignedShort;
                case System.TypeCode.UInt32:
                    return XmlTypeCode.UnsignedInt;
                case System.TypeCode.UInt64:
                    return XmlTypeCode.UnsignedLong;
                case System.TypeCode.SByte:
                    return XmlTypeCode.Byte;
                case System.TypeCode.Byte:
                    return XmlTypeCode.UnsignedByte;
                case System.TypeCode.Single:
                    return XmlTypeCode.Float;
                case System.TypeCode.Decimal:
                    return XmlTypeCode.Decimal;
                case System.TypeCode.Double:
                    return XmlTypeCode.Double;
                case System.TypeCode.Char:
                case System.TypeCode.String:
                    return XmlTypeCode.String;
                default:
                    if (type == typeof(XPathNavigator))
                        return XmlTypeCode.Node;
                    if (type == typeof(UntypedAtomic))
                        return XmlTypeCode.UntypedAtomic;
                    if (type == typeof(Integer))
                        return XmlTypeCode.Integer;
                    if (type == typeof(DateTimeValue))
                        return XmlTypeCode.DateTime;
                    if (type == typeof(DateValue))
                        return XmlTypeCode.Date;
                    if (type == typeof(TimeValue))
                        return XmlTypeCode.Time;
                    if (type == typeof(DurationValue))
                        return XmlTypeCode.Duration;
                    if (type == typeof(YearMonthDurationValue))
                        return XmlTypeCode.YearMonthDuration;
                    if (type == typeof(DayTimeDurationValue))
                        return XmlTypeCode.DayTimeDuration;
                    if (type == typeof(GYearMonthValue))
                        return XmlTypeCode.GYearMonth;
                    if (type == typeof(GYearValue))
                        return XmlTypeCode.GYear;
                    if (type == typeof(GDayValue))
                        return XmlTypeCode.GDay;
                    if (type == typeof(GMonthValue))
                        return XmlTypeCode.GMonth;
                    if (type == typeof(GMonthDayValue))
                        return XmlTypeCode.GMonthDay;
                    if (type == typeof(QNameValue))
                        return XmlTypeCode.QName;
                    if (type == typeof(AnyUriValue))
                        return XmlTypeCode.AnyUri;
                    if (type == typeof(HexBinaryValue))
                        return XmlTypeCode.HexBinary;
                    if (type == typeof(Base64BinaryValue))
                        return XmlTypeCode.Base64Binary;
                    if (type == typeof(IDREFSValue))
                        return XmlTypeCode.Idref;
                    if (type == typeof(ENTITIESValue))
                        return XmlTypeCode.Entity;
                    if (type == typeof(NMTOKENSValue))
                        return XmlTypeCode.NmToken;
                    return XmlTypeCode.Item;
            }
        }

        public bool IsDerivedFrom(SequenceType src)
        {
            switch (src.TypeCode)
            {
                case XmlTypeCode.Node:
                    if (!IsNode)
                        return false;
                    break;

                case XmlTypeCode.AnyAtomicType:
                case XmlTypeCode.UntypedAtomic:
                    if (IsNode)
                        return false;
                    break;

                case XmlTypeCode.Document:
                    if (TypeCode != XmlTypeCode.Document ||
                        SchemaElement != src.SchemaElement)
                        return false;
                    break;

                case XmlTypeCode.Element:
                    if (TypeCode != XmlTypeCode.Element ||
                        SchemaElement != src.SchemaElement)
                        return false;
                    break;

                case XmlTypeCode.Attribute:
                    if (TypeCode != XmlTypeCode.Attribute ||
                        SchemaAttribute != src.SchemaAttribute)
                        return false;
                    break;

                case XmlTypeCode.ProcessingInstruction:
                    if (TypeCode != XmlTypeCode.ProcessingInstruction)
                        return false;
                    break;

                case XmlTypeCode.Comment:
                    if (TypeCode != XmlTypeCode.Comment)
                        return false;
                    break;

                case XmlTypeCode.Text:
                    if (TypeCode != XmlTypeCode.Text)
                        return false;
                    break;
            }
            if (SchemaType != null || src.SchemaType != null)
            {
                if (SchemaType != null && src.SchemaType != null)
                {
                    if (!XmlSchemaType.IsDerivedFrom(SchemaType, src.SchemaType, XmlSchemaDerivationMethod.Empty))
                        return false;
                }
                else
                    return false;
            }

            if (Cardinality != src.Cardinality)
            {
                if ((Cardinality == XmlTypeCardinality.ZeroOrOne ||
                     Cardinality == XmlTypeCardinality.ZeroOrMore) &&
                    (src.Cardinality == XmlTypeCardinality.One ||
                     src.Cardinality == XmlTypeCardinality.OneOrMore))
                    return false;
                if (Cardinality == XmlTypeCardinality.One &&
                    src.Cardinality == XmlTypeCardinality.OneOrMore)
                    return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            SequenceType dest = obj as SequenceType;
            if (!ReferenceEquals(obj, null))
                return TypeCode == dest.TypeCode &&
                       SchemaElement == dest.SchemaElement &&
                       SchemaAttribute == dest.SchemaAttribute &&
                       SchemaType == dest.SchemaType &&
                       Cardinality == dest.Cardinality;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static SequenceType Create(string name)
        {
            if (name == string.Empty || name == "empty-sequence()")
                return null;

            XmlTypeCardinality cardinality = XmlTypeCardinality.One;
            if (name.EndsWith("?"))
            {
                cardinality = XmlTypeCardinality.ZeroOrOne;
                name = name.Substring(1, name.Length - 1);
            }
            else if (name.EndsWith("+"))
            {
                cardinality = XmlTypeCardinality.OneOrMore;
                name = name.Substring(1, name.Length - 1);
            }
            else if (name.EndsWith("*"))
            {
                cardinality = XmlTypeCardinality.ZeroOrMore;
                name = name.Substring(1, name.Length - 1);
            }

            if (name.Equals("xs:AnyAtomicType"))
                return new SequenceType(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.AnyAtomicType), cardinality, null);

            if (name.Equals("item()"))
                return new SequenceType(XmlTypeCode.Item, cardinality);

            if (name.Equals("node()"))
                return new SequenceType(XmlTypeCode.Node, cardinality);

            string prefix;
            string localName;
            QNameParser.Split(name, out prefix, out localName);
            if (prefix != "xs")
                throw new ArgumentException("name");

            XmlSchemaType schemaType =
                XmlSchemaType.GetBuiltInSimpleType(new XmlQualifiedName(localName, XmlReservedNs.NsXs));
            if (schemaType == null)
                throw new ArgumentException("name");

            return new SequenceType(schemaType, cardinality, null);
        }

        public static class XmlSchema
        {
            public readonly static XmlSchemaType AnySimpleType =
                XmlSchemaType.GetBuiltInSimpleType(new XmlQualifiedName("anySimpleType", XmlReservedNs.NsXs));

            public readonly static XmlSchemaType AnyType =
                XmlSchemaType.GetBuiltInComplexType(new XmlQualifiedName("anyType", XmlReservedNs.NsXs));

            public readonly static XmlSchemaType AnyAtomicType = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.AnyAtomicType);
            public readonly static XmlSchemaType UntypedAtomic = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.UntypedAtomic);
            public readonly static XmlSchemaType Integer = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Integer);
            public readonly static XmlSchemaType DateTime = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.DateTime);
            public readonly static XmlSchemaType Date = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Date);
            public readonly static XmlSchemaType Time = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Time);
            public readonly static XmlSchemaType Duration = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Duration);

            public readonly static XmlSchemaType YearMonthDuration =
                XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.YearMonthDuration);

            public readonly static XmlSchemaType DayTimeDuration = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.DayTimeDuration);
            public readonly static XmlSchemaType GYearMonth = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.GYearMonth);
            public readonly static XmlSchemaType GYear = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.GYear);
            public readonly static XmlSchemaType GDay = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.GDay);
            public readonly static XmlSchemaType GMonth = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.GMonth);
            public readonly static XmlSchemaType GMonthDay = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.GMonthDay);
            public readonly static XmlSchemaType QName = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.QName);
            public readonly static XmlSchemaType HexBinary = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.HexBinary);
            public readonly static XmlSchemaType Base64Binary = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Base64Binary);

            public readonly static XmlSchemaType IDREFS =
                XmlSchemaType.GetBuiltInSimpleType(new XmlQualifiedName("IDREFS", XmlReservedNs.NsXs));

            public readonly static XmlSchemaType NMTOKENS =
                XmlSchemaType.GetBuiltInSimpleType(new XmlQualifiedName("NMTOKENS", XmlReservedNs.NsXs));

            public readonly static XmlSchemaType ENTITIES =
                XmlSchemaType.GetBuiltInSimpleType(new XmlQualifiedName("ENTITIES", XmlReservedNs.NsXs));
        }

        #region build-in

        internal static SequenceType Void = new SequenceType(XmlTypeCode.None);
        internal static SequenceType Item = new SequenceType(XmlTypeCode.Item);
        internal static SequenceType Items = new SequenceType(XmlTypeCode.Item, XmlTypeCardinality.ZeroOrMore);
        internal static SequenceType Node = new SequenceType(XmlTypeCode.Node);
        internal static SequenceType ProcessingInstruction = new SequenceType(XmlTypeCode.ProcessingInstruction);
        internal static SequenceType Text = new SequenceType(XmlTypeCode.Text);
        internal static SequenceType Comment = new SequenceType(XmlTypeCode.Comment);
        internal static SequenceType Element = new SequenceType(XmlTypeCode.Element);
        internal static SequenceType Attribute = new SequenceType(XmlTypeCode.Attribute);
        internal static SequenceType Document = new SequenceType(XmlTypeCode.Document);

        internal static SequenceType Boolean = new SequenceType(XmlTypeCode.Boolean);
        internal static SequenceType AnyAtomicType = new SequenceType(XmlTypeCode.AnyAtomicType);

        internal static SequenceType AnyAtomicTypeOptional = new SequenceType(XmlTypeCode.AnyAtomicType,
            XmlTypeCardinality.ZeroOrOne);

        internal static SequenceType Double = new SequenceType(XmlTypeCode.Double);

        internal static SequenceType Date = new SequenceType(XmlTypeCode.Date);
        internal static SequenceType Time = new SequenceType(XmlTypeCode.Time);
        internal static SequenceType DateTime = new SequenceType(XmlTypeCode.DateTime);

        internal static SequenceType StringOptional = new SequenceType(XmlTypeCode.String, XmlTypeCardinality.ZeroOrOne);
        internal static SequenceType String = new SequenceType(XmlTypeCode.String);

        internal static SequenceType Int = new SequenceType(XmlTypeCode.Int);

        #endregion
    }
}