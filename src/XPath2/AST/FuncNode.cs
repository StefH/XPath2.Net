// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System;
using System.Collections.Generic;
using Wmhelp.XPath2.MS;
using Wmhelp.XPath2.Properties;
using Wmhelp.XPath2.Proxy;

namespace Wmhelp.XPath2.AST
{
    /// <summary>
    /// This class is used by XPath.Net internally. It isn't intended for use in application code.
    /// </summary>
    public sealed class FuncNode : AbstractNode
    {
        private readonly String _name;
        private readonly String _ns;
        private readonly XPathFunctionDef _func;

        public FuncNode(XPath2Context context, string name, string ns)
            : base(context)
        {
            _func = FunctionTable.Inst.Bind(name, ns, 0);
            if (_func == null)
                throw new XPath2Exception("XPST0017", Resources.XPST0017, name, 0, ns);
            _name = name;
            _ns = ns;
        }

        public FuncNode(XPath2Context context, string name, string ns, List<object> nodes)
            : base(context)
        {
            _func = FunctionTable.Inst.Bind(name, ns, nodes.Count);
            if (_func == null)
                throw new XPath2Exception("XPST0017", Resources.XPST0017, name, nodes.Count, ns);
            _name = name;
            _ns = ns;
            AddRange(nodes);
        }

        public override void Bind()
        {
            if (Count > 0)
            {
                PathExprNode pathExpr = this[0] as PathExprNode;
                if (pathExpr != null &&
                    s_aggregates.Contains(_name) && _ns == XmlReservedNs.NsXQueryFunc)
                    pathExpr.Unordered = true;
            }
            base.Bind();
        }

        public override bool IsContextSensitive()
        {
            return (Count == 0 && s_contextDs.Contains(_name) &&
                    _ns == XmlReservedNs.NsXQueryFunc) || base.IsContextSensitive();
        }

        public override object Execute(IContextProvider provider, object[] dataPool)
        {
            object[] args = new object[Count];
            for (int k = 0; k < Count; k++)
                args[k] = ValueProxy.Unwrap(this[k].Execute(provider, dataPool));
            return _func.Invoke(Context, provider, args);
        }

        public override XPath2ResultType GetReturnType(object[] dataPool)
        {
            if (_func.ResultType == XPath2ResultType.Any && Count > 0)
            {
                XPath2ResultType resType = this[0].GetItemType(dataPool);
                if (resType == XPath2ResultType.NodeSet)
                    return XPath2ResultType.Any;
                return resType;
            }
            return _func.ResultType;
        }

        internal override XPath2ResultType GetItemType(object[] dataPool)
        {
            if (_func.Name == "string-to-codepoints")
                return XPath2ResultType.Number;
            return base.GetItemType(dataPool);
        }

        private static readonly HashSet<String> s_aggregates;

        private static readonly String[] s_names = new String[]
        {
            "sum", "count", "avg", "min", "max",
            "distinct-values", "empty", "exits"
        };

        private static readonly HashSet<String> s_contextDs;

        private static readonly String[] s_names2 = new String[]
        {
            "name", "local-name", "namespace-uri",
            "base-uri", "position", "last", "string-length", "normalize-space", "number"
        };

        static FuncNode()
        {
            s_aggregates = new HashSet<string>();
            foreach (string name in s_names)
                s_aggregates.Add(name);
            s_contextDs = new HashSet<string>();
            foreach (string name in s_names2)
                s_contextDs.Add(name);
        }
    }
}