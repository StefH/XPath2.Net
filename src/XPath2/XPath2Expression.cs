// Microsoft Public License (Ms-PL)
// See the file License.rtf or License.txt for the license details.

// Copyright (c) 2011-2014, Semyon A. Chertkov (semyonc@gmail.com)
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Wmhelp.XPath2.AST;
using Wmhelp.XPath2.Proxy;

namespace Wmhelp.XPath2
{
    public class XPath2Expression
    {
        private readonly string expr;
        private readonly XPath2Context context;
        private XPath2ResultType? resultType;
        /// <summary>
        /// This property is used by XPath.Net internally. It isn't intended for use in application code.
        /// </summary>
        public AbstractNode ExpressionTree { get; }

        private XPath2Expression(string expr, AbstractNode exprTree, XPath2Context context)
        {
            this.expr = expr;
            this.ExpressionTree = exprTree;
            this.context = context;
        }

        public XPath2Expression Clone()
        {
            return new XPath2Expression(expr, ExpressionTree, context);
        }

        private IEnumerable<XPathItem> CreateIterator(IEnumerable<XNode> en)
        {
            foreach (XNode node in en)
                yield return node.CreateNavigator();
        }

        private IEnumerable<XPathItem> CreateIterator(IEnumerable<Object> en)
        {
            foreach (object item in en)
                yield return new XPath2Item(item);
        }

        private object PrepareValue(object value)
        {
            if (value == null)
                return Undefined.Value;
            XmlNode xmlNode = value as XmlNode;
            if (xmlNode != null)
                return xmlNode.CreateNavigator();
            XNode xnode = value as XNode;
            if (xnode != null)
                return xnode.CreateNavigator();
            XPath2NodeIterator iter = value as XPath2NodeIterator;
            if (iter != null)
                return iter;
            IEnumerable<XNode> en = value as IEnumerable<XNode>;
            if (en != null)
                return new NodeIterator(CreateIterator(en));
            IEnumerable<Object> eno = value as IEnumerable<Object>;
            if (eno != null)
                return new NodeIterator(CreateIterator(eno));
            return value;
        }

        public static XPath2Expression Compile(string xpath)
        {
            return Compile(xpath, null);
        }

        public static object Evaluate(string xpath2, object arg)
        {
            return Evaluate(xpath2, null, arg);
        }

        public static object Evaluate(string xpath2, IXmlNamespaceResolver nsResolver, object arg)
        {
            return Compile(xpath2, nsResolver).EvaluateWithProperties(null, arg);
        }

        [Obsolete("Please use Evaluate")]
        public static object Evalute(string xpath2, IXmlNamespaceResolver nsResolver, IDictionary<XmlQualifiedName, object> param)
        {
            return Evaluate(xpath2, nsResolver, param);
        }

        public static object Evaluate(string xpath2, IXmlNamespaceResolver nsResolver, IDictionary<XmlQualifiedName, object> param)
        {
            return Compile(xpath2, nsResolver).Evaluate(null, param);
        }

        public static IEnumerable<T> Select<T>(string xpath, object arg)
            where T : XObject
        {
            return Select<T>(xpath, null, arg);
        }

        public static IEnumerable<T> Select<T>(string xpath, IXmlNamespaceResolver resolver, object arg)
            where T : XObject
        {
            XPath2NodeIterator iter = XPath2NodeIterator.Create(Compile(xpath, resolver).EvaluateWithProperties(null, arg));
            foreach (XPathItem item in iter)
                if (item.IsNode)
                {
                    XPathNavigator curr = (XPathNavigator)item;
                    XObject o = (XObject)curr.UnderlyingObject;
                    if (!(o is T))
                        throw new InvalidOperationException(String.Format("Unexpected evaluation {0}", o.GetType()));
                    yield return (T)o;
                }
                else
                    throw new InvalidOperationException(String.Format("Unexpected evaluation {0}", item.TypedValue.GetType()));
        }

        public static IEnumerable<Object> SelectValues(string xpath, object arg)
        {
            return SelectValues(xpath, null, arg);
        }

        public static IEnumerable<Object> SelectValues(string xpath, IXmlNamespaceResolver resolver, object arg)
        {
            XPath2NodeIterator iter = XPath2NodeIterator.Create(Compile(xpath, resolver).EvaluateWithProperties(null, arg));
            while (iter.MoveNext())
                yield return iter.Current.GetTypedValue();
        }

        public static IEnumerable<Object> SelectValues(string xpath, IDictionary<XmlQualifiedName, object> param)
        {
            return SelectValues(xpath, null, param);
        }

        public static IEnumerable<Object> SelectValues(string xpath, IXmlNamespaceResolver resolver, IDictionary<XmlQualifiedName, object> vars)
        {
            XPath2NodeIterator iter = XPath2NodeIterator.Create(Compile(xpath, resolver).Evaluate(null, vars));
            while (iter.MoveNext())
                yield return iter.Current.GetTypedValue();
        }

        public static XPath2Expression Compile(string xpath, IXmlNamespaceResolver resolver)
        {
            if (xpath == null)
                throw new ArgumentNullException(nameof(xpath));
            if (xpath == "")
                throw new XPath2Exception("", "Empty xpath expression");
            XPath2Context context = new XPath2Context(resolver);
            Tokenizer tokenizer = new Tokenizer(xpath);
            YYParser parser = new YYParser(context);
            AbstractNode exprTree = (AbstractNode)parser.yyparseSafe(tokenizer);
            return new XPath2Expression(xpath, exprTree, context);
        }

        public object Evaluate()
        {
            return Evaluate(null, null);
        }

        private object[] BindExpr(IDictionary<XmlQualifiedName, object> vars)
        {
            XPath2RunningContext runningContext = new XPath2RunningContext();
            NameBinder.ReferenceLink[] variables = null;
            object[] variableValues = null;
            if (vars != null)
            {
                variables = new NameBinder.ReferenceLink[vars.Count];
                variableValues = new object[vars.Count];
                var array = new KeyValuePair<XmlQualifiedName, object>[vars.Count];
                vars.CopyTo(array, 0);
                for (int k = 0; k < array.Length; k++)
                {
                    variables[k] = runningContext.NameBinder.PushVar(array[k].Key);
                    variableValues[k] = array[k].Value;
                }
            }
            context.RunningContext = runningContext;
            ExpressionTree.Bind();
            object[] dataPool = new object[runningContext.NameBinder.Length];
            if (vars != null)
                for (int k = 0; k < variables.Length; k++)
                    variables[k].Set(dataPool, PrepareValue(variableValues[k]));
            return dataPool;
        }

        public object Evaluate(IContextProvider provider, IDictionary<XmlQualifiedName, object> vars)
        {
            object res = ExpressionTree.Execute(provider, BindExpr(vars));
            if (res is XPathItem)
            {
                XPathItem item = (XPathItem)res;
                if (!item.IsNode)
                    res = item.TypedValue;
            }
            resultType = CoreFuncs.GetXPath2ResultType(res);
            ValueProxy proxy = res as ValueProxy;
            if (proxy != null)
                return proxy.Value;
            return res;
        }

        public object EvaluateWithProperties(IContextProvider provider, object props)
        {
            IDictionary<XmlQualifiedName, object> vars = null;
            if (props != null)
            {
                Type type = props.GetType();
                if (type.IsArray)
                    throw new ArgumentException("props");
                PropertyInfo[] propsInfo = type.GetProperties();
                vars = new Dictionary<XmlQualifiedName, object>(propsInfo.Length);
                for (int k = 0; k < propsInfo.Length; k++)
                {
                    PropertyInfo property = propsInfo[k];
                    vars.Add(new XmlQualifiedName(property.Name), property.GetValue(props, null));
                }
            }
            return Evaluate(provider, vars);
        }

        public String Expression => expr;

        public XPath2ResultType GetResultType(IDictionary<XmlQualifiedName, object> vars)
        {
            if (!resultType.HasValue)
                resultType = ExpressionTree.GetReturnType(BindExpr(vars));
            return resultType.Value;
        }
    }
}