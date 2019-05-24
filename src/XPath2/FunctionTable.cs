// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System.Collections.Generic;
using System.Xml.Schema;
using Wmhelp.XPath2.MS;

namespace Wmhelp.XPath2
{
    public delegate object XPathFunctionDelegate(XPath2Context context, IContextProvider provider, object[] args);

    public class XPathFunctionDef
    {
        public string Name { get; private set; }

        public XPathFunctionDelegate Delegate { get; }

        public XPath2ResultType ResultType { get; private set; }

        public XPathFunctionDef(string name, XPathFunctionDelegate _delegate, XPath2ResultType resultType)
        {
            Name = name;
            Delegate = _delegate;
            ResultType = resultType;
        }

        public object Invoke(XPath2Context context, IContextProvider provider, object[] args)
        {
            return Delegate?.Invoke(context, provider, args);
        }
    }

    public class FunctionTable
    {
        private readonly Dictionary<FunctionDesc, XPathFunctionDef> _funcTable;

        private FunctionTable()
        {
            _funcTable = new Dictionary<FunctionDesc, XPathFunctionDef>();

            Add(XmlReservedNs.NsXQueryFunc, "dateTime", 2, XPath2ResultType.DateTime, (context, provider, args) =>
                ExtFuncs.CreateDateTime(
                   CoreFuncs.CastArg(context, args[0], new SequenceType(XmlTypeCode.Date, XmlTypeCardinality.ZeroOrOne)),
                   CoreFuncs.CastArg(context, args[1], new SequenceType(XmlTypeCode.Time, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "current-dateTime", 0, XPath2ResultType.DateTime, (context, provider, args) =>
                    ExtFuncs.GetCurrentDateTime(context));
            Add(XmlReservedNs.NsXQueryFunc, "current-date", 0, XPath2ResultType.DateTime, (context, provider, args) =>
                    ExtFuncs.GetCurrentDate(context));
            Add(XmlReservedNs.NsXQueryFunc, "current-time", 0, XPath2ResultType.DateTime, (context, provider, args) =>
                    ExtFuncs.GetCurrentTime(context));
            Add(XmlReservedNs.NsXQueryFunc, "in-scope-prefixes", 1, XPath2ResultType.NodeSet, (context, provider, args) =>
                ExtFuncs.GetInScopePrefixes(CoreFuncs.NodeValue(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "namespace-uri-for-prefix", 2, XPath2ResultType.AnyUri, (context, provider, args) =>
                ExtFuncs.GetNamespaceUriForPrefix(context, CoreFuncs.Atomize(args[0]), CoreFuncs.NodeValue(args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "resolve-QName", 2, XPath2ResultType.QName, (context, provider, args) =>
                ExtFuncs.ResolveQName(context, CoreFuncs.Atomize(args[0]), CoreFuncs.NodeValue(args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "QName", 2, XPath2ResultType.QName, (context, provider, args) =>
                ExtFuncs.CreateQName(context, CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringExactOne(context, args[1], false)));
            Add(XmlReservedNs.NsXQueryFunc, "prefix-from-QName", 1, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.PrefixFromQName(CoreFuncs.Atomize(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "local-name-from-QName", 1, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.LocalNameFromQName(CoreFuncs.Atomize(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "namespace-uri-from-QName", 1, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.NamespaceUriFromQName(CoreFuncs.Atomize(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "string-to-codepoints", 1, XPath2ResultType.NodeSet, (context, provider, args) =>
                ExtFuncs.StringToCodepoint(CoreFuncs.CastToStringOptional(context, args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "codepoints-to-string", 1, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.CodepointToString((XPath2NodeIterator)CoreFuncs.CastArg(context, args[0],
                    new SequenceType(XmlTypeCode.Int, XmlTypeCardinality.ZeroOrMore))));
            Add(XmlReservedNs.NsXQueryFunc, "default-collation", 0, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.DefaultCollation(context));
            Add(XmlReservedNs.NsXQueryFunc, "resolve-uri", 2, XPath2ResultType.AnyUri, (context, provider, args) =>
                ExtFuncs.ResolveUri(CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringOptional(context, args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "resolve-uri", 1, XPath2ResultType.AnyUri, (context, provider, args) =>
                ExtFuncs.ResolveUri(context, CoreFuncs.CastToStringOptional(context, args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "static-base-uri", 0, XPath2ResultType.AnyUri, (context, provider, args) =>
                ExtFuncs.StaticBaseUri(context));
            Add(XmlReservedNs.NsXQueryFunc, "implicit-timezone", 0, XPath2ResultType.Duration, (context, provider, args) =>
                ExtFuncs.ImplicitTimezone(context));
            Add(XmlReservedNs.NsXQueryFunc, "lang", 2, XPath2ResultType.Boolean, (context, provider, args) =>
                ExtFuncs.NodeLang(CoreFuncs.CastToStringOptional(context, args[0]), CoreFuncs.NodeValue(args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "lang", 1, XPath2ResultType.Boolean, (context, provider, args) =>
                ExtFuncs.NodeLang(provider, CoreFuncs.CastToStringOptional(context, args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "name", 1, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.GetName(CoreFuncs.NodeValue(args[0], false)));
            Add(XmlReservedNs.NsXQueryFunc, "name", 0, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.GetName(provider));
            Add(XmlReservedNs.NsXQueryFunc, "node-name", 1, XPath2ResultType.QName, (context, provider, args) =>
                ExtFuncs.GetNodeName(context, CoreFuncs.NodeValue(args[0], false)));
            Add(XmlReservedNs.NsXQueryFunc, "local-name", 1, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.GetLocalName(CoreFuncs.NodeValue(args[0], false)));
            Add(XmlReservedNs.NsXQueryFunc, "local-name", 0, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.GetLocalName(provider));
            Add(XmlReservedNs.NsXQueryFunc, "namespace-uri", 1, XPath2ResultType.AnyUri, (context, provider, args) =>
                ExtFuncs.GetNamespaceUri(CoreFuncs.NodeValue(args[0], false)));
            Add(XmlReservedNs.NsXQueryFunc, "namespace-uri", 0, XPath2ResultType.AnyUri, (context, provider, args) =>
                ExtFuncs.GetNamespaceUri(provider));
            Add(XmlReservedNs.NsXQueryFunc, "nilled", 1, XPath2ResultType.Boolean, (context, provider, args) =>
                ExtFuncs.GetNilled(CoreFuncs.NodeValue(args[0], false)));
            Add(XmlReservedNs.NsXQueryFunc, "base-uri", 0, XPath2ResultType.AnyUri, (context, provider, args) =>
                ExtFuncs.GetBaseUri(context, provider));
            Add(XmlReservedNs.NsXQueryFunc, "base-uri", 1, XPath2ResultType.AnyUri, (context, provider, args) =>
                ExtFuncs.GetBaseUri(context, CoreFuncs.NodeValue(args[0], false)));
            Add(XmlReservedNs.NsXQueryFunc, "document-uri", 1, XPath2ResultType.AnyUri, (context, provider, args) =>
                ExtFuncs.DocumentUri(CoreFuncs.NodeValue(args[0], false)));
            Add(XmlReservedNs.NsXQueryFunc, "trace", 1, XPath2ResultType.NodeSet, (context, provider, args) =>
                ExtFuncs.WriteTrace(context, XPath2NodeIterator.Create(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "trace", 2, XPath2ResultType.NodeSet, (context, provider, args) =>
                ExtFuncs.WriteTrace(context, XPath2NodeIterator.Create(args[0]),
                CoreFuncs.CastToStringExactOne(context, args[1], false)));
            Add(XmlReservedNs.NsXQueryFunc, "data", 1, XPath2ResultType.NodeSet, (context, provider, args) =>
                ExtFuncs.GetData(XPath2NodeIterator.Create(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "concat", XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.Concat(context, args));
            Add(XmlReservedNs.NsXQueryFunc, "string-join", 2, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.StringJoin(XPath2NodeIterator.Create(args[0]),
                   CoreFuncs.CastToStringExactOne(context, args[1], false)));
            Add(XmlReservedNs.NsXQueryFunc, "substring", 3, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.Substring(CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.Number(context, args[1]), CoreFuncs.Number(context, args[2])));
            Add(XmlReservedNs.NsXQueryFunc, "substring", 2, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.Substring(CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.Number(context, args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "string-length", 0, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.StringLength(context, provider));
            Add(XmlReservedNs.NsXQueryFunc, "string-length", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.StringLength(CoreFuncs.CastToStringOptional(context, args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "normalize-space", 0, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.NormalizeSpace(context, provider));
            Add(XmlReservedNs.NsXQueryFunc, "normalize-space", 1, XPath2ResultType.String, (context, provider, args) =>
                CoreFuncs.NormalizeSpace(CoreFuncs.CastToStringOptional(context, args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "normalize-unicode", 1, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.NormalizeUnicode(CoreFuncs.CastToStringOptional(context, args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "normalize-unicode", 2, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.NormalizeUnicode(CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringExactOne(context, args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "upper-case", 1, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.UpperCase(CoreFuncs.CastToStringOptional(context, args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "lower-case", 1, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.LowerCase(CoreFuncs.CastToStringOptional(context, args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "translate", 3, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.Translate(CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringExactOne(context, args[1]),
                    CoreFuncs.CastToStringExactOne(context, args[2])));
            Add(XmlReservedNs.NsXQueryFunc, "encode-for-uri", 1, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.EncodeForUri(CoreFuncs.CastToStringOptional(context, args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "iri-to-uri", 1, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.IriToUri(CoreFuncs.CastToStringOptional(context, args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "escape-html-uri", 1, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.EscapeHtmlUri(CoreFuncs.CastToStringOptional(context, args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "contains", 3, XPath2ResultType.Boolean, (context, provider, args) =>
                ExtFuncs.Contains(context, CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringOptional(context, args[1]),
                    CoreFuncs.CastToStringExactOne(context, args[2], false)));
            Add(XmlReservedNs.NsXQueryFunc, "contains", 2, XPath2ResultType.Boolean, (context, provider, args) =>
                ExtFuncs.Contains(CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringOptional(context, args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "starts-with", 3, XPath2ResultType.Boolean, (context, provider, args) =>
                ExtFuncs.StartsWith(context, CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringOptional(context, args[1]),
                    CoreFuncs.CastToStringExactOne(context, args[2])));
            Add(XmlReservedNs.NsXQueryFunc, "starts-with", 2, XPath2ResultType.Boolean, (context, provider, args) =>
                ExtFuncs.StartsWith(CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringOptional(context, args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "ends-with", 3, XPath2ResultType.Boolean, (context, provider, args) =>
                ExtFuncs.EndsWith(context, CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringOptional(context, args[1]),
                    CoreFuncs.CastToStringExactOne(context, args[2])));
            Add(XmlReservedNs.NsXQueryFunc, "ends-with", 2, XPath2ResultType.Boolean, (context, provider, args) =>
                ExtFuncs.EndsWith(CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringOptional(context, args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "substring-before", 3, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.SubstringBefore(context, CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringOptional(context, args[1]),
                    CoreFuncs.CastToStringExactOne(context, args[2])));
            Add(XmlReservedNs.NsXQueryFunc, "substring-before", 2, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.SubstringBefore(CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringOptional(context, args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "substring-after", 3, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.SubstringAfter(context, CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringOptional(context, args[1]),
                    CoreFuncs.CastToStringExactOne(context, args[2])));
            Add(XmlReservedNs.NsXQueryFunc, "substring-after", 2, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.SubstringAfter(CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringOptional(context, args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "matches", 2, XPath2ResultType.Boolean, (context, provider, args) =>
                ExtFuncs.Matches(CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringOptional(context, args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "matches", 3, XPath2ResultType.Boolean, (context, provider, args) =>
                ExtFuncs.Matches(CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringOptional(context, args[1]),
                    CoreFuncs.CastToStringExactOne(context, args[2])));
            Add(XmlReservedNs.NsXQueryFunc, "replace", 3, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.Replace(CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringOptional(context, args[1]),
                    CoreFuncs.CastToStringExactOne(context, args[2])));
            Add(XmlReservedNs.NsXQueryFunc, "replace", 4, XPath2ResultType.String, (context, provider, args) =>
                ExtFuncs.Replace(CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringOptional(context, args[1]),
                    CoreFuncs.CastToStringExactOne(context, args[2]),
                    (string)CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[3]), new SequenceType(XmlTypeCode.String))));
            Add(XmlReservedNs.NsXQueryFunc, "tokenize", 2, XPath2ResultType.NodeSet, (context, provider, args) =>
                ExtFuncs.Tokenize(CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringOptional(context, args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "tokenize", 3, XPath2ResultType.NodeSet, (context, provider, args) =>
                ExtFuncs.Tokenize(CoreFuncs.CastToStringOptional(context, args[0]),
                    CoreFuncs.CastToStringOptional(context, args[1]),
                    CoreFuncs.CastToStringExactOne(context, args[2])));
            Add(XmlReservedNs.NsXQueryFunc, "years-from-duration", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.YearsFromDuration(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.Duration, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "months-from-duration", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.MonthsFromDuration(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.Duration, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "days-from-duration", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.DaysFromDuration(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.Duration, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "hours-from-duration", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.HoursFromDuration(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.Duration, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "minutes-from-duration", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.MinutesFromDuration(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.Duration, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "seconds-from-duration", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.SecondsFromDuration(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.Duration, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "year-from-dateTime", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.YearFromDateTime(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.DateTime, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "month-from-dateTime", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.MonthFromDateTime(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.DateTime, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "day-from-dateTime", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.DayFromDateTime(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.DateTime, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "hours-from-dateTime", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.HoursFromDateTime(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.DateTime, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "minutes-from-dateTime", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.MinutesFromDateTime(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.DateTime, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "seconds-from-dateTime", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.SecondsFromDateTime(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.DateTime, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "timezone-from-dateTime", 1, XPath2ResultType.Duration, (context, provider, args) =>
               ExtFuncs.TimezoneFromDateTime(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.DateTime, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "year-from-date", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.YearFromDate(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.Date, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "month-from-date", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.MonthFromDate(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.Date, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "day-from-date", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.DayFromDate(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.Date, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "timezone-from-date", 1, XPath2ResultType.Duration, (context, provider, args) =>
                ExtFuncs.TimezoneFromDate(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.Date, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "hours-from-time", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.HoursFromTime(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.Time, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "minutes-from-time", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.MinutesFromTime(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.Time, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "seconds-from-time", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.SecondsFromTime(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.Time, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "timezone-from-time", 1, XPath2ResultType.Duration, (context, provider, args) =>
                ExtFuncs.TimezoneFromTime(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.Time, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "adjust-dateTime-to-timezone", 2, XPath2ResultType.DateTime, (context, provider, args) =>
                ExtFuncs.AdjustDateTimeToTimezone(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.DateTime, XmlTypeCardinality.ZeroOrOne)),
                    CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[1]),
                        new SequenceType(XmlTypeCode.DayTimeDuration, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "adjust-dateTime-to-timezone", 1, XPath2ResultType.DateTime, (context, provider, args) =>
                ExtFuncs.AdjustDateTimeToTimezone(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.DateTime, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "adjust-date-to-timezone", 2, XPath2ResultType.DateTime, (context, provider, args) =>
                ExtFuncs.AdjustDateToTimezone(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.Date, XmlTypeCardinality.ZeroOrOne)),
                    CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[1]), new SequenceType(XmlTypeCode.DayTimeDuration,
                         XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "adjust-date-to-timezone", 1, XPath2ResultType.DateTime, (context, provider, args) =>
                ExtFuncs.AdjustDateToTimezone(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.Date, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "adjust-time-to-timezone", 2, XPath2ResultType.DateTime, (context, provider, args) =>
                ExtFuncs.AdjustTimeToTimezone(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.Time, XmlTypeCardinality.ZeroOrOne)),
                    CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[1]),
                         new SequenceType(XmlTypeCode.DayTimeDuration, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "adjust-time-to-timezone", 1, XPath2ResultType.DateTime, (context, provider, args) =>
                ExtFuncs.AdjustTimeToTimezone(CoreFuncs.CastArg(context, CoreFuncs.Atomize(args[0]),
                    new SequenceType(XmlTypeCode.Time, XmlTypeCardinality.ZeroOrOne))));
            Add(XmlReservedNs.NsXQueryFunc, "abs", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.GetAbs(CoreFuncs.Atomize(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "ceiling", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.GetCeiling(CoreFuncs.Atomize(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "floor", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.GetFloor(CoreFuncs.Atomize(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "round", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.GetRound(CoreFuncs.Atomize(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "round-half-to-even", 2, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.GetRoundHalfToEven(CoreFuncs.Atomize(args[0]), CoreFuncs.Atomize(args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "round-half-to-even", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.GetRoundHalfToEven(CoreFuncs.Atomize(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "compare", 2, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.Compare(CoreFuncs.CastToStringOptional(context, args[0]), CoreFuncs.CastToStringOptional(context, args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "compare", 3, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.Compare(context, CoreFuncs.CastToStringOptional(context, args[0]), CoreFuncs.CastToStringOptional(context, args[1]),
                    CoreFuncs.CastToStringExactOne(context, args[2], false)));
            Add(XmlReservedNs.NsXQueryFunc, "codepoint-equal", 2, XPath2ResultType.Boolean, (context, provider, args) =>
                ExtFuncs.CodepointEqual(CoreFuncs.CastToStringOptional(context, args[0]), CoreFuncs.CastToStringOptional(context, args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "empty", 1, XPath2ResultType.Boolean, (context, provider, args) =>
                ExtFuncs.EmptySequence(XPath2NodeIterator.Create(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "exists", 1, XPath2ResultType.Boolean, (context, provider, args) =>
                ExtFuncs.ExistsSequence(XPath2NodeIterator.Create(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "reverse", 1, XPath2ResultType.NodeSet, (context, provider, args) =>
                ExtFuncs.ReverseSequence(XPath2NodeIterator.Create(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "index-of", 3, XPath2ResultType.NodeSet, (context, provider, args) =>
                ExtFuncs.IndexOfSequence(context, XPath2NodeIterator.Create(args[0]), CoreFuncs.Atomize(args[1]),
                    CoreFuncs.CastToStringExactOne(context, args[2], false)));
            Add(XmlReservedNs.NsXQueryFunc, "index-of", 2, XPath2ResultType.NodeSet, (context, provider, args) =>
                ExtFuncs.IndexOfSequence(XPath2NodeIterator.Create(args[0]), CoreFuncs.Atomize(args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "remove", 2, XPath2ResultType.NodeSet, (context, provider, args) =>
                ExtFuncs.Remove(XPath2NodeIterator.Create(args[0]), CoreFuncs.CastToInt(context, args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "insert-before", 3, XPath2ResultType.NodeSet, (context, provider, args) =>
                ExtFuncs.InsertBefore(XPath2NodeIterator.Create(args[0]), CoreFuncs.CastToInt(context, args[1]),
                    XPath2NodeIterator.Create(args[2])));
            Add(XmlReservedNs.NsXQueryFunc, "subsequence", 3, XPath2ResultType.NodeSet, (context, provider, args) =>
                ExtFuncs.Subsequence(XPath2NodeIterator.Create(args[0]), CoreFuncs.Number(context, args[1]),
                    CoreFuncs.Number(context, args[2])));
            Add(XmlReservedNs.NsXQueryFunc, "subsequence", 2, XPath2ResultType.NodeSet, (context, provider, args) =>
                ExtFuncs.Subsequence(XPath2NodeIterator.Create(args[0]), CoreFuncs.Number(context, args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "unordered", 1, XPath2ResultType.NodeSet, (context, provider, args) =>
                ExtFuncs.Unordered(XPath2NodeIterator.Create(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "zero-or-one", 1, XPath2ResultType.Any, (context, provider, args) =>
                ExtFuncs.ZeroOrOne(XPath2NodeIterator.Create(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "one-or-more", 1, XPath2ResultType.NodeSet, (context, provider, args) =>
                ExtFuncs.OneOrMore(XPath2NodeIterator.Create(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "exactly-one", 1, XPath2ResultType.Any, (context, provider, args) =>
                ExtFuncs.ExactlyOne(XPath2NodeIterator.Create(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "distinct-values", 2, XPath2ResultType.NodeSet, (context, provider, args) =>
                ExtFuncs.DistinctValues(context, XPath2NodeIterator.Create(args[0]),
                    CoreFuncs.CastToStringExactOne(context, args[1], false)));
            Add(XmlReservedNs.NsXQueryFunc, "distinct-values", 1, XPath2ResultType.NodeSet, (context, provider, args) =>
                ExtFuncs.DistinctValues(XPath2NodeIterator.Create(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "deep-equal", 3, XPath2ResultType.Boolean, (context, provider, args) =>
                ExtFuncs.DeepEqual(context, XPath2NodeIterator.Create(args[0]), XPath2NodeIterator.Create(args[1]),
                CoreFuncs.CastToStringExactOne(context, args[2])));
            Add(XmlReservedNs.NsXQueryFunc, "deep-equal", 2, XPath2ResultType.Boolean, (context, provider, args) =>
                ExtFuncs.DeepEqual(XPath2NodeIterator.Create(args[0]), XPath2NodeIterator.Create(args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "count", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.CountValues(XPath2NodeIterator.Create(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "max", 1, XPath2ResultType.Any, (context, provider, args) =>
                ExtFuncs.MaxValue(context, XPath2NodeIterator.Create(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "max", 2, XPath2ResultType.Any, (context, provider, args) =>
                ExtFuncs.MaxValue(context, XPath2NodeIterator.Create(args[0]),
                CoreFuncs.CastToStringExactOne(context, args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "min", 1, XPath2ResultType.Any, (context, provider, args) =>
                ExtFuncs.MinValue(context, XPath2NodeIterator.Create(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "min", 2, XPath2ResultType.Any, (context, provider, args) =>
                ExtFuncs.MaxValue(context, XPath2NodeIterator.Create(args[0]),
                CoreFuncs.CastToStringExactOne(context, args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "sum", 2, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.SumValue(context, XPath2NodeIterator.Create(args[0]), CoreFuncs.Atomize(args[1])));
            Add(XmlReservedNs.NsXQueryFunc, "sum", 1, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.SumValue(context, XPath2NodeIterator.Create(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "avg", 1, XPath2ResultType.Any, (context, provider, args) =>
                ExtFuncs.AvgValue(context, XPath2NodeIterator.Create(args[0])));
            Add(XmlReservedNs.NsXQueryFunc, "position", 0, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.CurrentPosition(provider));
            Add(XmlReservedNs.NsXQueryFunc, "last", 0, XPath2ResultType.Number, (context, provider, args) =>
                ExtFuncs.LastPosition(provider));
            Add(XmlReservedNs.NsXQueryFunc, "root", 1, XPath2ResultType.Navigator, (context, provider, args) =>
                CoreFuncs.GetRoot(CoreFuncs.NodeValue(args[0], false)));
            Add(XmlReservedNs.NsXQueryFunc, "boolean", 1, XPath2ResultType.Boolean, (context, provider, args) =>
                CoreFuncs.BooleanValue(args[0]));
            Add(XmlReservedNs.NsXQueryFunc, "true", 0, XPath2ResultType.Boolean, (context, provider, args) => CoreFuncs.True);
            Add(XmlReservedNs.NsXQueryFunc, "false", 0, XPath2ResultType.Boolean, (context, provider, args) => CoreFuncs.False);
            Add(XmlReservedNs.NsXQueryFunc, "not", 1, XPath2ResultType.Boolean, (context, provider, args) =>
                CoreFuncs.Not(args[0]));
            Add(XmlReservedNs.NsXQueryFunc, "string", 0, XPath2ResultType.String, (context, provider, args) =>
                CoreFuncs.StringValue(context, provider));
            Add(XmlReservedNs.NsXQueryFunc, "string", 1, XPath2ResultType.String, (context, provider, args) =>
                CoreFuncs.StringValue(context, args[0]));
            Add(XmlReservedNs.NsXQueryFunc, "number", 0, XPath2ResultType.Number, (context, provider, args) =>
                CoreFuncs.Number(context, provider));
            Add(XmlReservedNs.NsXQueryFunc, "number", 1, XPath2ResultType.Number, (context, provider, args) =>
                CoreFuncs.Number(context, args[0]));
        }

        public XPathFunctionDef Bind(string name, string ns, int arity)
        {
            XPathFunctionDef res;
            if (!_funcTable.TryGetValue(new FunctionDesc(name, ns, arity), out res))
            {
                // Just check if XPath 2010 function namespace is used, and if yes, try older version.
                if (ns == XmlReservedNs.NsXPathFunc)
                    if (!_funcTable.TryGetValue(new FunctionDesc(name, XmlReservedNs.NsXQueryFunc, arity), out res))
                        return null;
            }
            return res;
        }

        public void Add(string ns, string name, XPath2ResultType resultType, XPathFunctionDelegate action, bool overwriteExistingEntry = true)
        {
            Add(ns, name, -1, resultType, action, overwriteExistingEntry);
        }

        public void Add(string ns, string name, int arity, XPath2ResultType resultType, XPathFunctionDelegate action, bool overwriteExistingEntry = true)
        {
            var key = new FunctionDesc(name, ns, arity);

            if (_funcTable.ContainsKey(key) && overwriteExistingEntry)
                _funcTable[key] = new XPathFunctionDef(name, action, resultType);
            else
                _funcTable.Add(key, new XPathFunctionDef(name, action, resultType));
        }

        private static readonly object syncRoot = new object();
        private static volatile FunctionTable _inst;

        public static FunctionTable Inst
        {
            get
            {
                if (_inst == null)
                {
                    lock (syncRoot)
                        if (_inst == null)
                            _inst = new FunctionTable();
                }
                return _inst;
            }
        }

        private sealed class FunctionDesc
        {
            public readonly string Name;
            public readonly string Ns;
            public readonly int Arity;

            public FunctionDesc(string name, string ns, int arity)
            {
                Name = name;
                Ns = ns;
                Arity = arity;
            }

            public override bool Equals(object obj)
            {
                FunctionDesc other = obj as FunctionDesc;
                if (other != null)
                    return Name == other.Name && Ns == other.Ns && (Arity == other.Arity || Arity == -1 || other.Arity == -1);

                return false;
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }
        }
    }
}