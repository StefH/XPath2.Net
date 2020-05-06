// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using Wmhelp.XPath2.MS;
using Wmhelp.XPath2.Properties;
using Wmhelp.XPath2.Proxy;
using Wmhelp.XPath2.Value;

namespace Wmhelp.XPath2
{
    public static class CoreFuncs
    {
        public static readonly object True = true;
        public static readonly object False = false;

        static CoreFuncs()
        {
            ValueProxy.AddFactory(
                new ValueProxyFactory[] {
                    new ShortFactory(),
                    new IntFactory(),
                    new LongFactory(),
                    new IntegerProxyFactory(),
                    new DecimalProxyFactory(),
                    new FloatFactory(),
                    new DoubleProxyFactory(),
                    new StringProxyFactory(),
                    new SByteProxyFactory(),
                    new ByteProxyFactory(),
                    new UShortFactory(),
                    new UIntFactory(),
                    new ULongFactory(),
                    new BoolFactory(),
                    new DateTimeValue.ProxyFactory(),
                    new DateValue.ProxyFactory(),
                    new TimeValue.ProxyFactory(),
                    new DurationValue.ProxyFactory(),
                    new YearMonthDurationValue.ProxyFactory(),
                    new DayTimeDurationValue.ProxyFactory()
            });
        }

        public static object OperatorEq(object arg1, object arg2)
        {
            if (ReferenceEquals(arg1, arg2))
                return True;
            if (arg1 == null)
                arg1 = False;
            if (arg2 == null)
                arg2 = False;
            bool res;
            if (ValueProxy.Eq(arg1, arg2, out res))
                return res ? True : False;
            object a = arg1;
            object b = arg2;
            if (arg1 is UntypedAtomic || arg1 is AnyUriValue)
                a = arg1.ToString();
            if (arg2 is UntypedAtomic || arg2 is AnyUriValue)
                b = arg2.ToString();
            if (a.GetType() == b.GetType() ||
                (a is DurationValue && b is DurationValue))
            {
                if (a.Equals(b))
                    return True;
            }
            else
                throw new XPath2Exception("", Resources.BinaryOperatorNotDefined, "op:eq",
                    new SequenceType(arg1.GetType(), XmlTypeCardinality.One),
                    new SequenceType(arg2.GetType(), XmlTypeCardinality.One));
            return False;
        }

        public static object OperatorGt(object arg1, object arg2)
        {
            if (ReferenceEquals(arg1, arg2))
                return False;
            if (arg1 == null)
                arg1 = False;
            if (arg2 == null)
                arg2 = False;
            bool res;
            if (ValueProxy.Gt(arg1, arg2, out res))
                return res ? True : False;
            if (arg1 is IComparable && arg2 is IComparable)
            {
                object a = arg1;
                object b = arg2;
                if (arg1 is UntypedAtomic || arg1 is AnyUriValue)
                    a = arg1.ToString();
                if (arg2 is UntypedAtomic || arg2 is AnyUriValue)
                    b = arg2.ToString();
                if (a.GetType() == b.GetType())
                {
                    if (((IComparable)a).CompareTo(b) > 0)
                        return True;
                }
                else
                    throw new XPath2Exception("", Resources.BinaryOperatorNotDefined, "op:gt",
                        new SequenceType(arg1.GetType(), XmlTypeCardinality.One),
                        new SequenceType(arg2.GetType(), XmlTypeCardinality.One));
            }
            else
                throw new XPath2Exception("", Resources.BinaryOperatorNotDefined, "op:gt",
                    new SequenceType(arg1.GetType(), XmlTypeCardinality.One),
                    new SequenceType(arg2.GetType(), XmlTypeCardinality.One));
            return False;
        }

        internal static IEnumerable<XPathItem> RootIterator(XPath2NodeIterator iter)
        {
            foreach (XPathItem item in iter)
            {
                XPathNavigator nav = item as XPathNavigator;
                if (nav != null)
                {
                    XPathNavigator curr = nav.Clone();
                    curr.MoveToRoot();
                    yield return curr;
                }
            }
        }

        internal static IEnumerable<XPathItem> AttributeIterator(XPath2NodeIterator iter)
        {
            foreach (XPathItem item in iter)
            {
                XPathNavigator nav = item as XPathNavigator;
                if (nav != null)
                {
                    XPathNavigator curr = nav.Clone();
                    if (curr.MoveToFirstAttribute())
                        do
                        {
                            yield return curr;
                        } while (curr.MoveToNextAttribute());
                }
            }
        }

        internal static IEnumerable<XPathItem> UnionIterator1(XPath2NodeIterator iter1, XPath2NodeIterator iter2)
        {
            SortedDictionary<XPathItem, XPathItem> set =
                new SortedDictionary<XPathItem, XPathItem>(new XPathComparer());
            foreach (XPathItem item in iter1)
                if (!set.ContainsKey(item))
                    set.Add(item.Clone(), null);
            foreach (XPathItem item in iter2)
                if (!set.ContainsKey(item))
                    set.Add(item.Clone(), null);
            foreach (KeyValuePair<XPathItem, XPathItem> kvp in set)
                yield return kvp.Key;
        }

        internal static IEnumerable<XPathItem> UnionIterator2(XPath2NodeIterator iter1, XPath2NodeIterator iter2)
        {
            HashSet<XPathItem> hs = new HashSet<XPathItem>(new XPathNavigatorEqualityComparer());
            foreach (XPathItem item in iter1)
                if (!hs.Contains(item))
                {
                    hs.Add(item.Clone());
                    yield return item;
                }
            foreach (XPathItem item in iter2)
                if (!hs.Contains(item))
                {
                    hs.Add(item.Clone());
                    yield return item;
                }
        }

        internal static IEnumerable<XPathItem> IntersectExceptIterator1(bool except, XPath2NodeIterator iter1, XPath2NodeIterator iter2)
        {
            SortedDictionary<XPathItem, XPathItem> set =
                new SortedDictionary<XPathItem, XPathItem>(new XPathComparer());
            HashSet<XPathItem> hs = new HashSet<XPathItem>(new XPathNavigatorEqualityComparer());
            foreach (XPathItem item in iter1)
                if (!set.ContainsKey(item))
                    set.Add(item.Clone(), null);
            foreach (XPathItem item in iter2)
                if (!hs.Contains(item))
                    hs.Add(item.Clone());
            foreach (KeyValuePair<XPathItem, XPathItem> kvp in set)
                if (except)
                {
                    if (!hs.Contains(kvp.Key))
                        yield return kvp.Key;
                }
                else
                {
                    if (hs.Contains(kvp.Key))
                        yield return kvp.Key;
                }
        }

        internal static IEnumerable<XPathItem> IntersectExceptIterator2(bool except, XPath2NodeIterator iter1, XPath2NodeIterator iter2)
        {
            HashSet<XPathItem> hs = new HashSet<XPathItem>(new XPathNavigatorEqualityComparer());
            foreach (XPathItem item in iter1)
                if (!hs.Contains(item))
                    hs.Add(item.Clone());
            if (except)
                hs.ExceptWith(iter2);
            else
                hs.IntersectWith(iter2);
            foreach (XPathItem item in hs)
                yield return item;
        }

        internal static IEnumerable<XPathItem> ConvertIterator(XPath2NodeIterator iter, SequenceType destType, XPath2Context context)
        {
            int num = 0;
            SequenceType itemType = new SequenceType(destType);
            itemType.Cardinality = XmlTypeCardinality.One;
            foreach (XPathItem item in iter)
            {
                if (num == 1)
                {
                    if (destType.Cardinality == XmlTypeCardinality.ZeroOrOne ||
                        destType.Cardinality == XmlTypeCardinality.One)
                        throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "item()+", destType);
                }
                yield return item.ChangeType(itemType, context);
                num++;
            }
            if (num == 0)
            {
                if (destType.Cardinality == XmlTypeCardinality.One ||
                    destType.Cardinality == XmlTypeCardinality.OneOrMore)
                    throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "item()?", destType);
            }
        }

        internal static IEnumerable<XPathItem> ValueIterator(XPath2NodeIterator iter, SequenceType destType,
            XPath2Context context)
        {
            int num = 0;
            foreach (XPathItem item in iter)
            {
                if (num == 1)
                {
                    if (destType.Cardinality == XmlTypeCardinality.ZeroOrOne ||
                        destType.Cardinality == XmlTypeCardinality.One)
                        throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "item()+", destType);
                }
                if (destType.IsNode)
                {
                    if (!destType.Match(item, context))
                        throw new XPath2Exception("XPTY0004", Resources.XPTY0004,
                            new SequenceType(item.GetSchemaType(), XmlTypeCardinality.OneOrMore, null), destType);
                    yield return item;
                }
                else
                    yield return new XPath2Item(XPath2Convert.ValueAs(item.GetTypedValue(), destType,
                        context.NameTable, context.NamespaceManager));
                num++;
            }
            if (num == 0)
            {
                if (destType.Cardinality == XmlTypeCardinality.One ||
                    destType.Cardinality == XmlTypeCardinality.OneOrMore)
                    throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "item()?", destType);
            }
        }

        internal static IEnumerable<XPathItem> TreatIterator(XPath2NodeIterator iter, SequenceType destType, XPath2Context context)
        {
            int num = 0;
            foreach (XPathItem item in iter)
            {
                if (num == 1)
                {
                    if (destType.Cardinality == XmlTypeCardinality.ZeroOrOne ||
                        destType.Cardinality == XmlTypeCardinality.One)
                        throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "item()+", destType);
                }
                if (destType.IsNode)
                {
                    if (!destType.Match(item, context))
                        throw new XPath2Exception("XPTY0004", Resources.XPTY0004,
                            new SequenceType(item.GetSchemaType(), XmlTypeCardinality.OneOrMore, null), destType);
                    yield return item;
                }
                else
                    yield return new XPath2Item(XPath2Convert.TreatValueAs(item.GetTypedValue(), destType));
                num++;
            }
            if (num == 0)
            {
                if (destType.Cardinality == XmlTypeCardinality.One ||
                    destType.Cardinality == XmlTypeCardinality.OneOrMore)
                    throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "item()?", destType);
            }
        }

        internal static IEnumerable<XPathItem> ValidateIterator(XPath2NodeIterator iter, XmlSchemaSet schemaSet, bool lax)
        {
            int n = 0;
            foreach (XPathItem item in iter)
            {
                if (!item.IsNode)
                    throw new XPath2Exception("XPTY0004", Resources.XPTY0004,
                        new SequenceType(item.GetTypedValue().GetType(), XmlTypeCardinality.One), "node()*");
                XPathNavigator nav = (XPathNavigator)item.Clone();
                try
                {
                    nav.CheckValidity(schemaSet, null);
                }
                catch (XmlSchemaValidationException ex)
                {
                    throw new XPath2Exception(ex.Message, ex);
                }
                catch (InvalidOperationException ex)
                {
                    throw new XPath2Exception(ex.Message, ex);
                }
                yield return nav;
                n++;
            }
            if (n == 0)
                throw new XPath2Exception("XQTY0030", Resources.XQTY0030);
        }

        internal static IEnumerable<XPathItem> CodepointIterator(string text)
        {
            for (int k = 0; k < text.Length; k++)
                yield return new XPath2Item(Convert.ToInt32(text[k]));
        }


        internal static XPathItem Clone(this XPathItem item)
        {
            XPathNavigator nav = item as XPathNavigator;
            if (nav != null)
                return nav.Clone();
            return item;
        }

        internal static XPathItem ChangeType(this XPathItem item, SequenceType destType, XPath2Context context)
        {
            if (destType.IsNode)
            {
                if (!destType.Match(item, context))
                    throw new XPath2Exception("XPTY0004", Resources.XPTY0004,
                        new SequenceType(item.GetSchemaType().TypeCode), destType);
                return item.Clone();
            }
            else
            {
                if (destType.SchemaType == item.GetSchemaType())
                    return item.Clone();
                else if (destType.TypeCode == XmlTypeCode.Item &&
                    (destType.Cardinality == XmlTypeCardinality.One || destType.Cardinality == XmlTypeCardinality.ZeroOrOne))
                    return item.Clone();
                else
                {
                    XmlSchemaSimpleType simpleType = destType.SchemaType as XmlSchemaSimpleType;
                    if (simpleType == null)
                        throw new InvalidOperationException();
                    if (simpleType == SequenceType.XmlSchema.AnySimpleType)
                        throw new XPath2Exception("XPST0051", Resources.XPST0051, "xs:anySimpleType");
                    return new XPath2Item(XPath2Convert.ChangeType(item.GetSchemaType(), item.GetTypedValue(),
                        destType, context.NameTable, context.NamespaceManager), destType.SchemaType);
                }
            }
        }

        public static string NormalizeStringValue(string value, bool attr, bool raiseException)
        {
            StringBuilder sb = new StringBuilder(value);
            int i = 0;
            while (i < sb.Length)
            {
                switch (sb[i])
                {
                    case '\t':
                        if (attr)
                            sb[i] = ' ';
                        i++;
                        break;

                    case '\n':
                        if (i < sb.Length - 1 && sb[i + 1] == '\r')
                            sb.Remove(i + 1, 1);
                        if (attr)
                            sb[i] = ' ';
                        i++;
                        break;

                    case '\r':
                        if (i < sb.Length - 1 && sb[i + 1] == '\n')
                            sb.Remove(i + 1, 1);
                        if (attr)
                            sb[i] = ' ';
                        else
                            sb[i] = '\n';
                        i++;
                        break;

                    case '&':
                        bool process = false;
                        for (int j = i + 1; j < sb.Length; j++)
                            if (sb[j] == ';')
                            {
                                string entity = sb.ToString(i + 1, j - i - 1);
                                string entity_value = null;
                                if (entity.StartsWith("#"))
                                {
                                    int n;
                                    if (entity.StartsWith("#x"))
                                    {
                                        if (entity.Length > 2 && int.TryParse(entity.Substring(2, entity.Length - 2),
                                                NumberStyles.HexNumber, CultureInfo.InvariantCulture, out n))
                                            entity_value = Convert.ToString(Convert.ToChar(n));
                                    }
                                    else
                                    {
                                        if (entity.Length > 1 && int.TryParse(entity.Substring(1, entity.Length - 1), out n))
                                            entity_value = Convert.ToString(Convert.ToChar(n));
                                    }
                                }
                                else if (entity == "gt")
                                    entity_value = ">";
                                else if (entity == "lt")
                                    entity_value = "<";
                                else if (entity == "amp")
                                    entity_value = "&";
                                else if (entity == "quot")
                                    entity_value = "\"";
                                else if (entity == "apos")
                                    entity_value = "\'";
                                if (entity_value != null)
                                {
                                    sb.Remove(i, j - i + 1);
                                    sb.Insert(i, entity_value);
                                    i += entity_value.Length;
                                    process = true;
                                    break;
                                }
                                else
                                    if (raiseException)
                                    throw new XPath2Exception("XPST0003", Resources.XPST0003, string.Format("Entity reference '&{0};' was not recognized.", entity_value));
                            }
                        if (!process)
                        {
                            if (raiseException)
                                throw new XPath2Exception("XPST0003", Resources.XPST0003, "Entity reference '&' was not terminated by a semi-colon.");
                            i++;
                        }
                        break;

                    default:
                        i++;
                        break;
                }
            }
            return sb.ToString();
        }

        public static object BooleanValue(object value)
        {
            if (value == null ||
                value == Undefined.Value)
                return False;
            if (value == False || value == True)
                return value;
            if (GetBooleanValue(ValueProxy.Unwrap(value)))
                return True;
            return False;
        }

        public static bool GetBooleanValue(object value)
        {
            XPathItem item;
            XPath2NodeIterator iter = value as XPath2NodeIterator;
            if (iter != null)
            {
                if (!iter.MoveNext())
                    return false;
                item = iter.Current.Clone();
                if (item.IsNode)
                    return true;
                if (iter.MoveNext())
                    throw new XPath2Exception("FORG0006", Resources.FORG0006, "fn:boolean()",
                        new SequenceType(XmlTypeCode.AnyAtomicType, XmlTypeCardinality.OneOrMore));
            }
            else
                item = value as XPathItem;
            if (item != null)
                switch (item.GetSchemaType().TypeCode)
                {
                    case XmlTypeCode.Boolean:
                        return item.ValueAsBoolean;

                    case XmlTypeCode.String:
                    case XmlTypeCode.AnyUri:
                    case XmlTypeCode.UntypedAtomic:
                        return item.Value != string.Empty;

                    case XmlTypeCode.Float:
                    case XmlTypeCode.Double:
                        return !double.IsNaN(item.ValueAsDouble) && item.ValueAsDouble != 0.0;

                    case XmlTypeCode.Decimal:
                    case XmlTypeCode.Integer:
                    case XmlTypeCode.NonPositiveInteger:
                    case XmlTypeCode.NegativeInteger:
                    case XmlTypeCode.Long:
                    case XmlTypeCode.Int:
                    case XmlTypeCode.Short:
                    case XmlTypeCode.Byte:
                    case XmlTypeCode.UnsignedInt:
                    case XmlTypeCode.UnsignedShort:
                    case XmlTypeCode.UnsignedByte:
                    case XmlTypeCode.NonNegativeInteger:
                    case XmlTypeCode.UnsignedLong:
                    case XmlTypeCode.PositiveInteger:
                        return (decimal)(item.ValueAs(typeof(decimal))) != 0;

                    default:
                        throw new XPath2Exception("FORG0006", Resources.FORG0006, "fn:boolean()",
                            new SequenceType(item.GetSchemaType().TypeCode, XmlTypeCardinality.One));
                }

            else
            {
                TypeCode typeCode;
                IConvertible conv = value as IConvertible;
                if (conv != null)
                    typeCode = conv.GetTypeCode();
                else
                    typeCode = Type.GetTypeCode(value.GetType());
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        return Convert.ToBoolean(value, CultureInfo.InvariantCulture);

                    case TypeCode.String:
                        return Convert.ToString(value, CultureInfo.InvariantCulture) != string.Empty;

                    case TypeCode.Single:
                    case TypeCode.Double:
                        return Convert.ToDouble(value, CultureInfo.InvariantCulture) != 0.0 &&
                            !double.IsNaN(Convert.ToDouble(value, CultureInfo.InvariantCulture));
                    default:
                        {
                            if (value is AnyUriValue || value is UntypedAtomic)
                                return value.ToString() != string.Empty;
                            if (ValueProxy.IsNumeric(value.GetType()))
                                return Convert.ToDecimal(value) != 0;
                            throw new XPath2Exception("FORG0006", Resources.FORG0006, "fn:boolean()",
                                new SequenceType(value.GetType(), XmlTypeCardinality.One));
                        }
                }
            }
        }

        public static string NormalizeSpace(object item)
        {
            if (item == Undefined.Value)
                return string.Empty;
            string value = (string)item;
            // Copyright (c) 2006 Microsoft Corporation.  All rights reserved.
            // Original source is XsltFunctions.cs (System.Xml.Xsl.Runtime)
            XmlCharType xmlCharType = XmlCharType.Instance;
            StringBuilder sb = null;
            int idx, idxStart = 0, idxSpace = 0;

            for (idx = 0; idx < value.Length; idx++)
            {
                if (xmlCharType.IsWhiteSpace(value[idx]))
                {
                    if (idx == idxStart)
                    {
                        // Previous character was a whitespace character, so discard this character
                        idxStart++;
                    }
                    else if (value[idx] != ' ' || idxSpace == idx)
                    {
                        // Space was previous character or this is a non-space character
                        if (sb == null)
                            sb = new StringBuilder(value.Length);
                        else
                            sb.Append(' ');

                        // Copy non-space characters into string builder
                        if (idxSpace == idx)
                            sb.Append(value, idxStart, idx - idxStart - 1);
                        else
                            sb.Append(value, idxStart, idx - idxStart);

                        idxStart = idx + 1;
                    }
                    else
                    {
                        // Single whitespace character doesn't cause normalization, but mark its position
                        idxSpace = idx + 1;
                    }
                }
            }

            if (sb == null)
            {
                // Check for string that is entirely composed of whitespace
                if (idxStart == idx) return string.Empty;

                // If string does not end with a space, then it must already be normalized
                if (idxStart == 0 && idxSpace != idx) return value;

                sb = new StringBuilder(value.Length);
            }
            else if (idx != idxStart)
            {
                sb.Append(' ');
            }

            // Copy non-space characters into string builder
            if (idxSpace == idx)
                sb.Append(value, idxStart, idx - idxStart - 1);
            else
                sb.Append(value, idxStart, idx - idxStart);

            return sb.ToString();
        }

        public static object Atomize(object value)
        {
            XPathItem item = value as XPathItem;
            if (item != null)
                return item.GetTypedValue();

            XPath2NodeIterator iter = value as XPath2NodeIterator;
            if (iter != null)
            {
                iter = iter.Clone();
                if (!iter.MoveNext())
                    return Undefined.Value;

                object res = iter.Current.GetTypedValue();
                if (iter.MoveNext())
                    throw new XPath2Exception("XPDY0050", Resources.MoreThanOneItem);

                return res;
            }

            return value;
        }

        public static T Atomize<T>(object value)
        {
            object res = Atomize(value);
            if (res == Undefined.Value)
                throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "empty-sequence()", "item()");
            return (T)res;
        }

        public static XPathNavigator NodeValue(object value, bool raise = true)
        {
            if (value == Undefined.Value)
            {
                if (raise)
                    throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "empty-sequence()", "item()");
                return null;
            }

            XPath2NodeIterator iter = value as XPath2NodeIterator;
            if (iter != null)
            {
                iter = iter.Clone();
                if (!iter.MoveNext())
                {
                    if (raise)
                        throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "empty-sequence()", "item()");
                    return null;
                }

                XPathItem res = iter.Current.Clone();
                if (iter.MoveNext())
                    throw new XPath2Exception("XPDY0050", Resources.MoreThanOneItem);

                XPathNavigator nav = res as XPathNavigator;
                if (nav == null)
                    throw new XPath2Exception("XPTY0004", Resources.XPST0004, "node()");

                return nav;
            }
            else
            {
                XPathNavigator nav = value as XPathNavigator;
                if (nav == null)
                    throw new XPath2Exception("XPTY0004", Resources.XPST0004, "node()");

                return nav.Clone();
            }
        }

        public static object Some(object expr)
        {
            XPath2NodeIterator iter = expr as XPath2NodeIterator;
            if (iter != null)
            {
                while (iter.MoveNext())
                    if (iter.Current.ValueAsBoolean)
                        return True;
            }
            return False;
        }

        public static object Every(object expr)
        {
            XPath2NodeIterator iter = expr as XPath2NodeIterator;
            if (iter != null)
            {
                while (iter.MoveNext())
                    if (!iter.Current.ValueAsBoolean)
                        return False;
            }
            return True;
        }

        public static object CastTo(XPath2Context context, object value, SequenceType destType, bool isLiteral)
        {
            if (destType == SequenceType.Item)
                return value;

            if (value == Undefined.Value)
            {
                if (destType.Cardinality == XmlTypeCardinality.ZeroOrMore)
                    return EmptyIterator.Shared;

                if (destType.TypeCode != XmlTypeCode.None && destType.Cardinality != XmlTypeCardinality.ZeroOrOne)
                    throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "empty-sequence()", destType);

                return Undefined.Value;
            }

            if (destType.Cardinality == XmlTypeCardinality.One || destType.Cardinality == XmlTypeCardinality.ZeroOrOne)
            {
                XPathItem res;
                XPath2NodeIterator iter = value as XPath2NodeIterator;
                if (iter != null)
                {
                    iter = iter.Clone();
                    if (!iter.MoveNext())
                    {
                        if (destType.TypeCode != XmlTypeCode.None &&
                            (destType.Cardinality == XmlTypeCardinality.One || destType.Cardinality == XmlTypeCardinality.OneOrMore))
                            throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "empty-sequence()", destType);

                        return Undefined.Value;
                    }

                    if (!isLiteral)
                    {
                        if (destType.TypeCode == XmlTypeCode.QName && iter.Current.GetSchemaType().TypeCode != XmlTypeCode.QName ||
                            destType.TypeCode == XmlTypeCode.Notation && iter.Current.GetSchemaType().TypeCode != XmlTypeCode.Notation)
                            throw new XPath2Exception("XPTY0004", Resources.XPTY0004_CAST, destType);
                    }

                    res = iter.Current.ChangeType(destType, context);
                    if (iter.MoveNext())
                        throw new XPath2Exception("XPDY0050", Resources.MoreThanOneItem);

                    if (destType.IsNode)
                        return res;

                    return res.GetTypedValue();
                }

                XPathItem item = value as XPathItem ?? new XPath2Item(value);

                if (!isLiteral)
                {
                    if (destType.TypeCode == XmlTypeCode.QName && item.XmlType.TypeCode != XmlTypeCode.QName ||
                        destType.TypeCode == XmlTypeCode.Notation && item.XmlType.TypeCode != XmlTypeCode.Notation)
                        throw new XPath2Exception("XPTY0004", Resources.XPTY0004_CAST, destType);
                }

                res = item.ChangeType(destType, context);
                if (destType.IsNode)
                    return res;

                return res.GetTypedValue();
            }

            return new NodeIterator(ConvertIterator(XPath2NodeIterator.Create(value), destType, context));
        }

        public static object CastArg(XPath2Context context, object value, SequenceType destType)
        {
            if (destType == SequenceType.Item)
                return value;

            if (value == Undefined.Value)
            {
                if (destType.Cardinality == XmlTypeCardinality.ZeroOrMore)
                    return EmptyIterator.Shared;
                if (destType.TypeCode != XmlTypeCode.None && destType.Cardinality != XmlTypeCardinality.ZeroOrOne)
                    throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "empty-sequence()", destType);
                return Undefined.Value;
            }

            if (destType.Cardinality == XmlTypeCardinality.One || destType.Cardinality == XmlTypeCardinality.ZeroOrOne)
            {
                XPath2NodeIterator iter = value as XPath2NodeIterator;
                if (iter != null)
                {
                    iter = iter.Clone();
                    if (!iter.MoveNext())
                    {
                        if (destType.TypeCode != XmlTypeCode.None && (destType.Cardinality == XmlTypeCardinality.One || destType.Cardinality == XmlTypeCardinality.OneOrMore))
                            throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "empty-sequence()", destType);

                        return Undefined.Value;
                    }

                    object res;
                    if (destType.IsNode)
                    {
                        if (!destType.Match(iter.Current, context))
                            throw new XPath2Exception("XPTY0004", Resources.XPTY0004, new SequenceType(iter.Current.GetSchemaType(), XmlTypeCardinality.OneOrMore, null), destType);

                        res = iter.Current.Clone();
                    }
                    else
                        res = XPath2Convert.ValueAs(iter.Current.GetTypedValue(), destType, context.NameTable, context.NamespaceManager);

                    if (iter.MoveNext())
                        throw new XPath2Exception("XPDY0050", Resources.MoreThanOneItem);

                    return res;
                }

                XPathItem item = value as XPathItem;
                if (item != null)
                {
                    if (item.IsNode)
                    {
                        if (!destType.Match(item, context))
                            throw new XPath2Exception("XPTY0004", Resources.XPTY0004, new SequenceType(item.GetSchemaType(), XmlTypeCardinality.OneOrMore, null), destType);

                        return item;
                    }

                    return XPath2Convert.ValueAs(item.GetTypedValue(), destType, context.NameTable, context.NamespaceManager);
                }

                return XPath2Convert.ValueAs(value, destType, context.NameTable, context.NamespaceManager);
            }

            return new NodeIterator(ValueIterator(XPath2NodeIterator.Create(value), destType, context));
        }

        public static object TreatAs(XPath2Context context, object value, SequenceType destType)
        {
            if (destType == SequenceType.Item)
                return value;
            if (value == Undefined.Value)
            {
                if (destType.Cardinality == XmlTypeCardinality.ZeroOrMore)
                    return EmptyIterator.Shared;
                if (destType.TypeCode != XmlTypeCode.None && destType.Cardinality != XmlTypeCardinality.ZeroOrOne)
                    throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "empty-sequence()", destType);
                return Undefined.Value;
            }
            if (destType.Cardinality == XmlTypeCardinality.One ||
                destType.Cardinality == XmlTypeCardinality.ZeroOrOne)
            {
                object res;
                XPath2NodeIterator iter = value as XPath2NodeIterator;
                if (iter != null)
                {
                    iter = iter.Clone();
                    if (!iter.MoveNext())
                    {
                        if (destType.TypeCode != XmlTypeCode.None &&
                            (destType.Cardinality == XmlTypeCardinality.One || destType.Cardinality == XmlTypeCardinality.OneOrMore))
                            throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "empty-sequence()", destType);
                        return Undefined.Value;
                    }
                    if (destType.TypeCode == XmlTypeCode.None)
                        throw new XPath2Exception("XPTY0004", Resources.XPTY0004,
                            new SequenceType(iter.Current.GetSchemaType(), XmlTypeCardinality.OneOrMore, null), "empty-sequence()");
                    if (destType.IsNode)
                    {
                        if (!destType.Match(iter.Current, context))
                            throw new XPath2Exception("XPTY0004", Resources.XPTY0004,
                                new SequenceType(iter.Current.GetSchemaType(), XmlTypeCardinality.OneOrMore, null), destType);
                        res = iter.Current.Clone();
                    }
                    else
                        res = XPath2Convert.TreatValueAs(iter.Current.GetTypedValue(), destType);
                    if (iter.MoveNext())
                        throw new XPath2Exception("XPDY0050", Resources.MoreThanOneItem);
                    return res;
                }
                else
                {
                    XPathItem item = value as XPathItem;
                    if (item != null)
                    {
                        if (item.IsNode)
                        {
                            if (!destType.Match(item, context))
                                throw new XPath2Exception("XPTY0004", Resources.XPTY0004,
                                    new SequenceType(item.GetSchemaType(), XmlTypeCardinality.OneOrMore, null), destType);
                            return item;
                        }
                        else
                            return XPath2Convert.TreatValueAs(item.GetTypedValue(), destType);
                    }
                    return XPath2Convert.TreatValueAs(value, destType);
                }
            }
            else
                return new NodeIterator(TreatIterator(
                    XPath2NodeIterator.Create(value), destType, context));
        }

        public static object CastToItem(XPath2Context context,
            object value, SequenceType destType)
        {
            if (value == null)
                value = False;
            else
            {
                value = Atomize(value);
                if (value == Undefined.Value)
                {
                    if (destType.TypeCode == XmlTypeCode.String)
                        return string.Empty;
                    return value;
                }
            }
            XmlTypeCode typeCode = SequenceType.GetXmlTypeCode(ValueProxy.Unwrap(value).GetType());
            XmlSchemaType xmlType = XmlSchemaType.GetBuiltInSimpleType(typeCode);
            return XPath2Convert.ChangeType(xmlType, value,
                destType, context.NameTable, context.NamespaceManager);
        }

        public static object InstanceOf(XPath2Context context, object value, SequenceType destType)
        {
            if (value == Undefined.Value)
                return destType == SequenceType.Void ||
                    destType.Cardinality == XmlTypeCardinality.ZeroOrOne ||
                    destType.Cardinality == XmlTypeCardinality.ZeroOrMore;
            if (value == null)
                value = False;
            XPath2NodeIterator iter = value as XPath2NodeIterator;
            if (iter != null)
            {
                int num = 0;
                foreach (XPathItem item in iter)
                {
                    if (num == 1)
                    {
                        if (destType.Cardinality == XmlTypeCardinality.ZeroOrOne ||
                            destType.Cardinality == XmlTypeCardinality.One)
                            return False;
                    }
                    if (!destType.Match(item, context))
                        return False;
                    num++;
                }
                if (num == 0)
                {
                    if (destType.TypeCode != XmlTypeCode.None && (destType.Cardinality == XmlTypeCardinality.One ||
                         destType.Cardinality == XmlTypeCardinality.OneOrMore))
                        return False;
                }
                return True;
            }
            else
            {
                if (destType.ItemType == value.GetType())
                    return True;
                XPathItem item = value as XPathItem;
                if (item == null)
                    item = new XPath2Item(value);
                if (destType.Match(item, context))
                    return True;
                return False;
            }
        }

        public static object Castable(XPath2Context context, object value, SequenceType destType, bool isLiteral)
        {
            try
            {
                CastTo(context, value, destType, isLiteral);
                return True;
            }
            catch (XPath2Exception)
            {
                return False;
            }
        }

        public static object SameNode(object a, object b)
        {
            XPathNavigator nav1 = (XPathNavigator)a;
            XPathNavigator nav2 = (XPathNavigator)b;
            XmlNodeOrder res = nav1.ComparePosition(nav2);
            if (res != XmlNodeOrder.Unknown)
                return res == XmlNodeOrder.Same ? True : False;
            return nav2.ComparePosition(nav1) == XmlNodeOrder.Same ? True : False;
        }

        public static object PrecedingNode(object a, object b)
        {
            XPathNavigator nav1 = (XPathNavigator)a;
            XPathNavigator nav2 = (XPathNavigator)b;
            XPathComparer comp = new XPathComparer();
            return comp.Compare(nav1, nav2) == -1 ? True : False;
        }

        public static object FollowingNode(object a, object b)
        {
            XPathNavigator nav1 = (XPathNavigator)a;
            XPathNavigator nav2 = (XPathNavigator)b;
            XPathComparer comp = new XPathComparer();
            return comp.Compare(nav1, nav2) == 1 ? True : False;
        }

        private static void MagnitudeRelationship(XPath2Context context, XPathItem item1, XPathItem item2,
            out object x, out object y)
        {
            x = item1.GetTypedValue();
            y = item2.GetTypedValue();
            if (x is UntypedAtomic)
            {
                if (ValueProxy.IsNumeric(y.GetType()))
                    x = Convert.ToDouble(x, CultureInfo.InvariantCulture);
                else
                    if (y is string)
                    x = x.ToString();
                else if (!(y is UntypedAtomic))
                    x = item1.ChangeType(new SequenceType(item2.GetSchemaType().TypeCode), context).GetTypedValue();
            }
            if (y is UntypedAtomic)
            {
                if (ValueProxy.IsNumeric(x.GetType()))
                    y = Convert.ToDouble(y, CultureInfo.InvariantCulture);
                else
                    if (x is string)
                    y = y.ToString();
                else if (!(x is UntypedAtomic))
                    y = item2.ChangeType(new SequenceType(item1.GetSchemaType().TypeCode), context).GetTypedValue();
            }
        }

        public static object GeneralEQ(XPath2Context context, object a, object b)
        {
            XPath2NodeIterator iter1 = XPath2NodeIterator.Create(a);
            XPath2NodeIterator iter2 = XPath2NodeIterator.Create(b);
            while (iter1.MoveNext())
            {
                XPath2NodeIterator iter = iter2.Clone();
                while (iter.MoveNext())
                {
                    object x;
                    object y;
                    MagnitudeRelationship(context, iter1.Current, iter.Current, out x, out y);
                    if (OperatorEq(x, y) == True)
                        return True;
                }
            }
            return False;
        }

        public static object GeneralGT(XPath2Context context, object a, object b)
        {
            XPath2NodeIterator iter1 = XPath2NodeIterator.Create(a);
            XPath2NodeIterator iter2 = XPath2NodeIterator.Create(b);
            while (iter1.MoveNext())
            {
                XPath2NodeIterator iter = iter2.Clone();
                while (iter.MoveNext())
                {
                    object x;
                    object y;
                    MagnitudeRelationship(context, iter1.Current, iter.Current, out x, out y);
                    if (OperatorGt(x, y) == True)
                        return True;
                }
            }
            return False;
        }

        public static object GeneralNE(XPath2Context context, object a, object b)
        {
            XPath2NodeIterator iter1 = XPath2NodeIterator.Create(a);
            XPath2NodeIterator iter2 = XPath2NodeIterator.Create(b);
            while (iter1.MoveNext())
            {
                XPath2NodeIterator iter = iter2.Clone();
                while (iter.MoveNext())
                {
                    object x;
                    object y;
                    MagnitudeRelationship(context, iter1.Current, iter.Current, out x, out y);
                    if (OperatorEq(x, y) == False)
                        return True;
                }
            }
            return False;
        }

        public static object GeneralGE(XPath2Context context, object a, object b)
        {
            XPath2NodeIterator iter1 = XPath2NodeIterator.Create(a);
            XPath2NodeIterator iter2 = XPath2NodeIterator.Create(b);
            while (iter1.MoveNext())
            {
                XPath2NodeIterator iter = iter2.Clone();
                while (iter.MoveNext())
                {
                    object x;
                    object y;
                    MagnitudeRelationship(context, iter1.Current, iter.Current, out x, out y);
                    if (OperatorEq(x, y) == True || OperatorGt(x, y) == True)
                        return True;
                }
            }
            return False;
        }

        public static object GeneralLT(XPath2Context context, object a, object b)
        {
            XPath2NodeIterator iter1 = XPath2NodeIterator.Create(a);
            XPath2NodeIterator iter2 = XPath2NodeIterator.Create(b);
            while (iter1.MoveNext())
            {
                XPath2NodeIterator iter = iter2.Clone();
                while (iter.MoveNext())
                {
                    object x;
                    object y;
                    MagnitudeRelationship(context, iter1.Current, iter.Current, out x, out y);
                    if (OperatorGt(y, x) == True)
                        return True;
                }
            }
            return False;
        }

        public static object GeneralLE(XPath2Context context, object a, object b)
        {
            XPath2NodeIterator iter1 = XPath2NodeIterator.Create(a);
            XPath2NodeIterator iter2 = XPath2NodeIterator.Create(b);
            while (iter1.MoveNext())
            {
                XPath2NodeIterator iter = iter2.Clone();
                while (iter.MoveNext())
                {
                    object x;
                    object y;
                    MagnitudeRelationship(context, iter1.Current, iter.Current, out x, out y);
                    if (OperatorEq(x, y) == True || OperatorGt(y, x) == True)
                        return True;
                }
            }
            return False;
        }

        public static XPath2NodeIterator GetRange(object arg1, object arg2)
        {
            object lo = Atomize(arg1);
            if (lo == Undefined.Value)
                return EmptyIterator.Shared;
            if (lo is UntypedAtomic)
            {
                int i;
                if (!int.TryParse(lo.ToString(), out i))
                    throw new XPath2Exception("XPTY0004", Resources.XPTY0004,
                        new SequenceType(lo.GetType(), XmlTypeCardinality.One), "xs:integer in first argument op:range");
                lo = i;
            }
            object high = Atomize(arg2);
            if (high == Undefined.Value)
                return EmptyIterator.Shared;
            if (high is UntypedAtomic)
            {
                int i;
                if (!int.TryParse(high.ToString(), out i))
                    throw new XPath2Exception("XPTY0004", Resources.XPTY0004,
                        new SequenceType(lo.GetType(), XmlTypeCardinality.One), "xs:integer in second argument op:range");
                high = i;
            }
            ValueProxy prx1 = lo as ValueProxy;
            if (prx1 != null)
                lo = prx1.Value;
            ValueProxy prx2 = high as ValueProxy;
            if (prx2 != null)
                high = prx2.Value;
            if (!Integer.IsDerivedSubtype(lo))
                throw new XPath2Exception("XPTY0004", Resources.XPTY0004,
                    new SequenceType(lo.GetType(), XmlTypeCardinality.One), "xs:integer in first argument op:range");
            if (!Integer.IsDerivedSubtype(high))
                throw new XPath2Exception("XPTY0004", Resources.XPTY0004,
                    new SequenceType(high.GetType(), XmlTypeCardinality.One), "xs:integer in second argument op:range");
            return new RangeIterator(Convert.ToInt32(lo), Convert.ToInt32(high));
        }

        public static XPath2NodeIterator Union(XPath2Context context, object a, object b)
        {
            XPath2NodeIterator iter1 = XPath2NodeIterator.Create(a);
            XPath2NodeIterator iter2 = XPath2NodeIterator.Create(b);
            if (context.RunningContext.IsOrdered)
                return new NodeIterator(UnionIterator1(iter1, iter2));
            else
                return new NodeIterator(UnionIterator2(iter1, iter2));
        }

        public static XPath2NodeIterator Except(XPath2Context context, object a, object b)
        {
            XPath2NodeIterator iter1 = XPath2NodeIterator.Create(a);
            XPath2NodeIterator iter2 = XPath2NodeIterator.Create(b);
            if (context.RunningContext.IsOrdered)
                return new NodeIterator(IntersectExceptIterator1(true, iter1, iter2));
            else
                return new NodeIterator(IntersectExceptIterator2(true, iter1, iter2));
        }

        public static XPath2NodeIterator Intersect(XPath2Context context, object a, object b)
        {
            XPath2NodeIterator iter1 = XPath2NodeIterator.Create(a);
            XPath2NodeIterator iter2 = XPath2NodeIterator.Create(b);
            if (context.RunningContext.IsOrdered)
                return new NodeIterator(IntersectExceptIterator1(false, iter1, iter2));
            else
                return new NodeIterator(IntersectExceptIterator2(false, iter1, iter2));
        }

        public static XPathItem ContextNode(IContextProvider provider)
        {
            if (provider == null)
                throw new XPath2Exception("XPDY0002", Resources.XPDY0002);
            XPathItem item = provider.Context;
            if (item == null)
                throw new XPath2Exception("XPDY0002", Resources.XPDY0002);
            return item;
        }

        public static XPath2ResultType GetXPath2ResultType(SequenceType sequenceType)
        {
            if (sequenceType.Cardinality == XmlTypeCardinality.ZeroOrMore ||
                sequenceType.Cardinality == XmlTypeCardinality.OneOrMore)
                return XPath2ResultType.NodeSet;
            switch (sequenceType.TypeCode)
            {
                case XmlTypeCode.String:
                    return XPath2ResultType.String;
                case XmlTypeCode.Time:
                case XmlTypeCode.Date:
                case XmlTypeCode.DateTime:
                    return XPath2ResultType.DateTime;
                case XmlTypeCode.Boolean:
                    return XPath2ResultType.Boolean;
                case XmlTypeCode.AnyUri:
                    return XPath2ResultType.AnyUri;
                case XmlTypeCode.QName:
                    return XPath2ResultType.QName;
                case XmlTypeCode.GDay:
                case XmlTypeCode.GMonth:
                case XmlTypeCode.GMonthDay:
                case XmlTypeCode.GYear:
                case XmlTypeCode.GYearMonth:
                    return XPath2ResultType.DateTime;
                case XmlTypeCode.Duration:
                case XmlTypeCode.DayTimeDuration:
                case XmlTypeCode.YearMonthDuration:
                    return XPath2ResultType.Duration;
                default:
                    if (SequenceType.TypeCodeIsNodeType(sequenceType.TypeCode))
                        return XPath2ResultType.Navigator;
                    if (sequenceType.IsNumeric)
                        return XPath2ResultType.Number;
                    return XPath2ResultType.Other;
            }
        }

        public static XPath2ResultType GetXPath2ResultType(object value)
        {
            if (value == null || value == Undefined.Value)
                return XPath2ResultType.Any;
            if (value is XPath2NodeIterator)
                return XPath2ResultType.NodeSet;
            if (value is XPathItem)
            {
                XPathItem item = (XPathItem)value;
                if (item.IsNode)
                    return XPath2ResultType.Navigator;
                else
                    value = item.TypedValue;
            }
            ValueProxy proxy = value as ValueProxy;
            if (proxy != null)
                value = proxy.Value;
            if (value is AnyUriValue)
                return XPath2ResultType.AnyUri;
            if (value is QNameValue)
                return XPath2ResultType.QName;
            if (value is Integer)
                return XPath2ResultType.Number;
            if (value is DateTimeValueBase || value is TimeValue)
                return XPath2ResultType.DateTime;
            if (value is DurationValue)
                return XPath2ResultType.Duration;
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Boolean:
                    return XPath2ResultType.Boolean;

                case TypeCode.Char:
                case TypeCode.String:
                    return XPath2ResultType.String;

                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.Int16:
                case TypeCode.UInt32:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Decimal:
                case TypeCode.Single:
                case TypeCode.Double:
                    return XPath2ResultType.Number;

                default:
                    return XPath2ResultType.Other;
            }
        }

        public static object GetRoot(IContextProvider provider)
        {
            return GetRoot(NodeValue(ContextNode(provider)));
        }

        public static object GetRoot(object node)
        {
            if (node == null)
                return Undefined.Value;
            XPathNavigator nav = node as XPathNavigator;
            if (nav == null)
                throw new XPath2Exception("XPTY0004", Resources.XPTY0004,
                    new SequenceType(node.GetType(), XmlTypeCardinality.ZeroOrOne), "node()? in fn:root()");
            XPathNavigator curr = nav.Clone();
            curr.MoveToRoot();
            return curr;
        }

        public static object Not(object value)
        {
            if (BooleanValue(value) == False)
                return True;
            return False;
        }

        public static string CastToStringOptional(XPath2Context context, object value)
        {
            var result = CastArg(context, Atomize(value), SequenceType.StringOptional);

            return result != Undefined.Value ? (string)result : string.Empty;
        }

        public static string CastToStringExactOne(XPath2Context context, object value, bool atomize = true)
        {
            var result = CastArg(context, atomize ? Atomize(value) : value, SequenceType.String);

            return result != Undefined.Value ? (string)result : string.Empty;
        }

        public static int CastToInt(XPath2Context context, object value)
        {
            return (int)CastArg(context, value, SequenceType.Int);
        }

        public static double Number(XPath2Context context, IContextProvider provider)
        {
            return Number(context, Atomize(ContextNode(provider)));
        }

        public static double Number(XPath2Context context, object value)
        {
            if (value == Undefined.Value)
            {
                return double.NaN;
            }

            if (value is IXmlConvertable xmlConvertableValue)
            {
                try
                {
                    return (double) xmlConvertableValue.ValueAs(SequenceType.Double, context.NamespaceManager);
                } 
                catch (InvalidCastException)
                {
                    return double.NaN;
                }
            }
            if (!(value is IConvertible))
            {
                var stringValue = StringValue(context, value);
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return double.NaN;
                }
                value = stringValue.Trim();
            }

            try
            {
                return (double)Convert.ChangeType(value, TypeCode.Double, context.RunningContext.DefaultCulture);
            }
            catch (FormatException)
            {
                return double.NaN;
            }
            catch (InvalidCastException)
            {
                return double.NaN;
            }
        }
        public static object CastToNumber1(XPath2Context context, object value)
        {
            try
            {
                if (value is UntypedAtomic)
                    return Convert.ToDouble(value, context.RunningContext.DefaultCulture);
            }
            catch (FormatException)
            {
                throw new XPath2Exception("FORG0001", Resources.FORG0001, value, "xs:double?");
            }
            catch (InvalidCastException)
            {
                throw new XPath2Exception("FORG0001", Resources.FORG0001, value, "xs:double?");
            }
            return value;
        }

        public static double CastToNumber2(XPath2Context context, object value)
        {
            try
            {
                if (!(value is UntypedAtomic))
                    throw new XPath2Exception("XPTY0004", Resources.XPTY0004,
                        new SequenceType(value.GetType(), XmlTypeCardinality.One), "xs:untypedAtomic?");
                return Convert.ToDouble(value, context.RunningContext.DefaultCulture);
            }
            catch (FormatException)
            {
                throw new XPath2Exception("FORG0001", Resources.FORG0001, value, "xs:double?");
            }
            catch (InvalidCastException)
            {
                throw new XPath2Exception("FORG0001", Resources.FORG0001, value, "xs:double?");
            }
        }

        public static double CastToNumber3(XPath2Context context, object value)
        {
            try
            {
                return Convert.ToDouble(value, context.RunningContext.DefaultCulture);
            }
            catch (FormatException)
            {
                throw new XPath2Exception("FORG0001", Resources.FORG0001, value, "xs:double?");
            }
            catch (InvalidCastException)
            {
                throw new XPath2Exception("FORG0001", Resources.FORG0001, value, "xs:double?");
            }
        }

        public static string StringValue(XPath2Context context, IContextProvider provider)
        {
            return StringValue(context, ContextNode(provider));
        }

        public static string StringValue(XPath2Context context, object value)
        {
            if (value == Undefined.Value)
                return string.Empty;

            XPath2NodeIterator iter = value as XPath2NodeIterator;
            if (iter != null)
            {
                iter = iter.Clone();
                if (!iter.MoveNext())
                    return string.Empty;

                string res = iter.Current.Value;
                if (iter.MoveNext())
                    throw new XPath2Exception("XPDY0050", Resources.MoreThanOneItem);

                return res;
            }

            XPathItem item = value as XPathItem;
            if (item != null)
                return item.Value;

            return XPath2Convert.ToString(value);
        }

        public static bool TryProcessTypeName(XPath2Context context, string qname, bool raise, out XmlSchemaObject schemaObject)
        {
            XmlQualifiedName qualifiedName =
                (XmlQualifiedName)QNameParser.Parse(qname, context.NamespaceManager,
                    context.NamespaceManager.DefaultNamespace, context.NameTable);
            return TryProcessTypeName(context, qualifiedName, raise, out schemaObject);
        }

        public static bool TryProcessTypeName(XPath2Context context, XmlQualifiedName qualifiedName,
            bool raise, out XmlSchemaObject schemaObject)
        {
            schemaObject = null;
            if (qualifiedName.Name == "anyAtomicType" && qualifiedName.Namespace == XmlReservedNs.NsXs)
            {
                schemaObject = SequenceType.XmlSchema.AnyAtomicType;
                return true;
            }
            if (qualifiedName.Name == "untypedAtomic" && qualifiedName.Namespace == XmlReservedNs.NsXs)
            {
                schemaObject = SequenceType.XmlSchema.UntypedAtomic;
                return true;
            }
            if (qualifiedName.Name == "anyType" && qualifiedName.Namespace == XmlReservedNs.NsXs)
            {
                schemaObject = SequenceType.XmlSchema.AnyType;
                return true;
            }
            if (qualifiedName.Name == "untyped" && qualifiedName.Namespace == XmlReservedNs.NsXs)
                return true;
            if (qualifiedName.Name == "yearMonthDuration" && qualifiedName.Namespace == XmlReservedNs.NsXs)
            {
                schemaObject = SequenceType.XmlSchema.YearMonthDuration;
                return true;
            }
            if (qualifiedName.Name == "dayTimeDuration" && qualifiedName.Namespace == XmlReservedNs.NsXs)
            {
                schemaObject = SequenceType.XmlSchema.DayTimeDuration;
                return true;
            }
            if (qualifiedName.Namespace == XmlReservedNs.NsXs)
                schemaObject = XmlSchemaType.GetBuiltInSimpleType(qualifiedName);
            else
                schemaObject = context.SchemaSet.GlobalTypes[qualifiedName];
            if (schemaObject == null && raise)
                throw new XPath2Exception("XPST0008", Resources.XPST0008, qualifiedName);
            return schemaObject != null;
        }
    }
}
