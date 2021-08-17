using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using SimpleTreeNode;
using Wmhelp.XPath2;

namespace XPath2.TestRunner
{
    public class XQTSRunner
    {
        public const string XQTSNamespace = "http://www.w3.org/2005/02/query-test-XQTSCatalog";

        private string _basePath;
        private string _queryOffsetPath;
        private string _resultOffsetPath;
        private string _queryFileExtension;

        private readonly TextWriter _out;
        private readonly bool _logErrors;

        private readonly NameTable _nameTable = new NameTable();
        private XmlNamespaceManager _nsmgr;
        private XmlDocument _catalog;
        private DataTable _testTab;
        private Dictionary<string, string> _sources;
        private Dictionary<string, string> _module;
        private Dictionary<string, string[]> _collection;
        private Dictionary<string, string[]> _schema;
        private HashSet<string> _ignoredTests;

        private int _total;
        private int _passed;

        private static readonly string[] testsToIgnore =
        {
            "nametest-1", "nametest-2", "nametest-5", "nametest-6",
            "nametest-7", "nametest-8", "nametest-9", "nametest-10",
            "nametest-11", "nametest-12", "nametest-13", "nametest-14",
            "nametest-15", "nametest-16", "nametest-17", "nametest-18",
            "CastAs660", "CastAs661", "CastAs662", "CastAs663",
            "CastAs664", "CastAs665", "CastAs666", "CastAs667",
            "CastAs668", "CastAs669", "CastAs671", "CastableAs648",
            "fn-trace-2", "fn-trace-9",
            "NodeTesthc-1", "NodeTesthc-2", "NodeTesthc-3", "NodeTesthc-4",
            "NodeTesthc-5", "NodeTesthc-6", "NodeTesthc-7", "NodeTesthc-8",
            "fn-max-3", "fn-min-3",
            "defaultnamespacedeclerr-1", "defaultnamespacedeclerr-2",
            "fn-document-uri-12", "fn-document-uri-15", "fn-document-uri-16",
            "fn-document-uri-17", "fn-document-uri-18", "fn-document-uri-19",
            "fn-prefix-from-qname-8", "boundaryspacedeclerr-1",
            "fn-resolve-uri-2",
            "ancestor-21", "ancestorself-21", "following-21",
            "followingsibling-21", "preceding-21", "preceding-sibling-21"
        };

        public XQTSRunner(TextWriter writer, bool logErrors = false)
        {
            _out = writer;
            _logErrors = logErrors;
        }

        public TestRunResult Run(string fileName)
        {
            _nsmgr = new XmlNamespaceManager(_nameTable);
            _nsmgr.AddNamespace("ts", XQTSNamespace);

            _testTab = new DataTable();
            _testTab.Columns.Add("Select", typeof(bool));
            _testTab.Columns.Add("Name", typeof(string));
            _testTab.Columns.Add("FilePath", typeof(string));
            _testTab.Columns.Add("scenario", typeof(string));
            _testTab.Columns.Add("Creator", typeof(string));
            _testTab.Columns.Add("Node", typeof(object));
            _testTab.Columns.Add("Description", typeof(string));

            _ignoredTests = new HashSet<string>(testsToIgnore);
            _catalog = new XmlDocument(_nameTable);

            var schemaSet = new XmlSchemaSet();
            var settings = new XmlReaderSettings
            {
                Schemas = schemaSet,
                DtdProcessing = DtdProcessing.Ignore
            };
            var resolver = new XmlUrlResolver
            {
                Credentials = CredentialCache.DefaultCredentials
            };

            settings.XmlResolver = resolver;
            settings.NameTable = _nameTable;
            settings.ValidationFlags = XmlSchemaValidationFlags.ProcessSchemaLocation | XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationType = ValidationType.Schema;
            using (XmlReader reader = XmlReader.Create(fileName, settings))
            {
                _catalog.Load(reader);
                reader.Close();
            }

            if (!(_catalog.DocumentElement.NamespaceURI == XQTSNamespace && _catalog.DocumentElement.LocalName == "test-suite"))
            {
                throw new ArgumentException("Input file is not XQTS catalog.");
            }

            if (_catalog.DocumentElement.GetAttribute("version") != "1.0.2")
            {
                throw new NotSupportedException("Only version 1.0.2 is XQTS supported.");
            }

            _basePath = Path.GetDirectoryName(fileName);
            //_sourceOffsetPath = _catalog.DocumentElement.GetAttribute("SourceOffsetPath");
            _queryOffsetPath = _catalog.DocumentElement.GetAttribute("XQueryQueryOffsetPath");
            _resultOffsetPath = _catalog.DocumentElement.GetAttribute("ResultOffsetPath");
            _queryFileExtension = _catalog.DocumentElement.GetAttribute("XQueryFileExtension");

            _sources = new Dictionary<string, string>();
            _module = new Dictionary<string, string>();
            _collection = new Dictionary<string, string[]>();
            _schema = new Dictionary<string, string[]>();

            foreach (XmlElement node in _catalog.SelectNodes("/ts:test-suite/ts:sources/ts:schema", _nsmgr))
            {
                string id = node.GetAttribute("ID");
                string targetNs = node.GetAttribute("uri");
                string schemaFileName = Path.Combine(_basePath, node.GetAttribute("FileName").Replace('/', '\\'));
                if (!File.Exists(schemaFileName))
                {
                    _out.WriteLine("Schema file {0} does not exists", schemaFileName);
                }

                _schema.Add(id, new[] { targetNs, schemaFileName });
            }

            foreach (XmlElement node in _catalog.SelectNodes("/ts:test-suite/ts:sources/ts:source", _nsmgr))
            {
                string id = node.GetAttribute("ID");
                string sourceFileName = Path.Combine(_basePath, node.GetAttribute("FileName").Replace('/', '\\'));
                if (!File.Exists(sourceFileName))
                {
                    _out.WriteLine("Source file {0} does not exists", sourceFileName);
                }
                _sources.Add(id, sourceFileName);
            }

            foreach (XmlElement node in _catalog.SelectNodes("/ts:test-suite/ts:sources/ts:collection", _nsmgr))
            {
                string id = node.GetAttribute("ID");
                XmlNodeList nodes = node.SelectNodes("ts:input-document", _nsmgr);
                string[] items = new string[nodes.Count];
                int k = 0;
                foreach (XmlElement curr in nodes)
                {
                    if (!_sources.ContainsKey(curr.InnerText))
                    {
                        _out.WriteLine("Referenced source ID {0} in collection {1} not exists", curr.InnerText, id);
                    }
                    items[k++] = curr.InnerText;
                }
                _collection.Add(id, items);
            }

            foreach (XmlElement node in _catalog.SelectNodes("/ts:test-suite/ts:sources/ts:module", _nsmgr))
            {
                string id = node.GetAttribute("ID");
                string moduleFileName = Path.Combine(_basePath, node.GetAttribute("FileName").Replace('/', '\\') + _queryFileExtension);
                if (!File.Exists(moduleFileName))
                {
                    _out.WriteLine("Module file {0} does not exists", moduleFileName);
                }
                _module.Add(id, moduleFileName);
            }


            var rootNode = new TreeNode<TreeNodeValue>(new TreeNodeValue { Text = "Test-suite" });
            ReadTestTree(_catalog.DocumentElement, rootNode);
            _out.Write(rootNode);

            SelectAll();
            // SelectSupported();            

            return RunParallel();
        }

        private void ReadTestTree(XmlNode node, TreeNode<TreeNodeValue> parentNode)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.LocalName == "test-group" && child.NamespaceURI == XQTSNamespace)
                {
                    var elem = (XmlElement)child;

                    var childNode = new TreeNode<TreeNodeValue>(new TreeNodeValue
                    {
                        Text = elem.GetAttribute("name"),
                        Tag = child
                    });

                    ReadTestTree(child, childNode);
                    parentNode.ChildNodes.Add(childNode);
                }
            }
        }

        private TestRunResult RunParallel()
        {
            var rows = _testTab.Select("");

            var sw = new Stopwatch();
            sw.Start();
            Parallel.ForEach(rows, dr =>
            {
                if ((bool)dr[0])
                {
                    XmlElement curr = (XmlElement)dr[5];
                    var tw = new StringWriter();
                    if (PerformTest(tw, curr))
                    {
                        // tw.WriteLine("Passed.");
                        //Interlocked.Increment(ref _passed);
                        _passed++;
                    }
                    else
                    {
                        tw.WriteLine("Failed.");
                        // _out.Write(tw.ToString());
                    }
                    //Interlocked.Increment(ref _total);
                    _total++;
                }
            });
            sw.Stop();
            _out.WriteLine("Elapsed {0}", sw.Elapsed);

            // It conforms for 12954 from 15133 (85.60%) regarding the test-set
            decimal total = _total;
            decimal passed = _passed;
            decimal percentage = Math.Round(passed / total * 100, 2);

            // It conforms for 12954 from 15133 (85.60%) regarding the test-set
            _out.WriteLine("{0} executed, {1} ({2}%) succeeded.", total, passed, percentage);
            
            return new TestRunResult
            {
                Total = _total,
                Passed = _passed,
                Percentage = percentage
            };
        }

        private bool PerformTest(TextWriter tw, XmlElement testCase)
        {
            try
            {
                PreparedXPath preparedXPath;
                XPath2ResultType expectedType;
                try
                {
                    preparedXPath = PrepareXPath(tw, testCase);
                    expectedType = preparedXPath.GetResultType();
                }
                catch (XPath2Exception)
                {
                    if (testCase.GetAttribute("scenario") == "parse-error" ||
                        testCase.GetAttribute("scenario") == "runtime-error" ||
                        testCase.SelectSingleNode("ts:expected-error", _nsmgr) != null)
                        return true;
                    throw;
                }

                object res;
                try
                {
                    res = preparedXPath.Evaluate();
                    if (res != Undefined.Value && preparedXPath.GetResultType() != expectedType)
                    {
                        if (_logErrors)
                        {
                            _out.Write("{0}: ", testCase.GetAttribute("name"));
                            _out.WriteLine("Expected type '{0}' differs the actual type '{1}'", expectedType, preparedXPath.GetResultType());
                        }
                    }
                }
                catch (XPath2Exception)
                {
                    if (testCase.GetAttribute("scenario") == "parse-error" ||
                        testCase.GetAttribute("scenario") == "runtime-error" ||
                        testCase.SelectSingleNode("ts:expected-error", _nsmgr) != null)
                    {
                        return true;
                    }

                    throw;
                }
                try
                {
                    if (testCase.GetAttribute("scenario") == "standard")
                    {
                        foreach (XmlElement outputFile in testCase.SelectNodes("ts:output-file", _nsmgr))
                        {
                            string compare = outputFile.GetAttribute("compare");
                            if (compare == "Text" || compare == "Fragment")
                            {
                                if (CompareResult(testCase, GetResultPath(testCase, outputFile.InnerText), res, false))
                                {
                                    return true;
                                }
                            }
                            else if (compare == "XML")
                            {
                                if (CompareResult(testCase, GetResultPath(testCase, outputFile.InnerText), res, true))
                                {
                                    return true;
                                }
                            }
                            else if (compare == "Inspect")
                            {
                                if (_logErrors)
                                {
                                    _out.WriteLine("{0}: Inspection needed.", testCase.GetAttribute("name"));
                                }
                                return true;
                            }
                            else if (compare == "Ignore")
                            {
                                continue;
                            }
                            else
                            {
                                throw new InvalidOperationException();
                            }
                        }

                        return false;
                    }
                    else if (testCase.GetAttribute("scenario") == "runtime-error")
                    {
                        if (res is XPath2NodeIterator)
                        {
                            var iter = (XPath2NodeIterator)res;
                            while (iter.MoveNext())
                                ;
                        }

                        return false;
                    }

                    return true;
                }
                catch (XPath2Exception)
                {
                    if (testCase.GetAttribute("scenario") == "runtime-error" ||
                        testCase.SelectSingleNode("ts:expected-error", _nsmgr) != null)
                    {
                        return true;
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (_logErrors)
                {
                    _out.WriteLine();
                    _out.WriteLine(ex);
                }
                return false;
            }
        }

        private PreparedXPath PrepareXPath(TextWriter tw, XmlElement node)
        {
            string fileName = GetFilePath(node);
            tw.Write("{0}: ", node.GetAttribute("name"));
            if (!File.Exists(fileName))
            {
                _out.WriteLine("File {0} not exists.", fileName);
                throw new ArgumentException();
            }
            PreparedXPath res;
            res.provider = null;
            TextReader textReader = new StreamReader(fileName, true);
            string xpath = PrepareQueryText(textReader.ReadToEnd());
            textReader.Close();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(_nameTable);
            nsMgr.AddNamespace("foo", "http://example.org");
            nsMgr.AddNamespace("FOO", "http://example.org");
            nsMgr.AddNamespace("atomic", "http://www.w3.org/XQueryTest");
            Dictionary<XmlQualifiedName, object> vars = null;
            foreach (XmlNode child in node.ChildNodes)
            {
                XmlElement curr = child as XmlElement;
                if (curr == null || curr.NamespaceURI != XQTSNamespace)
                    continue;
                if (curr.LocalName == "input-file")
                {
                    if (vars == null)
                        vars = new Dictionary<XmlQualifiedName, object>();
                    string var = curr.GetAttribute("variable");
                    string id = curr.InnerText;
                    XmlDocument xmldoc = new XmlDocument(_nameTable);
                    xmldoc.Load(_sources[id]);
                    vars.Add(new XmlQualifiedName(var), xmldoc.CreateNavigator());
                }
                else if (curr.LocalName == "contextItem")
                {
                    string id = curr.InnerText;
                    XmlDocument xmldoc = new XmlDocument(_nameTable);
                    xmldoc.Load(_sources[id]);
                    res.provider = new NodeProvider(xmldoc.CreateNavigator());
                }
                else if (curr.LocalName == "input-URI")
                {
                    if (vars == null)
                        vars = new Dictionary<XmlQualifiedName, object>();
                    string var = curr.GetAttribute("variable");
                    string value = curr.InnerText;
                    string expandedUri;
                    if (!_sources.TryGetValue(value, out expandedUri))
                        expandedUri = value;
                    vars.Add(new XmlQualifiedName(var), expandedUri);
                }
            }
            res.expression = XPath2Expression.Compile(xpath, nsMgr);
            res.vars = vars;
            return res;
        }

        private bool CompareResult(XmlNode testCase, string sourceFile, object value, bool xmlCompare)
        {
            string id = ((XmlElement)testCase).GetAttribute("name");
            bool isSingle = false;
            bool isExcpt = (id == "fn-union-node-args-005") ||
                (id == "fn-union-node-args-006") || (id == "fn-union-node-args-007") ||
                (id == "fn-union-node-args-009") || (id == "fn-union-node-args-010") ||
                (id == "fn-union-node-args-011");
            if (id == "ReturnExpr010")
                xmlCompare = true;
            if (id != "CondExpr012" && id != "NodeTest006")
            {
                if (value is XPathItem)
                    isSingle = true;
                else if (value is XPath2NodeIterator)
                {
                    XPath2NodeIterator iter = (XPath2NodeIterator)value;
                    isSingle = iter.IsSingleIterator;
                }
            }
            var doc1 = new XmlDocument(_nameTable);
            if (xmlCompare)
            {
                doc1.Load(sourceFile);
            }
            else
            {
                var sb = new StringBuilder();
                sb.AppendLine("<?xml version='1.0'?>");
                sb.Append("<root>");
                TextReader textReader = new StreamReader(sourceFile, true);
                sb.Append(textReader.ReadToEnd());
                textReader.Close();
                sb.Append("</root>");
                doc1.LoadXml(sb.ToString());
            }
            MemoryStream ms = new MemoryStream();
            XmlWriter writer = new XmlTextWriter(ms, Encoding.UTF8);
            if (!(xmlCompare && isSingle || isExcpt))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement(doc1.DocumentElement.Name, "");
            }
            if (value is XPath2NodeIterator)
            {
                bool string_flag = false;
                foreach (XPathItem item in (XPath2NodeIterator)value)
                {
                    if (item.IsNode)
                    {
                        XPathNavigator nav = (XPathNavigator)item;
                        if (nav.NodeType == XPathNodeType.Attribute)
                        {
                            writer.WriteStartAttribute(nav.Prefix, nav.LocalName, nav.NamespaceURI);
                            writer.WriteString(nav.Value);
                            writer.WriteEndAttribute();
                        }
                        else
                            writer.WriteNode(nav, false);
                        string_flag = false;
                    }
                    else
                    {
                        if (string_flag)
                            writer.WriteString(" ");
                        writer.WriteString(item.Value);
                        string_flag = true;
                    }
                }
            }
            else if (value is XPathItem)
            {
                XPathItem item = (XPathItem)value;
                if (item.IsNode)
                    writer.WriteNode((XPathNavigator)item, false);
                else
                    writer.WriteString(item.Value);
            }
            else
            {
                if (value != Undefined.Value)
                    writer.WriteString(XPath2Convert.ToString(value));
            }
            if (!(xmlCompare && isSingle || isExcpt))
                writer.WriteEndElement();
            writer.Flush();
            ms.Position = 0;

            XmlDocument doc2 = new XmlDocument(_nameTable);
            doc2.Load(ms);
            writer.Close();

            TreeComparer comparer = new TreeComparer();
            comparer.IgnoreWhitespace = true;
            return comparer.DeepEqual(doc1.CreateNavigator(), doc2.CreateNavigator());
        }

        private void SelectAll()
        {
            var nodes = _catalog.SelectNodes(".//ts:test-case", _nsmgr);

            foreach (XmlElement child in nodes)
            {
                var row = _testTab.NewRow();
                row[0] = true;
                row[1] = child.GetAttribute("name");
                row[2] = child.GetAttribute("FilePath");
                row[3] = child.GetAttribute("scenario");
                row[4] = child.GetAttribute("Creator");
                row[5] = child;

                XmlElement desc = (XmlElement)child.SelectSingleNode("ts:description", _nsmgr);
                if (desc != null)
                {
                    row[6] = desc.InnerText;
                }

                _testTab.Rows.Add(row);
            }

            _out.WriteLine("{0} test case(s) loaded, {1} selected.", _testTab.Rows.Count, _testTab.Rows.Count);
        }

        private void SelectSupported()
        {
            var nodes = _catalog.SelectNodes(".//ts:test-case", _nsmgr);

            foreach (XmlElement child in nodes)
            {
                var row = _testTab.NewRow();
                row[0] = false;
                row[1] = child.GetAttribute("name");
                row[2] = child.GetAttribute("FilePath");
                row[3] = child.GetAttribute("scenario");
                row[4] = child.GetAttribute("Creator");
                row[5] = child;

                XmlElement desc = (XmlElement)child.SelectSingleNode("ts:description", _nsmgr);
                if (desc != null)
                {
                    row[6] = desc.InnerText;
                }

                _testTab.Rows.Add(row);
            }

            var hs = new HashSet<XmlNode>();
            foreach (XmlNode child in _catalog.SelectNodes(".//ts:test-group[@name='MinimalConformance']//ts:test-case", _nsmgr))
            {
                if (((XmlElement)child).GetAttribute("is-XPath2") != "false")
                {
                    hs.Add(child);
                }
            }
            foreach (XmlNode child in _catalog.SelectNodes(".//ts:test-group[@name='QuantExprWith']//ts:test-case", _nsmgr))
            {
                hs.Remove(child);
            }
            foreach (XmlNode child in _catalog.SelectNodes(".//ts:test-group[@name='XQueryComment']//ts:test-case", _nsmgr))
            {
                hs.Remove(child);
            }
            foreach (XmlNode child in _catalog.SelectNodes(".//ts:test-group[@name='Surrogates']//ts:test-case", _nsmgr))
            {
                hs.Remove(child);
            }
            foreach (XmlNode child in _catalog.SelectNodes(".//ts:test-group[@name='SeqIDFunc']//ts:test-case", _nsmgr))
            {
                hs.Remove(child);
            }
            foreach (XmlNode child in _catalog.SelectNodes(".//ts:test-group[@name='SeqCollectionFunc']//ts:test-case", _nsmgr))
            {
                hs.Remove(child);
            }
            foreach (XmlNode child in _catalog.SelectNodes(".//ts:test-group[@name='SeqDocFunc']//ts:test-case", _nsmgr))
            {
                hs.Remove(child);
            }
            foreach (XmlNode child in _catalog.SelectNodes(".//ts:test-group[@name='StaticBaseURIFunc']//ts:test-case", _nsmgr))
            {
                hs.Remove(child);
            }
            foreach (XmlNode child in _catalog.SelectNodes(".//ts:test-group[@name='FullAxis']//ts:test-case", _nsmgr))
            {
                hs.Add(child);
            }

            int sel = 0;
            foreach (DataRow row in _testTab.Rows)
            {
                var curr = (XmlElement)row[5];
                string name = curr.GetAttribute("name");
                if (hs.Contains(curr) && !_ignoredTests.Contains(name))
                {
                    row[0] = true;
                    sel++;
                }
            }

            _out.WriteLine("{0} test case(s) loaded, {1} supported selected.", _testTab.Rows.Count, sel);
        }

        private string PrepareQueryText(string text)
        {
            int index = text.IndexOf("(: Kelvin sign :)");
            if (index != -1)
                text = text.Remove(index, "(: Kelvin sign :)".Length);
            index = text.LastIndexOf(":)");
            if (index != -1)
                text = text.Substring(index + 2);
            index = EscapedIndexOf(text, '{');
            if (index != -1 && text.LastIndexOf("}") != -1)
            {
                text = text.Substring(index + 1);
                index = text.LastIndexOf("}");
                text = text.Substring(0, index);
            }
            return text.Trim();
        }

        private int EscapedIndexOf(string text, char letter)
        {
            bool isLiteral = false;
            char literal = '\0';
            for (int s = 0; s < text.Length; s++)
            {
                char ch = text[s];
                if (isLiteral)
                {
                    if (ch == literal)
                        isLiteral = false;
                }
                else
                    switch (ch)
                    {
                        case '"':
                        case '\'':
                            literal = ch;
                            isLiteral = true;
                            break;

                        default:
                            if (ch == letter)
                                return s;
                            break;
                    }
            }
            return -1;
        }

        private string GetResultPath(XmlElement node, string fileName)
        {
            return _basePath + "\\" + (_resultOffsetPath + node.GetAttribute("FilePath") + fileName).Replace('/', '\\');
        }

        private string GetFilePath(XmlElement node)
        {
            XmlNode queryName = node.SelectSingleNode("ts:query/@name", _nsmgr);
            return _basePath + "\\" + (_queryOffsetPath + node.GetAttribute("FilePath") + queryName.Value + _queryFileExtension).Replace('/', '\\');
        }
    }
}