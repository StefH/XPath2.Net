// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using Wmhelp.XPath2.MS;
using Wmhelp.XPath2.Properties;
using Wmhelp.XPath2.Proxy;
using Wmhelp.XPath2.Value;

namespace Wmhelp.XPath2
{
    public static class ExtFuncs
    {
        public static string GetName(IContextProvider provider)
        {
            return GetName(CoreFuncs.NodeValue(CoreFuncs.ContextNode(provider)));
        }

        public static string GetName(XPathNavigator nav)
        {
            if (nav == null)
                return String.Empty;
            return nav.Name;
        }

        public static object GetNodeName(XPath2Context context, XPathNavigator nav)
        {
            if (nav != null)
            {
                if (nav.NodeType == XPathNodeType.Element || nav.NodeType == XPathNodeType.Attribute)
                    return new QNameValue(nav.Prefix, nav.LocalName, nav.NamespaceURI, nav.NameTable);
                else if (nav.NodeType == XPathNodeType.ProcessingInstruction || nav.NodeType == XPathNodeType.Namespace)
                    return new QNameValue("", nav.Name, "", nav.NameTable);
            }
            return Undefined.Value;
        }

        public static string GetLocalName(IContextProvider provider)
        {
            return GetLocalName(CoreFuncs.NodeValue(CoreFuncs.ContextNode(provider)));
        }

        public static string GetLocalName(XPathNavigator nav)
        {
            if (nav == null)
                return String.Empty;
            return nav.LocalName;
        }

        public static object GetNamespaceUri(IContextProvider provider)
        {
            return GetNamespaceUri(CoreFuncs.NodeValue(CoreFuncs.ContextNode(provider)));
        }

        public static object GetNamespaceUri(XPathNavigator nav)
        {
            if (nav == null)
                return new AnyUriValue(String.Empty);
            return new AnyUriValue(nav.NamespaceURI);
        }

        public static object GetNilled(XPathNavigator nav)
        {
            if (nav == null)
                return Undefined.Value;
            if (nav.NodeType != XPathNodeType.Element)
                return Undefined.Value;
            if (nav.SchemaInfo != null)
                return nav.SchemaInfo.IsNil;
            return false;
        }

        public static object GetBaseUri(XPath2Context context, XPathNavigator nav)
        {
            if (nav == null)
                return Undefined.Value;

            if (!(nav.NodeType == XPathNodeType.Element ||
                  nav.NodeType == XPathNodeType.Attribute ||
                  nav.NodeType == XPathNodeType.Root ||
                  nav.NodeType == XPathNodeType.Namespace))
                return Undefined.Value;

            nav = nav.Clone();
            List<string> uri = new List<string>();
            do
            {
                string baseUri = nav.BaseURI;
                if (baseUri != "")
                    uri.Add(baseUri);
            }
            while (nav.MoveToParent());
            Uri res = null;
            if (context.RunningContext.BaseUri != null)
                res = new Uri(context.RunningContext.BaseUri);
            for (int k = uri.Count - 1; k >= 0; k--)
            {
                if (res != null)
                    res = new Uri(res, uri[k]);
                else
                    res = new Uri(uri[k]);
            }
            if (res == null)
                return Undefined.Value;
            else
                return new AnyUriValue(res);
        }

        public static object GetBaseUri(XPath2Context context, IContextProvider provider)
        {
            return GetBaseUri(context, CoreFuncs.NodeValue(CoreFuncs.ContextNode(provider)));
        }

        public static object DocumentUri(XPathNavigator nav)
        {
            if (nav == null)
                return Undefined.Value;
            if (nav.NodeType != XPathNodeType.Root || nav.BaseURI == "")
                return Undefined.Value;
            return new AnyUriValue(nav.BaseURI);
        }

        public static XPath2NodeIterator WriteTrace(XPath2Context context, XPath2NodeIterator iter, string label)
        {
            StringBuilder sb = new StringBuilder();
            if (label != "")
                sb.AppendFormat("{0}: ", label);
            bool first = true;
            foreach (XPathItem item in iter)
            {
                if (!first)
                    sb.Append(", ");
                else
                    first = false;
                if (item.IsNode)
                    sb.Append(((XPathNavigator)item).OuterXml);
                else
                    sb.Append(item.Value);
            }
            Trace.WriteLine(sb.ToString());
            return iter;
        }

        public static XPath2NodeIterator WriteTrace(XPath2Context context, XPath2NodeIterator iter)
        {
            return WriteTrace(context, iter, "");
        }

        private static IEnumerable<XPathItem> AtomizeIterator(XPath2NodeIterator iter)
        {
            foreach (XPathItem item in iter)
            {
                if (item.IsNode && item.TypedValue == null)
                {
                    XPathNavigator nav = (XPathNavigator)item;
                    if (nav.SchemaInfo != null &&
                        nav.SchemaInfo.SchemaType != null && !(nav.SchemaInfo.SchemaType is XmlSchemaSimpleType))
                        throw new XPath2Exception("FOTY0012", Resources.FOTY0012, new XmlQualifiedName(nav.LocalName, nav.NamespaceURI));
                }
                yield return new XPath2Item(item.GetTypedValue());
            }
        }

        public static XPath2NodeIterator GetData(XPath2NodeIterator iter)
        {
            return new NodeIterator(AtomizeIterator(iter));
        }

        public static string Concat(XPath2Context context, object[] args)
        {
            StringBuilder sb = new StringBuilder();
            if (args.Length < 2)
                throw new XPath2Exception("XPST0017", Resources.XPST0017, "concat",
                    args.Length, XmlReservedNs.NsXQueryFunc);
            foreach (object arg in args)
                if (arg != Undefined.Value)
                    sb.Append(CoreFuncs.StringValue(context, arg));
            return sb.ToString();
        }

        public static object StringJoin(XPath2NodeIterator iter, object s)
        {
            if (s == Undefined.Value)
                return Undefined.Value;
            string str = (string)s;
            StringBuilder sb = new StringBuilder();
            foreach (XPathItem item in iter)
            {
                if (sb.Length > 0 && str != "")
                    sb.Append(str);
                sb.Append(item.Value);
            }
            return sb.ToString();
        }

        public static string Substring(object item, double startingLoc)
        {
            if (item == Undefined.Value)
                return String.Empty;
            string value = (string)item;
            int pos = Convert.ToInt32(Math.Round(startingLoc)) - 1;
            if (pos <= 0)
                pos = 0;
            if (pos < value.Length)
                return value.Substring(pos);
            else
                return String.Empty;
        }

        public static string Substring(object item, double startingLoc, double length)
        {
            if (item == Undefined.Value)
                return String.Empty;
            string value = (string)item;
            if (Double.IsInfinity(startingLoc) || Double.IsNaN(startingLoc) ||
                Double.IsNegativeInfinity(length) || Double.IsNaN(length))
                return String.Empty;
            int pos = Convert.ToInt32(Math.Round(startingLoc)) - 1;
            int len;
            if (Double.IsPositiveInfinity(length))
                len = Int32.MaxValue;
            else
                len = Convert.ToInt32(Math.Round(length));
            if (pos < 0)
            {
                len = len + pos;
                pos = 0;
            }
            if (pos < value.Length)
            {
                if (pos + len > value.Length)
                    len = value.Length - pos;
                if (len > 0)
                    return value.Substring(pos, len);
            }
            return String.Empty;
        }

        public static int StringLength(object source)
        {
            if (source == Undefined.Value)
                return 0;
            return ((string)source).Length;
        }

        public static int StringLength(XPath2Context context, IContextProvider provider)
        {
            return StringLength(CoreFuncs.StringValue(context, CoreFuncs.Atomize(CoreFuncs.ContextNode(provider))));
        }

        public static string NormalizeSpace(XPath2Context context, IContextProvider provider)
        {
            return CoreFuncs.NormalizeSpace(CoreFuncs.StringValue(context, CoreFuncs.Atomize(CoreFuncs.ContextNode(provider))));
        }

        public static string NormalizeUnicode(object arg, string form)
        {
            if (arg == Undefined.Value)
                return String.Empty;
            string value = (String)arg;
            form = form.Trim();
            if (String.Equals(form, "NFC", StringComparison.OrdinalIgnoreCase))
                return value.Normalize(NormalizationForm.FormC);
            if (String.Equals(form, "NFD", StringComparison.OrdinalIgnoreCase))
                return value.Normalize(NormalizationForm.FormD);
            if (String.Equals(form, "NFKC", StringComparison.OrdinalIgnoreCase))
                return value.Normalize(NormalizationForm.FormKC);
            if (String.Equals(form, "NFKD", StringComparison.OrdinalIgnoreCase))
                return value.Normalize(NormalizationForm.FormKD);
            if (form.Length != 0)
                throw new XPath2Exception("", Resources.UnsupportedNormalizationForm, form);
            return value;
        }

        public static string NormalizeUnicode(object arg)
        {
            if (arg == Undefined.Value)
                return String.Empty;
            string value = (string)arg;
            return value.Normalize(NormalizationForm.FormC);
        }

        public static string UpperCase(object value)
        {
            if (value == Undefined.Value)
                return String.Empty;
            return ((string)value).ToUpper();
        }

        public static string LowerCase(object value)
        {
            if (value == Undefined.Value)
                return String.Empty;
            return ((string)value).ToLower();
        }
        private static Dictionary<int, int> CreateMapping(string mapString, string translateString)
        {
            Dictionary<int, int> dictionary = new Dictionary<int, int>();
            int index = 0;
            int num2 = 0;
            while (index < mapString.Length && num2 < translateString.Length)
            {
                int num3;
                if (Char.IsSurrogate(mapString[index]))
                {
                    num3 = Char.ConvertToUtf32(mapString, index);
                    index++;
                }
                else
                    num3 = mapString[index];
                if (!dictionary.ContainsKey(num3))
                {
                    int num4;
                    if (Char.IsSurrogate(translateString[num2]))
                    {
                        num4 = char.ConvertToUtf32(translateString, num2);
                        num2++;
                    }
                    num4 = translateString[num2];
                    dictionary[num3] = num4;
                }
                num2++;
                index++;
            }
            while (index < mapString.Length)
            {
                int num5;
                if (Char.IsSurrogate(mapString[index]))
                {
                    num5 = char.ConvertToUtf32(mapString, index);
                    index++;
                }
                else
                    num5 = mapString[index];
                if (!dictionary.ContainsKey(num5))
                    dictionary[num5] = 0;
                index++;
            }
            return dictionary;
        }

        public static string Translate(object item, string mapString, string transString)
        {
            if (item == Undefined.Value)
                return String.Empty;
            string value = (string)item;
            StringBuilder builder = new StringBuilder(value.Length);
            Dictionary<int, int> mapping = CreateMapping(mapString, transString);
            for (int i = 0; i < value.Length; i++)
            {
                int num3;
                int key = Char.ConvertToUtf32(value, i);
                if (Char.IsSurrogate(value, i))
                    i++;
                if (!mapping.TryGetValue(key, out num3))
                    num3 = key;
                if (num3 != 0)
                {
                    if (num3 < 0x10000)
                        builder.Append((char)num3);
                    else
                    {
                        num3 -= 0x10000;
                        builder.Append((char)((num3 >> 10) + 0xd800));
                        builder.Append((char)((num3 % 0x400) + 0xdc00));
                    }
                }
            }
            return builder.ToString();
        }

        public static string EncodeForUri(object value)
        {
            if (value == Undefined.Value)
                return String.Empty;
            char[] chArray = new char[3];
            chArray[0] = '%';
            StringBuilder sb = new StringBuilder();
            foreach (byte c in Encoding.UTF8.GetBytes((string)value))
            {
                if (Char.IsDigit((char)c) || ('a' <= c && c <= 'z') ||
                     ('A' <= c && c <= 'Z') || c == '-' || c == '_' || c == '.' || c == '~')
                    sb.Append((char)c);
                else
                {
                    int num = c;
                    int num3 = num / 0x10;
                    int num4 = num % 0x10;
                    chArray[1] = (num3 >= 10) ? ((char)(0x41 + (num3 - 10))) : ((char)(0x30 + num3));
                    chArray[2] = (num4 >= 10) ? ((char)(0x41 + (num4 - 10))) : ((char)(0x30 + num4));
                    sb.Append(chArray);
                }
            }
            return sb.ToString();
        }

        public static string IriToUri(object item)
        {
            if (item == Undefined.Value)
                return String.Empty;
            string value = (string)item;
            char[] chArray = new char[3];
            chArray[0] = '%';
            StringBuilder sb = new StringBuilder();
            foreach (byte num in Encoding.UTF8.GetBytes(value))
            {
                if (num == 0x20)
                    sb.Append("%20");
                else if ((((num < 0x7f && num >= 0x20) && (num != 60 && num != 0x3e)) &&
                    ((num != 0x22 && num != 0x7b) && (num != 0x7d && num != 0x7c))) && (((num != 0x5c) && (num != 0x5e)) && (num != 0x60)))
                    sb.Append((char)num);
                else
                {
                    int num2 = num;
                    int num3 = num2 / 0x10;
                    int num4 = num2 % 0x10;
                    chArray[1] = (num3 >= 10) ? ((char)(0x41 + (num3 - 10))) : ((char)(0x30 + num3));
                    chArray[2] = (num4 >= 10) ? ((char)(0x41 + (num4 - 10))) : ((char)(0x30 + num4));
                    sb.Append(chArray);
                }
            }
            return sb.ToString();
        }

        public static string EscapeHtmlUri(object item)
        {
            if (item == Undefined.Value)
                return String.Empty;
            string value = (string)item;
            StringBuilder builder = new StringBuilder(value.Length);
            foreach (byte num in Encoding.UTF8.GetBytes(value))
            {
                if (num >= 0x20 && num < 0x7f)
                    builder.Append((char)num);
                else
                {
                    int num2 = num / 0x10;
                    int num3 = num % 0x10;
                    char ch = (num2 >= 10) ? ((char)(0x41 + (num2 - 10))) : ((char)(0x30 + num2));
                    char ch2 = (num3 >= 10) ? ((char)(0x41 + (num3 - 10))) : ((char)(0x30 + num3));
                    builder.Append('%');
                    builder.Append(ch);
                    builder.Append(ch2);
                }
            }
            return builder.ToString();
        }

        public static bool Contains(string arg1, string arg2)
        {
            return arg1.Contains(arg2);
        }

        public static bool Contains(XPath2Context context, string arg1, string arg2, string collation)
        {
            CultureInfo culture = context.RunningContext.GetCulture(collation);
            return Contains(arg1, arg2);
        }

        public static bool StartsWith(string arg1, string arg2)
        {
            return arg1.StartsWith(arg2);
        }

        public static bool StartsWith(XPath2Context context, string arg1, string arg2, string collation)
        {
            CultureInfo culture = context.RunningContext.GetCulture(collation);
            return StartsWith(arg1, arg2);
        }

        public static bool EndsWith(string arg1, string arg2)
        {
            return arg1.EndsWith(arg2);
        }

        public static bool EndsWith(XPath2Context context, string arg1, string arg2, string collation)
        {
            CultureInfo culture = context.RunningContext.GetCulture(collation);
            return EndsWith(arg1, arg2);
        }

        public static string SubstringBefore(string arg1, string arg2)
        {
            int index = arg1.IndexOf(arg2);
            if (index >= 0)
                return arg1.Substring(0, index);
            return String.Empty;
        }

        public static string SubstringBefore(XPath2Context context, string arg1, string arg2, string collation)
        {
            CultureInfo culture = context.RunningContext.GetCulture(collation);
            return SubstringBefore(arg1, arg2);
        }

        public static string SubstringAfter(string arg1, string arg2)
        {
            int index = arg1.IndexOf(arg2);
            if (index >= 0)
                return arg1.Substring(index + arg2.Length);
            return String.Empty;
        }

        public static string SubstringAfter(XPath2Context context, string arg1, string arg2, string collation)
        {
            CultureInfo culture = context.RunningContext.GetCulture(collation);
            return SubstringAfter(arg1, arg2);
        }

        private static bool ParseFlags(string flagString, out RegexOptions flags)
        {
            flags = RegexOptions.None;
            foreach (char ch in flagString)
            {
                switch (ch)
                {
                    case 's':
                        flags |= RegexOptions.Singleline;
                        break;

                    case 'x':
                        flags |= RegexOptions.IgnorePatternWhitespace;
                        break;

                    case 'i':
                        flags |= RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
                        break;

                    case 'm':
                        flags |= RegexOptions.Multiline;
                        break;

                    default:
                        return false;
                }
            }
            return true;
        }

        private static bool IsValidReplacementString(string str)
        {
            char[] charArr = str.ToCharArray();
            for (int k = 0; k < charArr.Length; k++)
            {
                if (charArr[k] == '\\')
                {
                    if (k < charArr.Length - 1 && (charArr[k + 1] == '\\' || charArr[k + 1] == '$'))
                        k++;
                    else
                        return false;
                }
                if (charArr[k] == '$')
                {
                    if (k < charArr.Length - 1 && Char.IsDigit(charArr[k + 1]))
                        k++;
                    else
                        return false;
                }
            }
            return true;
        }

        private static String UnescapeReplacementString(string str) // 2011-10-28: Issue #11225
        {
            StringBuilder sb = new StringBuilder();
            char[] charArr = str.ToCharArray();
            for (int k = 0; k < charArr.Length; k++)
            {
                if (charArr[k] == '\\')
                {
                    if (k == charArr.Length - 1)
                        throw new XPath2Exception("FORX0004", Resources.FORX0004, str);
                    switch (charArr[k + 1])
                    {
                        case 'n':
                            sb.Append('\n');
                            break;

                        case 'r':
                            sb.Append('\r');
                            break;

                        case 't':
                            sb.Append('\t');
                            break;

                        case 'a':
                            sb.Append('\a');
                            break;

                        case '\\':
                            sb.Append('\\');
                            break;

                        default:
                            throw new XPath2Exception("FORX0004", Resources.FORX0004, str);
                    }
                    k++;
                }
                else
                    sb.Append(charArr[k]);
            }
            return sb.ToString();
        }


        public static bool Matches(object arg1, object arg2, string flagString)
        {
            string input;
            if (arg1 == Undefined.Value)
                input = String.Empty;
            else
                input = (string)arg1;
            string pattern;
            if (arg2 == Undefined.Value)
                throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "empty-sequence()", "xs:string in fn:matches");
            else
                pattern = (string)arg2;
            RegexOptions flags;
            if (!ParseFlags(flagString, out flags))
                throw new XPath2Exception("", Resources.InvalidRegularExpressionFlags, flagString);
            try
            {
                return new Regex(pattern, flags).IsMatch(input);
            }
            catch (ArgumentException)
            {
                throw new XPath2Exception("", Resources.InvalidRegularExpr, pattern);
            }
        }

        public static bool Matches(object arg1, object arg2)
        {
            string input;
            if (arg1 == Undefined.Value)
                input = String.Empty;
            else
                input = (string)arg1;
            string pattern;
            if (arg2 == Undefined.Value)
                throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "empty-sequence()", "xs:string in fn:matches");
            else
                pattern = (string)arg2;
            try
            {
                return new Regex(pattern).IsMatch(input);
            }
            catch (ArgumentException)
            {
                throw new XPath2Exception("", Resources.InvalidRegularExpr, pattern);
            }
        }

        public static string Replace(object arg1, object arg2, string replacement, string flagString)
        {
            string input;
            if (arg1 == Undefined.Value)
                input = String.Empty;
            else
                input = (string)arg1;
            string pattern;
            if (arg2 == Undefined.Value)
                throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "empty-sequence()", "xs:string in fn:replace");
            else
                pattern = (string)arg2;
            if (!IsValidReplacementString(replacement))
                throw new XPath2Exception("FORX0004", Resources.FORX0004, replacement);
            RegexOptions flags;
            if (!ParseFlags(flagString, out flags))
                throw new XPath2Exception("", Resources.InvalidRegularExpressionFlags, flagString);
            if (Regex.IsMatch("", pattern))
                throw new XPath2Exception("FORX0003", Resources.FORX0003, pattern);
            return Regex.Replace(input, pattern, UnescapeReplacementString(replacement), flags);
        }

        public static string Replace(object arg1, object arg2, string replacement)
        {
            string input;
            if (arg1 == Undefined.Value)
                input = String.Empty;
            else
                input = (string)arg1;
            string pattern;
            if (arg2 == Undefined.Value)
                throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "empty-sequence()", "xs:string in fn:replace");
            else
                pattern = (string)arg2;
            if (!IsValidReplacementString(replacement))
                throw new XPath2Exception("FORX0004", Resources.FORX0004, replacement);
            if (Regex.IsMatch("", pattern))
                throw new XPath2Exception("FORX0003", Resources.FORX0003, pattern);
            return Regex.Replace(input, pattern, UnescapeReplacementString(replacement));
        }

        private static IEnumerable<XPathItem> StringEnumerator(string input, string exclude, RegexOptions flags)
        {
            Regex regex = new Regex(exclude, flags);
            foreach (string str in new Regex(exclude, flags).Split(input))
            {
                if (str != "" && !regex.IsMatch(str))
                    yield return new XPath2Item(str, null);
            }
        }

        public static XPath2NodeIterator Tokenize(object arg1, object arg2, string flagString)
        {
            string input;
            if (arg1 == Undefined.Value)
                input = String.Empty;
            else
                input = (string)arg1;
            string pattern;
            if (arg2 == Undefined.Value)
                throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "empty-sequence()", "xs:string in fn:tokenize");
            else
                pattern = (string)arg2;
            RegexOptions flags;
            if (!ParseFlags(flagString, out flags))
                throw new XPath2Exception("", Resources.InvalidRegularExpressionFlags, flagString);
            if (Regex.IsMatch("", pattern))
                throw new XPath2Exception("FORX0003", Resources.FORX0003, pattern);
            return new NodeIterator(StringEnumerator(input, pattern, flags));
        }

        public static XPath2NodeIterator Tokenize(object arg1, object arg2)
        {
            string input;
            if (arg1 == Undefined.Value)
                input = String.Empty;
            else
                input = (string)arg1;
            string pattern;
            if (arg2 == Undefined.Value)
                throw new XPath2Exception("XPTY0004", Resources.XPTY0004, "empty-sequence()", "xs:string in fn:tokenize");
            else
                pattern = (string)arg2;
            if (Regex.IsMatch("", pattern))
                throw new XPath2Exception("FORX0003", Resources.FORX0003, pattern);
            return new NodeIterator(StringEnumerator(input, pattern, RegexOptions.None));
        }

        public static object YearsFromDuration(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DurationValue duration = (DurationValue)arg;
            return (Integer)duration.Years;
        }

        public static object MonthsFromDuration(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DurationValue duration = (DurationValue)arg;
            return (Integer)duration.Months;
        }

        public static object DaysFromDuration(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DurationValue duration = (DurationValue)arg;
            return (Integer)duration.Days;
        }

        public static object HoursFromDuration(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DurationValue duration = (DurationValue)arg;
            return (Integer)duration.Hours;
        }

        public static object MinutesFromDuration(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DurationValue duration = (DurationValue)arg;
            return (Integer)duration.Minutes;
        }

        public static object SecondsFromDuration(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DurationValue duration = (DurationValue)arg;
            return (decimal)duration.Seconds + (decimal)duration.Milliseconds / 1000;
        }

        public static object YearFromDateTime(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DateTimeValue dateTime = (DateTimeValue)arg;
            if (dateTime.S)
                return -dateTime.Value.Year;
            else
                return dateTime.Value.Year;
        }

        public static object MonthFromDateTime(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DateTimeValue dateTime = (DateTimeValue)arg;
            return dateTime.Value.Month;
        }

        public static object DayFromDateTime(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DateTimeValue dateTime = (DateTimeValue)arg;
            return dateTime.Value.Day;
        }

        public static object HoursFromDateTime(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DateTimeValue dateTime = (DateTimeValue)arg;
            return dateTime.Value.Hour;
        }

        public static object MinutesFromDateTime(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DateTimeValue dateTime = (DateTimeValue)arg;
            return dateTime.Value.Minute;
        }

        public static object SecondsFromDateTime(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DateTimeValue dateTime = (DateTimeValue)arg;
            return (decimal)dateTime.Value.Second + (decimal)dateTime.Value.Millisecond / 1000;
        }

        public static object TimezoneFromDateTime(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DateTimeValue dateTime = (DateTimeValue)arg;
            if (dateTime.IsLocal)
                return Undefined.Value;
            return new DayTimeDurationValue(dateTime.Value.Offset);
        }

        public static object YearFromDate(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DateValue date = (DateValue)arg;
            if (date.S)
                return -date.Value.Year;
            else
                return date.Value.Year;
        }

        public static object MonthFromDate(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DateValue date = (DateValue)arg;
            return date.Value.Month;
        }

        public static object DayFromDate(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DateValue date = (DateValue)arg;
            return date.Value.Day;
        }

        public static object TimezoneFromDate(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DateValue date = (DateValue)arg;
            if (date.IsLocal)
                return Undefined.Value;
            return new DayTimeDurationValue(date.Value.Offset);
        }

        public static object HoursFromTime(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            TimeValue time = (TimeValue)arg;
            return time.Value.Hour;
        }

        public static object MinutesFromTime(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            TimeValue time = (TimeValue)arg;
            return time.Value.Minute;
        }

        public static object SecondsFromTime(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            TimeValue time = (TimeValue)arg;
            return (decimal)time.Value.Second + (decimal)time.Value.Millisecond / 1000;
        }

        public static object TimezoneFromTime(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            TimeValue time = (TimeValue)arg;
            if (time.IsLocal)
                return Undefined.Value;
            return new DayTimeDurationValue(time.Value.Offset);
        }

        public static object AdjustDateTimeToTimezone(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DateTimeValue dateTime = (DateTimeValue)arg;
            return new DateTimeValue(dateTime.S, TimeZoneInfo.ConvertTime(dateTime.Value, TimeZoneInfo.Local));
        }

        public static object AdjustDateTimeToTimezone(object arg, object tz)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DateTimeValue dateTime = (DateTimeValue)arg;
            if (tz == Undefined.Value)
                return new DateTimeValue(dateTime.S, dateTime.Value.DateTime);
            DayTimeDurationValue _tz = (DayTimeDurationValue)tz;
            try
            {
                if (dateTime.IsLocal)
                    return new DateTimeValue(dateTime.S, new DateTimeOffset(
                        DateTime.SpecifyKind(dateTime.Value.DateTime, DateTimeKind.Unspecified), _tz.LowPartValue));
                else
                    return new DateTimeValue(dateTime.S, dateTime.Value.ToOffset(_tz.LowPartValue));
            }
            catch (ArgumentException)
            {
                throw new XPath2Exception("FODT0003", Resources.FODT0003, _tz.ToString());
            }
        }

        public static object AdjustDateToTimezone(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DateValue date = (DateValue)arg;
            return new DateValue(date.S, TimeZoneInfo.ConvertTime(date.Value, TimeZoneInfo.Local));
        }

        public static object AdjustDateToTimezone(object arg, object tz)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            DateValue date = (DateValue)arg;
            if (tz == Undefined.Value)
                return new DateValue(date.S, date.Value.Date);
            DayTimeDurationValue _tz = (DayTimeDurationValue)tz;
            try
            {
                if (date.IsLocal)
                    return new DateValue(date.S, new DateTimeOffset(
                        DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Unspecified), _tz.LowPartValue));
                else
                {
                    DateTimeOffset offs = date.Value.ToOffset(_tz.LowPartValue);
                    return new DateValue(date.S, new DateTimeOffset(offs.Date, offs.Offset));
                }
            }
            catch (ArgumentException)
            {
                throw new XPath2Exception("FODT0003", Resources.FODT0003, _tz.ToString());
            }
        }

        public static object AdjustTimeToTimezone(object arg)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            TimeValue time = (TimeValue)arg;
            return new TimeValue(TimeZoneInfo.ConvertTime(time.Value, TimeZoneInfo.Local));
        }

        public static object AdjustTimeToTimezone(object arg, object tz)
        {
            if (arg == Undefined.Value)
                return Undefined.Value;
            TimeValue time = (TimeValue)arg;
            if (tz == Undefined.Value)
                return new TimeValue(time.Value.DateTime);
            DayTimeDurationValue _tz = (DayTimeDurationValue)tz;
            try
            {
                if (time.IsLocal)
                    return new TimeValue(new DateTimeOffset(
                        DateTime.SpecifyKind(time.Value.DateTime, DateTimeKind.Unspecified), _tz.LowPartValue));
                else
                    return new TimeValue(time.Value.ToOffset(_tz.LowPartValue));
            }
            catch (ArgumentException)
            {
                throw new XPath2Exception("FODT0003", Resources.FODT0003, _tz.ToString());
            }
        }

        public static object GetAbs(object value)
        {
            if (value == Undefined.Value)
            {
                return value;
            }

            if (Integer.IsDerivedSubtype(value))
            {
                value = Integer.ToInteger(value);
            }

            switch (value)
            {
                case double valueAsDouble:
                    return Math.Abs(valueAsDouble);

                case decimal valueAsDecimal:
                    return Math.Abs(valueAsDecimal);

                case Integer valueAsInteger:
                    return (Integer)Math.Abs((decimal)valueAsInteger);

                case float valueAsFloat:
                    return Math.Abs(valueAsFloat);
            }

            throw new XPath2Exception("XPTY0004", Resources.XPTY0004, new SequenceType(value.GetType(), XmlTypeCardinality.One),
                "xs:float | xs:double | xs:decimal | xs:integer in fn:abs()");
        }

        public static object GetCeiling(object value)
        {
            if (value == Undefined.Value)
            {
                return value;
            }

            if (Integer.IsDerivedSubtype(value))
            {
                value = Integer.ToInteger(value);
            }

            switch (value)
            {
                case double valueAsDouble:
                    return Math.Ceiling(valueAsDouble);

                case decimal valueAsDecimal:
                    return Math.Ceiling(valueAsDecimal);

                case Integer valueAsInteger:
                    return (Integer)Math.Ceiling((decimal)valueAsInteger);

                case float valueAsFloat:
                    return (float)Math.Ceiling(valueAsFloat);
            }

            throw new XPath2Exception("XPTY0004", Resources.XPTY0004, new SequenceType(value.GetType(), XmlTypeCardinality.One),
                "xs:float | xs:double | xs:decimal | xs:integer in fn:ceiling()");
        }

        public static object GetFloor(object value)
        {
            if (value == Undefined.Value)
            {
                return value;
            }

            if (Integer.IsDerivedSubtype(value))
            {
                value = Integer.ToInteger(value);
            }

            switch (value)
            {
                case double valueAsDouble:
                    return Math.Floor(valueAsDouble);

                case decimal valueAsDecimal:
                    return Math.Floor(valueAsDecimal);

                case Integer valueAsInteger:
                    return (Integer)Math.Floor((decimal)valueAsInteger);

                case float valueAsFloat:
                    return (float)Math.Floor(valueAsFloat);
            }

            throw new XPath2Exception("XPTY0004", Resources.XPTY0004, new SequenceType(value.GetType(), XmlTypeCardinality.One),
                "xs:float | xs:double | xs:decimal | xs:integer in fn:floor()");
        }

        public static object GetRound(object value)
        {
            if (value == Undefined.Value)
            {
                return value;
            }

            if (Integer.IsDerivedSubtype(value))
            {
                return Integer.ToInteger(value);
            }

            switch (value)
            {
                case double valueAsDouble:
                    double fractionAsDouble = Math.Abs(valueAsDouble - Math.Floor(valueAsDouble));
                    return fractionAsDouble >= 0.5 ? Math.Ceiling(valueAsDouble) : Math.Floor(valueAsDouble);

                case decimal valueAsDecimal:
                    decimal fractionAsDecimal = Math.Abs(valueAsDecimal - Math.Floor(valueAsDecimal));
                    return fractionAsDecimal >= 0.5m ? Math.Ceiling(valueAsDecimal) : Math.Floor(valueAsDecimal);

                case float valueAsFloat:
                    var fractionAsFloat = Math.Abs(valueAsFloat - Math.Floor(valueAsFloat));
                    return (float)(fractionAsFloat >= 0.5 ? Math.Ceiling(valueAsFloat) : Math.Floor(valueAsFloat));
            }

            throw new XPath2Exception("XPTY0004", Resources.XPTY0004, new SequenceType(value.GetType(), XmlTypeCardinality.One),
                "xs:float | xs:double | xs:decimal | xs:integer in fn:round()");
        }

        public static object GetRoundHalfToEven(object value)
        {
            if (value == Undefined.Value)
            {
                return value;
            }

            if (Integer.IsDerivedSubtype(value))
            {
                value = Integer.ToInteger(value);
            }

            switch (value)
            {
                case double valueAsDouble:
                    return Math.Round(valueAsDouble, MidpointRounding.ToEven);

                case decimal valueAsDecimal:
                    return Math.Round(valueAsDecimal, MidpointRounding.ToEven);

                case Integer valueAsInteger:
                    return (Integer)Math.Round((decimal)valueAsInteger, MidpointRounding.ToEven);

                case float valueAsFloat:
                    return (float)Math.Round(valueAsFloat, MidpointRounding.ToEven);
            }

            throw new XPath2Exception("XPTY0004", Resources.XPTY0004, new SequenceType(value.GetType(), XmlTypeCardinality.One),
                "xs:float | xs:double | xs:decimal | xs:integer in fn:round-half-to-even()");
        }

        public static object GetRoundHalfToEven(object value, object precision)
        {
            if (value == Undefined.Value || precision == Undefined.Value)
            {
                return Undefined.Value;
            }

            if (Integer.IsDerivedSubtype(value))
            {
                value = Integer.ToInteger(value);
            }

            int p = (int)(Integer)precision;
            if (p < 0)
            {
                int pow = 1;
                for (int k = 1; k <= -p; k++)
                {
                    pow = pow * 10;
                }

                switch (value)
                {
                    case double valueAsDouble:
                        return pow * Math.Round(valueAsDouble / pow, MidpointRounding.ToEven);

                    case decimal valueAsDecimal:
                        return pow * Math.Round(valueAsDecimal / pow, MidpointRounding.ToEven);

                    case Integer valueAsInteger:
                        return pow * (Integer)Math.Round((decimal)valueAsInteger / pow, MidpointRounding.ToEven);

                    case float valueAsFloat:
                        return pow * Math.Round(valueAsFloat / pow, MidpointRounding.ToEven);
                }
            }

            switch (value)
            {
                case double valueAsDouble:
                    return Math.Round(valueAsDouble, p, MidpointRounding.ToEven);

                case decimal valueAsDecimal:
                    return Math.Round(valueAsDecimal, p, MidpointRounding.ToEven);

                case Integer valueAsInteger:
                    return (Integer)Math.Round((decimal)valueAsInteger, p, MidpointRounding.ToEven);

                case float valueAsFloat:
                    return Math.Round(valueAsFloat, p, MidpointRounding.ToEven);

            }

            throw new XPath2Exception("XPTY0004", Resources.XPTY0004, new SequenceType(value.GetType(), XmlTypeCardinality.One),
                "xs:float | xs:double | xs:decimal | xs:integer in fn:round-half-to-even()");
        }

        public static object Compare(XPath2Context context, object a, object b, string collation)
        {
            if (a == Undefined.Value || b == Undefined.Value)
                return Undefined.Value;
            CultureInfo culture = context.RunningContext.GetCulture(collation);
            String s1 = (String)a;
            String s2 = (String)b;
            return String.Compare(s1, s2, false, culture);
        }

        public static object Compare(object a, object b)
        {
            if (a == Undefined.Value || b == Undefined.Value)
                return Undefined.Value;
            String s1 = (String)a;
            String s2 = (String)b;
            int res = String.CompareOrdinal(s1, s2);
            if (res > 0)
                return 1;
            else if (res < 0)
                return -1;
            return 0;
        }

        public static object CodepointEqual(object a, object b)
        {
            if (a == Undefined.Value || b == Undefined.Value)
                return Undefined.Value;
            String s1 = (String)a;
            String s2 = (String)b;
            return String.CompareOrdinal(s1, s2) == 0;
        }

        public static bool EmptySequence(XPath2NodeIterator iter)
        {
            XPath2NodeIterator probe = iter.Clone();
            return !probe.MoveNext();
        }

        public static bool ExistsSequence(XPath2NodeIterator iter)
        {
            return !EmptySequence(iter);
        }

        private static IEnumerable<XPathItem> ReverseIterator(LinkedList<XPathItem> list)
        {
            LinkedListNode<XPathItem> node = list.Last;
            while (node != null)
            {
                yield return node.Value;
                node = node.Previous;
            }
        }

        public static XPath2NodeIterator ReverseSequence(XPath2NodeIterator iter)
        {
            LinkedList<XPathItem> list = new LinkedList<XPathItem>();
            foreach (XPathItem item in iter)
                list.AddLast(item.Clone());
            return new NodeIterator(ReverseIterator(list));
        }

        private static IEnumerable<XPathItem> IndexOfIterator(XPath2NodeIterator iter, object value, CultureInfo collation)
        {
            Integer pos = 1;
            if (value is UntypedAtomic || value is AnyUriValue)
                value = value.ToString();
            foreach (XPathItem item in iter)
            {
                bool res;
                object curr = item.GetTypedValue();
                if (curr is UntypedAtomic || curr is AnyUriValue)
                    curr = curr.ToString();
                if (ValueProxy.Eq(curr, value, out res) && res)
                    yield return new XPath2Item(pos);
                pos++;
            }
        }

        public static XPath2NodeIterator IndexOfSequence(XPath2NodeIterator iter, object value)
        {
            return new NodeIterator(IndexOfIterator(iter, value, null));
        }

        public static XPath2NodeIterator IndexOfSequence(XPath2Context context, XPath2NodeIterator iter, object value, string collation)
        {
            return new NodeIterator(IndexOfIterator(iter, value, context.RunningContext.GetCulture(collation)));
        }

        private static IEnumerable<XPathItem> RemoveIterator(XPath2NodeIterator iter, int index)
        {
            int pos = 1;
            foreach (XPathItem item in iter)
            {
                if (index != pos)
                    yield return item;
                pos++;
            }
        }

        public static XPath2NodeIterator Remove(XPath2NodeIterator iter, int index)
        {
            return new NodeIterator(RemoveIterator(iter, index));
        }

        private static IEnumerable<XPathItem> InsertIterator(XPath2NodeIterator iter, int index, XPath2NodeIterator iter2)
        {
            int pos = 1;
            if (index < pos)
            {
                foreach (XPathItem item2 in iter2)
                    yield return item2;
            }
            foreach (XPathItem item in iter)
            {
                if (index == pos)
                    foreach (XPathItem item2 in iter2)
                        yield return item2;
                yield return item;
                pos++;
            }
            if (pos <= index)
            {
                foreach (XPathItem item2 in iter2)
                    yield return item2;
            }
        }

        public static XPath2NodeIterator InsertBefore(XPath2NodeIterator iter, int index, XPath2NodeIterator iter2)
        {
            return new NodeIterator(InsertIterator(iter, index, iter2));
        }

        private static IEnumerable<XPathItem> SubsequenceIterator(XPath2NodeIterator iter, double startingLoc, double length)
        {
            if (startingLoc < 1)
            {
                length = length + startingLoc - 1;
                startingLoc = 1;
            }
            int pos = 1;
            foreach (XPathItem item in iter)
            {
                if (startingLoc <= pos)
                {
                    if (length <= 0)
                        break;
                    yield return item;
                    length--;
                }
                pos++;
            }
        }

        public static XPath2NodeIterator Subsequence(XPath2NodeIterator iter, double startingLoc)
        {
            if (Double.IsInfinity(startingLoc) || Double.IsNaN(startingLoc))
                return EmptyIterator.Shared;
            return new NodeIterator(SubsequenceIterator(iter, startingLoc, Double.PositiveInfinity));
        }

        public static XPath2NodeIterator Subsequence(XPath2NodeIterator iter, double startingLoc, double length)
        {
            if (Double.IsInfinity(startingLoc) || Double.IsNaN(startingLoc) ||
                Double.IsNegativeInfinity(length) || Double.IsNaN(length))
                return EmptyIterator.Shared;
            return new NodeIterator(SubsequenceIterator(iter, startingLoc, length));
        }

        public static XPath2NodeIterator Unordered(XPath2NodeIterator iter)
        {
            return iter.Clone();
        }

        public static object ZeroOrOne(XPath2NodeIterator iter)
        {
            XPath2NodeIterator probe = iter.Clone();
            if (probe.MoveNext())
            {
                object res;
                if (probe.Current.IsNode)
                    res = probe.Current.Clone();
                else
                    res = probe.Current.GetTypedValue();
                if (probe.MoveNext())
                    throw new XPath2Exception("FORG0003", Resources.FORG0003);
                return res;
            }
            return Undefined.Value;
        }

        public static XPath2NodeIterator OneOrMore(XPath2NodeIterator iter)
        {
            iter = iter.CreateBufferedIterator();
            XPath2NodeIterator probe = iter.Clone();
            if (!probe.MoveNext())
                throw new XPath2Exception("FORG0004", Resources.FORG0004);
            return iter;
        }

        public static object ExactlyOne(XPath2NodeIterator iter)
        {
            XPath2NodeIterator probe = iter.Clone();
            if (!probe.MoveNext())
                throw new XPath2Exception("FORG0005", Resources.FORG0005);
            object res;
            if (probe.Current.IsNode)
                res = probe.Current.Clone();
            else
                res = probe.Current.GetTypedValue();
            if (probe.MoveNext())
                throw new XPath2Exception("FORG0005", Resources.FORG0005);
            return res;
        }

        private class DistinctComparer : IComparer<object>
        {
            public DistinctComparer()
            {
            }

            #region IComparer<XPathItem> Members

            int IComparer<object>.Compare(object a, object b)
            {
                if (a is UntypedAtomic || a is AnyUriValue)
                    a = a.ToString();
                if (b is UntypedAtomic || b is AnyUriValue)
                    b = b.ToString();
                bool res;
                if (a is Single && Single.IsNaN((float)a) ||
                    a is Double && Double.IsNaN((double)a))
                    a = Double.NaN;
                if (b is Single && Single.IsNaN((float)b) ||
                    b is Double && Double.IsNaN((double)b))
                    b = Double.NaN;
                if (a.Equals(b))
                    return 0;
                if (ValueProxy.Eq(a, b, out res) && res)
                    return 0;
                if (ValueProxy.Gt(a, b, out res) && res)
                    return 1;
                return -1;
            }

            #endregion
        }

        private static IEnumerable<XPathItem> DistinctIterator(XPath2NodeIterator iter, CultureInfo cultute)
        {
            SortedDictionary<object, object> dict =
                new SortedDictionary<object, object>(new DistinctComparer());
            iter = iter.Clone();
            while (iter.MoveNext())
            {
                XPathItem item = iter.Current;
                if (item.Value != String.Empty)
                {
                    object value = item.GetTypedValue();
                    if (!dict.ContainsKey(value))
                    {
                        yield return new XPath2Item(value);
                        dict.Add(value, null);
                    }
                }
            }
        }

        public static XPath2NodeIterator DistinctValues(XPath2NodeIterator iter)
        {
            return new NodeIterator(DistinctIterator(iter, null));
        }

        public static XPath2NodeIterator DistinctValues(XPath2Context context, XPath2NodeIterator iter, string collation)
        {
            return new NodeIterator(DistinctIterator(iter, context.RunningContext.GetCulture(collation)));
        }

        public static bool DeepEqual(XPath2NodeIterator iter1, XPath2NodeIterator iter2)
        {
            TreeComparer comparer = new TreeComparer();
            return comparer.DeepEqual(iter1, iter2);
        }

        public static bool DeepEqual(XPath2Context context, XPath2NodeIterator iter1, XPath2NodeIterator iter2, string collation)
        {
            TreeComparer comparer = new TreeComparer(context.RunningContext.GetCulture(collation));
            return comparer.DeepEqual(iter1, iter2);
        }

        public static int CountValues(XPath2NodeIterator iter)
        {
            return iter.Count;
        }

        public static object MaxValue(XPath2Context context, XPath2NodeIterator iter, string collation)
        {
            CultureInfo culture = context.RunningContext.GetCulture(collation);
            ValueProxy acc = null;
            iter = iter.Clone();
            while (iter.MoveNext())
            {
                object curr = CoreFuncs.CastToNumber1(context, iter.Current.GetTypedValue());
                if (curr is AnyUriValue)
                    curr = curr.ToString();
                try
                {
                    if (acc == null)
                        acc = ValueProxy.New(curr);
                    else
                        acc = ValueProxy.Max(acc, ValueProxy.New(curr));
                }
                catch (InvalidCastException)
                {
                    throw new XPath2Exception("FORG0006", Resources.FORG0006, "fn:max()",
                        new SequenceType(curr.GetType(), XmlTypeCardinality.One));
                }
            }
            if (acc == null)
                return Undefined.Value;
            return acc.Value;
        }

        public static object MaxValue(XPath2Context context, XPath2NodeIterator iter)
        {
            return MaxValue(context, iter, null);
        }

        public static object MinValue(XPath2Context context, XPath2NodeIterator iter, string collation)
        {
            CultureInfo culture = context.RunningContext.GetCulture(collation);
            ValueProxy acc = null;
            iter = iter.Clone();
            while (iter.MoveNext())
            {
                object curr = CoreFuncs.CastToNumber1(context, iter.Current.GetTypedValue());
                if (curr is AnyUriValue)
                    curr = curr.ToString();
                try
                {
                    if (acc == null)
                        acc = ValueProxy.New(curr);
                    else
                        acc = ValueProxy.Min(acc, ValueProxy.New(curr));
                }
                catch (InvalidCastException)
                {
                    throw new XPath2Exception("FORG0006", Resources.FORG0006, "fn:min",
                        new SequenceType(curr.GetType(), XmlTypeCardinality.One));
                }
            }
            if (acc == null)
                return Undefined.Value;
            return acc.Value;
        }

        public static object MinValue(XPath2Context context, XPath2NodeIterator iter)
        {
            return MinValue(context, iter, null);
        }

        public static object SumValue(XPath2Context context, XPath2NodeIterator iter)
        {
            return SumValue(context, iter, 0);
        }

        public static object SumValue(XPath2Context context, XPath2NodeIterator iter, object zero)
        {
            ValueProxy acc = null;
            foreach (XPathItem item in iter)
            {
                ValueProxy arg;
                try
                {
                    arg = ValueProxy.New(CoreFuncs.CastToNumber1(context, item.GetTypedValue()));
                    if (!(arg.IsNumeric() ||
                            arg.Value is YearMonthDurationValue || arg.Value is DayTimeDurationValue))
                        throw new XPath2Exception("FORG0006", Resources.FORG0006, "fn:sum()",
                            new SequenceType(item.GetTypedValue().GetType(), XmlTypeCardinality.One));
                    if (Integer.IsDerivedSubtype(arg.Value))
                        arg = (Integer)Convert.ToDecimal(arg);
                }
                catch (InvalidCastException)
                {
                    throw new XPath2Exception("FORG0006", Resources.FORG0006, "fn:sum()",
                        new SequenceType(item.GetTypedValue().GetType(), XmlTypeCardinality.One));
                }
                if (acc == null)
                    acc = arg;
                else
                    acc = acc + arg;
            }
            return acc != null ? acc.Value : zero;
        }

        public static object AvgValue(XPath2Context context, XPath2NodeIterator iter)
        {
            ValueProxy acc = null;
            int count = 0;
            foreach (XPathItem item in iter)
            {
                ValueProxy arg;
                try
                {
                    arg = ValueProxy.New(CoreFuncs.CastToNumber1(context, item.GetTypedValue()));
                    if (!(arg.IsNumeric() ||
                            arg.Value is YearMonthDurationValue || arg.Value is DayTimeDurationValue))
                        throw new XPath2Exception("FORG0006", Resources.FORG0006, "fn:avg()",
                            new SequenceType(item.GetTypedValue().GetType(), XmlTypeCardinality.One));
                    if (Integer.IsDerivedSubtype(arg.Value))
                        arg = (Integer)Convert.ToDecimal(arg);
                }
                catch (InvalidCastException)
                {
                    throw new XPath2Exception("FORG0006", Resources.FORG0006, "fn:avg()",
                        new SequenceType(item.GetTypedValue().GetType(), XmlTypeCardinality.One));
                }
                if (acc == null)
                    acc = arg;
                else
                    acc = acc + arg;
                count = count + 1;
            }
            if (acc == null)
                return Undefined.Value;
            return (acc / count).Value;
        }

        public static object CreateDateTime(object dateArg, object timeArg)
        {
            if (dateArg == Undefined.Value ||
                timeArg == Undefined.Value)
                return Undefined.Value;
            DateValue date = (DateValue)dateArg;
            TimeValue time = (TimeValue)timeArg;
            if (!date.IsLocal || !time.IsLocal)
            {
                TimeSpan offset;
                if (!date.IsLocal && !time.IsLocal)
                {
                    if (date.Value.Offset != time.Value.Offset)
                        throw new XPath2Exception("FORG0008", Resources.FORG0008);
                    offset = date.Value.Offset;
                }
                else
                    if (time.IsLocal)
                    offset = date.Value.Offset;
                else
                    offset = time.Value.Offset;
                return new DateTimeValue(date.S, new DateTimeOffset(date.Value.Year, date.Value.Month,
                    date.Value.Day, time.Value.Hour, time.Value.Minute, time.Value.Second, offset));
            }
            else
                return new DateTimeValue(date.S, date.Value.Date, time.Value.DateTime);
        }

        public static DateTimeValue GetCurrentDateTime(XPath2Context context)
        {
            return new DateTimeValue(false, new DateTimeOffset(context.RunningContext.Now));
        }

        public static DateValue GetCurrentDate(XPath2Context context)
        {
            DateValue res = new DateValue(false, context.RunningContext.Now.Date);
            res.IsLocal = false;
            return res;
        }

        public static TimeValue GetCurrentTime(XPath2Context context)
        {
            return new TimeValue(new DateTimeOffset(context.RunningContext.Now));
        }

        public static void ScanLocalNamespaces(XmlNamespaceManager nsmgr, XPathNavigator node, bool recursive)
        {
            if (node.NodeType == XPathNodeType.Root)
                node.MoveToChild(XPathNodeType.Element);
            else
                if (recursive)
            {
                XPathNavigator parent = node.Clone();
                if (parent.MoveToParent())
                    ScanLocalNamespaces(nsmgr, parent, recursive);
            }
            bool defaultNS = false;
            string prefix = node.Prefix;
            string ns = node.NamespaceURI;
            nsmgr.PushScope();
            if (node.MoveToFirstNamespace(XPathNamespaceScope.Local))
            {
                do
                {
                    nsmgr.AddNamespace(node.Name, node.Value);
                    if (node.Name == prefix)
                        defaultNS = true;
                }
                while (node.MoveToNextNamespace(XPathNamespaceScope.Local));
            }
            if (!defaultNS && ns != "")
                nsmgr.AddNamespace(prefix, ns);
        }

        private static IEnumerable<XPathItem> PrefixEnumerator(XPathNavigator nav)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(nav.NameTable);
            ScanLocalNamespaces(nsmgr, nav.Clone(), false);
            foreach (KeyValuePair<string, string> kvp in nsmgr.GetNamespacesInScope(XmlNamespaceScope.All))
                yield return new XPath2Item(kvp.Key);
        }

        public static XPath2NodeIterator GetInScopePrefixes(XPathNavigator nav)
        {
            return new NodeIterator(PrefixEnumerator(nav));
        }

        public static object GetNamespaceUriForPrefix(XPath2Context context, object prefix, XPathNavigator nav)
        {
            string ns;
            if (prefix == Undefined.Value || (string)prefix == String.Empty)
                ns = nav.NamespaceURI;
            else
            {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(context.NameTable);
                ScanLocalNamespaces(nsmgr, nav.Clone(), false);
                ns = nsmgr.LookupNamespace((string)prefix);
            }
            if (ns == null)
                return Undefined.Value;
            return new AnyUriValue(ns);
        }

        public static object ResolveQName(XPath2Context context, object qname, XPathNavigator nav)
        {
            if (qname == Undefined.Value)
                return Undefined.Value;
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(context.NameTable);
            ScanLocalNamespaces(nsmgr, nav.Clone(), true);
            return new XPath2Item(QNameValue.Parse((string)qname, nsmgr));
        }

        public static QNameValue CreateQName(XPath2Context context, object ns, string qname)
        {
            if (ns == Undefined.Value)
                ns = String.Empty;
            return QNameValue.Parse(qname, (string)ns, context.NameTable);
        }

        public static object PrefixFromQName(object qname)
        {
            if (qname == Undefined.Value)
                return qname;
            QNameValue qnameValue = qname as QNameValue;
            if (qnameValue == null)
                throw new XPath2Exception("XPTY0004", Resources.XPTY0004,
                    new SequenceType(qname.GetType(), XmlTypeCardinality.One), "QName");
            if (qnameValue.Prefix == "")
                return Undefined.Value;
            return qnameValue.Prefix;
        }

        public static object LocalNameFromQName(object qname)
        {
            if (qname == Undefined.Value)
                return qname;
            QNameValue qnameValue = qname as QNameValue;
            if (qnameValue == null)
                throw new XPath2Exception("XPTY0004", Resources.XPTY0004,
                    new SequenceType(qname.GetType(), XmlTypeCardinality.One), "QName");
            return qnameValue.LocalName;
        }

        public static object NamespaceUriFromQName(object qname)
        {
            if (qname == Undefined.Value)
                return Undefined.Value;
            QNameValue qnameValue = qname as QNameValue;
            if (qnameValue == null)
                throw new XPath2Exception("XPTY0004", Resources.XPTY0004,
                    new SequenceType(qname.GetType(), XmlTypeCardinality.One), "QName");
            return new AnyUriValue(qnameValue.NamespaceUri);
        }

        public static XPath2NodeIterator StringToCodepoint(object text)
        {
            if (text == Undefined.Value)
                return EmptyIterator.Shared;
            return new NodeIterator(CoreFuncs.CodepointIterator((string)text));
        }

        public static string CodepointToString(XPath2NodeIterator iter)
        {
            StringBuilder sb = new StringBuilder();
            foreach (XPathItem item in iter)
            {
                int codepoint = item.ValueAsInt;
                if (!XmlCharType.Instance.IsCharData((char)codepoint))
                    throw new XPath2Exception("FOCH0001", Resources.FOCH0001, codepoint.ToString("X"));
                try
                {
                    sb.Append(Convert.ToChar(codepoint));
                }
                catch
                {
                    throw new XPath2Exception("FOCH0001", Resources.FOCH0001, codepoint.ToString("X"));
                }
            }
            return sb.ToString();
        }

        public static string DefaultCollation(XPath2Context context)
        {
            return XmlReservedNs.NsCollationCodepoint;
        }

        public static object ResolveUri(XPath2Context context, object relative)
        {
            if (relative == Undefined.Value)
                return Undefined.Value;
            string rel = (string)relative;
            if (context.RunningContext.BaseUri == null)
                throw new XPath2Exception("FONS0005", Resources.FONS0005);
            try
            {
                return new AnyUriValue(new Uri(new Uri(context.RunningContext.BaseUri), rel));
            }
            catch (UriFormatException)
            {
                throw new XPath2Exception("FORG0009", Resources.FORG0009);
            }
        }

        public static object ResolveUri(object relative, object baseUri)
        {
            if (relative == Undefined.Value)
                return Undefined.Value;
            string rel = (string)relative;
            if (baseUri == Undefined.Value)
                return Undefined.Value;
            string bsUri = (string)baseUri;
            if (bsUri == "")
            {
                if (!Uri.IsWellFormedUriString(rel, UriKind.Absolute))
                    throw new XPath2Exception("FORG0009", Resources.FORG0009);
                return new AnyUriValue(rel);
            }
            try
            {
                return new AnyUriValue(new Uri(new Uri(bsUri), rel));
            }
            catch (UriFormatException)
            {
                throw new XPath2Exception("FORG0009", Resources.FORG0009);
            }
        }

        public static object StaticBaseUri(XPath2Context context)
        {
            if (context.RunningContext.BaseUri == null)
                return Undefined.Value;
            return new AnyUriValue(context.RunningContext.BaseUri);
        }

        public static DayTimeDurationValue ImplicitTimezone(XPath2Context context)
        {
            return new DayTimeDurationValue(new DateTimeOffset(context.RunningContext.Now).Offset);
        }

        public static bool NodeLang(IContextProvider provider, object testLang)
        {
            return NodeLang(testLang, CoreFuncs.ContextNode(provider));
        }

        public static bool NodeLang(object testLang, object node)
        {
            if (node == Undefined.Value)
                return false;
            XPathNavigator nav = node as XPathNavigator;
            if (nav == null)
                throw new XPath2Exception("XPTY0004", Resources.XPTY0004,
                    new SequenceType(node.GetType(), XmlTypeCardinality.ZeroOrOne), "node()? in fn:lang()");
            string xmlLang = nav.XmlLang;
            if (xmlLang == "")
                return false;
            string lang = (testLang == Undefined.Value) ?
                String.Empty : (string)testLang;
            if (String.Compare(xmlLang, lang, StringComparison.OrdinalIgnoreCase) == 0)
                return true;
            int index = xmlLang.IndexOf('-');
            if (index != -1)
                return String.Compare(xmlLang.Substring(0, index), lang,
                    StringComparison.OrdinalIgnoreCase) == 0;
            return false;
        }

        public static int CurrentPosition(IContextProvider provider)
        {
            return provider.CurrentPosition;
        }

        public static int LastPosition(IContextProvider provider)
        {
            return provider.LastPosition;
        }
    }
}
