using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using Wmhelp.XPath2;

namespace XQTSRun
{
    public partial class Form1 : Form
    {
        public const String XQTSNamespace = "http://www.w3.org/2005/02/query-test-XQTSCatalog";

        public enum TestResult
        {
            Pass,
            Fail,
            Exception
        }


        internal string _basePath;
        internal string _queryOffsetPath;
        internal string _sourceOffsetPath;
        internal string _resultOffsetPath;
        internal string _queryFileExtension;

        internal NameTable _nameTable;
        internal XmlNamespaceManager _nsmgr;
        internal XmlDocument _catalog;
        internal DataTable _testTab;
        internal Dictionary<string, string> _sources;
        internal Dictionary<string, string> _module;
        internal Dictionary<string, string[]> _collection;
        internal Dictionary<string, string[]> _schema;
        internal OutputWriter _out;
        internal string _lastFindString = "";
        internal HashSet<String> _ignoredTest;

        internal ToolStripProgressBar _progressBar;
        internal ToolStripStatusLabel _statusLabel;
        internal int _total;
        internal int _passed;
        internal int _repeatCount;

        static internal string[] s_ignoredTest =
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

        public Form1()
        {
            InitializeComponent();
            _out = new OutputWriter(richTextBox1);
            testToolStripMenuItem.Visible = false;
            _nameTable = new NameTable();
            _nsmgr = new XmlNamespaceManager(_nameTable);
            _nsmgr.AddNamespace("ts", XQTSNamespace);
            _testTab = new DataTable();
            _testTab.Columns.Add("Select", typeof(Boolean));
            _testTab.Columns.Add("Name", typeof(String));
            _testTab.Columns.Add("FilePath", typeof(String));
            _testTab.Columns.Add("scenario", typeof(String));
            _testTab.Columns.Add("Creator", typeof(String));
            _testTab.Columns.Add("Node", typeof(Object));
            _testTab.Columns.Add("Description", typeof(String));
            _ignoredTest = new HashSet<string>(s_ignoredTest);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "XQTSCatalog file (*.xml)|*.xml|All files (*.*)|*.*";
                if (dialog.ShowDialog() == DialogResult.OK)
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        OpenCatalog(dialog.FileName);
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                    }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_out != null)
                _out.Flush();
            if (_progressBar != null)
            {
                _progressBar.Value = _total;
                _statusLabel.Text = String.Format("{0}/{1}({2} Failed)", _total,
                    _progressBar.Maximum, _total - _passed);
            }
        }

        private void OpenCatalog(string fileName)
        {
            _catalog = new XmlDocument(_nameTable);
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Schemas = schemaSet;
            settings.DtdProcessing = DtdProcessing.Ignore;
            XmlUrlResolver resolver = new XmlUrlResolver();
            resolver.Credentials = CredentialCache.DefaultCredentials;
            settings.XmlResolver = resolver;
            settings.NameTable = _nameTable;
            settings.ValidationFlags = XmlSchemaValidationFlags.ProcessSchemaLocation |
                 XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationType = ValidationType.Schema;
            using (XmlReader reader = XmlReader.Create(fileName, settings))
            {
                _catalog.Load(reader);
                reader.Close();
            }
            if (!(_catalog.DocumentElement.NamespaceURI == XQTSNamespace &&
                  (_catalog.DocumentElement.LocalName == "test-suite")))
                throw new ArgumentException("Input file is not XQTS catalog.");
            if (_catalog.DocumentElement.GetAttribute("version") != "1.0.2")
                throw new NotSupportedException("Only version 1.0.2 is XQTS supported.");
            _basePath = Path.GetDirectoryName(fileName);
            _sourceOffsetPath = _catalog.DocumentElement.GetAttribute("SourceOffsetPath");
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
                    _out.WriteLine("Schema file {0} does not exists", schemaFileName);
                _schema.Add(id, new string[] { targetNs, schemaFileName });
            }
            foreach (XmlElement node in _catalog.SelectNodes("/ts:test-suite/ts:sources/ts:source", _nsmgr))
            {
                string id = node.GetAttribute("ID");
                string sourceFileName = Path.Combine(_basePath, node.GetAttribute("FileName").Replace('/', '\\'));
                if (!File.Exists(sourceFileName))
                    _out.WriteLine("Source file {0} does not exists", sourceFileName);
                _sources.Add(id, sourceFileName);
            }
            foreach (XmlElement node in _catalog.SelectNodes("/ts:test-suite/ts:sources/ts:collection", _nsmgr))
            {
                string id = node.GetAttribute("ID");
                XmlNodeList nodes = node.SelectNodes("ts:input-document", _nsmgr);
                String[] items = new String[nodes.Count];
                int k = 0;
                foreach (XmlElement curr in nodes)
                {
                    if (!_sources.ContainsKey(curr.InnerText))
                        _out.WriteLine("Referenced source ID {0} in collection {1} not exists", curr.InnerText, id);
                    items[k++] = curr.InnerText;
                }
                _collection.Add(id, items);
            }
            foreach (XmlElement node in _catalog.SelectNodes("/ts:test-suite/ts:sources/ts:module", _nsmgr))
            {
                string id = node.GetAttribute("ID");
                string moduleFileName = Path.Combine(_basePath, node.GetAttribute("FileName").Replace('/', '\\') + _queryFileExtension);
                if (!File.Exists(moduleFileName))
                    _out.WriteLine("Module file {0} does not exists", moduleFileName);
                _module.Add(id, moduleFileName);
            }
            treeView1.Nodes.Clear();
            treeView1.BeginUpdate();
            TreeNode rootNode = new TreeNode("Test-suite", 0, 0);
            treeView1.Nodes.Add(rootNode);
            ReadTestTree(_catalog.DocumentElement, rootNode);
            treeView1.EndUpdate();
            rootNode.Expand();
        }

        private void ReadTestTree(XmlNode node, TreeNode parentNode)
        {
            foreach (XmlNode child in node.ChildNodes)
                if (child.LocalName == "test-group" && child.NamespaceURI == XQTSNamespace)
                {
                    XmlElement elem = (XmlElement)child;
                    TreeNode childNode = new TreeNode(elem.GetAttribute("name"));
                    childNode.Tag = child;
                    ReadTestTree(child, childNode);
                    parentNode.Nodes.Add(childNode);
                }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            dataGridView1.DataSource = null;
            _testTab.Clear();
            testToolStripMenuItem.Visible = false;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                XmlNode node = e.Node.Tag as XmlNode;
                XmlNodeList nodes;
                if (node != null)
                {
                    XmlElement elem = node.SelectSingleNode("ts:GroupInfo/ts:title", _nsmgr) as XmlElement;
                    if (elem != null)
                        label3.Text = elem.InnerText;
                    else
                        label3.Text = "";
                    nodes = node.SelectNodes(".//ts:test-case", _nsmgr);
                }
                else
                    nodes = _catalog.SelectNodes(".//ts:test-case", _nsmgr);

                foreach (XmlElement child in nodes)
                {
                    DataRow row = _testTab.NewRow();
                    row[0] = false;
                    row[1] = child.GetAttribute("name");
                    row[2] = child.GetAttribute("FilePath");
                    row[3] = child.GetAttribute("scenario");
                    row[4] = child.GetAttribute("Creator");
                    row[5] = child;
                    XmlElement desc = (XmlElement)child.SelectSingleNode("ts:description", _nsmgr);
                    if (desc != null)
                        row[6] = desc.InnerText;
                    _testTab.Rows.Add(row);
                }
                dataGridView1.DataSource = _testTab;
                dataGridView1.Columns[0].Width = 50;
                dataGridView1.Columns[1].ReadOnly = true;
                dataGridView1.Columns[1].Width = 100;
                dataGridView1.Columns[2].ReadOnly = true;
                dataGridView1.Columns[2].Width = 190;
                dataGridView1.Columns[3].ReadOnly = true;
                dataGridView1.Columns[3].Width = 80;
                dataGridView1.Columns[4].ReadOnly = true;
                dataGridView1.Columns[4].Width = 150;
                dataGridView1.Columns[5].Visible = false;
                dataGridView1.Columns[6].Width = 300;
                dataGridView1.Columns[6].ReadOnly = true;
                testToolStripMenuItem.Visible = _testTab.Rows.Count > 0;

                if (node == null)
                {
                    int sel = 0;
                    HashSet<XmlNode> hs = new HashSet<XmlNode>();
                    foreach (XmlNode child in
                        _catalog.SelectNodes(".//ts:test-group[@name='MinimalConformance']//ts:test-case", _nsmgr))
                    {
                        if (((XmlElement)child).GetAttribute("is-XPath2") != "false")
                            hs.Add(child);
                    }
                    foreach (XmlNode child in
                        _catalog.SelectNodes(".//ts:test-group[@name='QuantExprWith']//ts:test-case", _nsmgr))
                        hs.Remove(child);
                    foreach (XmlNode child in
                        _catalog.SelectNodes(".//ts:test-group[@name='XQueryComment']//ts:test-case", _nsmgr))
                        hs.Remove(child);
                    foreach (XmlNode child in
                        _catalog.SelectNodes(".//ts:test-group[@name='Surrogates']//ts:test-case", _nsmgr))
                        hs.Remove(child);
                    foreach (XmlNode child in
                        _catalog.SelectNodes(".//ts:test-group[@name='SeqIDFunc']//ts:test-case", _nsmgr))
                        hs.Remove(child);
                    foreach (XmlNode child in
                        _catalog.SelectNodes(".//ts:test-group[@name='SeqCollectionFunc']//ts:test-case", _nsmgr))
                        hs.Remove(child);
                    foreach (XmlNode child in
                        _catalog.SelectNodes(".//ts:test-group[@name='SeqDocFunc']//ts:test-case", _nsmgr))
                        hs.Remove(child);
                    foreach (XmlNode child in
                        _catalog.SelectNodes(".//ts:test-group[@name='StaticBaseURIFunc']//ts:test-case", _nsmgr))
                        hs.Remove(child);
                    foreach (XmlNode child in
                        _catalog.SelectNodes(".//ts:test-group[@name='FullAxis']//ts:test-case", _nsmgr))
                        hs.Add(child);
                    sel = 0;
                    foreach (DataRow row in _testTab.Rows)
                    {
                        XmlElement curr = (XmlElement)row[5];
                        string name = curr.GetAttribute("name");
                        if (hs.Contains(curr) && !_ignoredTest.Contains(name))
                        {
                            row[0] = true;
                            sel++;
                        }
                    }
                    toolStripStatusLabel1.Text =
                        String.Format("{0} test case(s) loaded, {1} supported selected.", _testTab.Rows.Count, sel);
                }
                else
                    toolStripStatusLabel1.Text =
                        String.Format("{0} test case(s) loaded.", _testTab.Rows.Count);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    foreach (DataRow row in _testTab.Rows)
                        row[0] = true;
                    toolStripStatusLabel1.Text = String.Format("{0} tests selected.", _testTab.Rows.Count);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void viewCurrentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringWriter tw = new StringWriter();
            if (dataGridView1.CurrentRow != null)
            {
                string fileName = GetFilePath((XmlElement)dataGridView1.CurrentRow.Cells[5].Value);
                tw.WriteLine(fileName);
                TextReader textReader = new StreamReader(fileName, true);
                String line;
                while ((line = textReader.ReadLine()) != null)
                    tw.WriteLine(line);
                textReader.Close();
            }
            richTextBox1.Text = tw.ToString();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            StringWriter tw = new StringWriter();
            if (dataGridView1.CurrentRow != null)
            {
                XmlElement testCase = (XmlElement)dataGridView1.CurrentRow.Cells[5].Value;
                foreach (XmlElement outputFile in testCase.SelectNodes("ts:output-file", _nsmgr))
                {
                    string fileName = GetResultPath(testCase, outputFile.InnerText);
                    tw.WriteLine("{0}:", fileName);
                    TextReader textReader = new StreamReader(fileName, true);
                    String line;
                    while ((line = textReader.ReadLine()) != null)
                        tw.WriteLine(line);
                    textReader.Close();
                }
            }
            richTextBox1.Text = tw.ToString();
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            StringWriter tw = new StringWriter();
            if (dataGridView1.CurrentRow != null)
            {
                XmlElement testCase = (XmlElement)dataGridView1.CurrentRow.Cells[5].Value;
                foreach (XmlElement inputFile in testCase.SelectNodes("ts:input-file", _nsmgr))
                {
                    string fileName = _sources[inputFile.InnerText];
                    tw.WriteLine("{0}:", fileName);
                    TextReader textReader = new StreamReader(fileName, true);
                    String line;
                    while ((line = textReader.ReadLine()) != null)
                        tw.WriteLine(line);
                    textReader.Close();
                }
            }
            richTextBox1.Text = tw.ToString();
        }

        private void runCurrentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
                return;
            richTextBox1.Clear();
            XmlElement testCase = (XmlElement)dataGridView1.CurrentRow.Cells[5].Value;
            try
            {
                object res = PrepareXPath(_out, testCase).Evalute();
                if (res != null)
                {
                    if (res is XPath2NodeIterator)
                    {
                        XPath2NodeIterator iter = (XPath2NodeIterator)res;
                        while (iter.MoveNext())
                        {
                            _out.WriteLine();
                            _out.WriteItem(iter.Current);
                        }
                    }
                    else
                    {
                        _out.WriteLine();
                        _out.WriteItem(res);
                    }
                }
            }
            catch (Exception ex)
            {
                _out.WriteLine();
                _out.WriteLine(ex);
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
                return;
            richTextBox1.Clear();
            XmlElement testCase = (XmlElement)dataGridView1.CurrentRow.Cells[5].Value;
            if (PerformTest(_out, testCase, false))
                _out.WriteLine("Passed.");
            else
                _out.WriteLine("Failed.");
        }

        private void batchRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            dataGridView1.EndEdit();
            int count = 0;
            foreach (DataRow r in _testTab.Select(""))
                if ((bool)r[0])
                    count++;
            if (count == 0)
            {
                MessageBox.Show("No test case selected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (count > 100 && _repeatCount == 0)
            {
                if (MessageBox.Show(String.Format("{0} test case(s) selected. Continue ?", count),
                    "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                    return;
            }
            _total = 0;
            _passed = 0;
            _progressBar = new ToolStripProgressBar();
            _progressBar.Minimum = 0;
            _progressBar.Value = 0;
            _progressBar.Maximum = count;
            _statusLabel = new ToolStripStatusLabel();
            toolStripStatusLabel1.Text = "Running";
            statusStrip1.Items.Add(_progressBar);
            statusStrip1.Items.Add(_statusLabel);
            BatchRun();
        }

        private void repeatRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _repeatCount = 100;
            richTextBox1.Clear();
            dataGridView1.EndEdit();
            int count = 0;
            foreach (DataRow r in _testTab.Select(""))
                if ((bool)r[0])
                    count++;
            if (count == 0)
            {
                MessageBox.Show("No test case selected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (count > 100 && _repeatCount == 0)
            {
                if (MessageBox.Show(String.Format("{0} test case(s) selected. Continue ?", count),
                    "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                    return;
            }
            _total = 0;
            _passed = 0;
            _progressBar = new ToolStripProgressBar();
            _progressBar.Minimum = 0;
            _progressBar.Value = 0;
            _progressBar.Maximum = count * _repeatCount;
            _statusLabel = new ToolStripStatusLabel();
            toolStripStatusLabel1.Text = "Running";
            statusStrip1.Items.Add(_progressBar);
            statusStrip1.Items.Add(_statusLabel);
            BatchRun();
        }

        private void BatchRun()
        {
            Thread worker = new Thread(new ThreadStart(BatchTestThread));
            worker.Start();
        }

        private void CompleteBatchTest()
        {
            if (_repeatCount > 1)
            {
                _repeatCount--;
                BatchRun();
            }
            else
            {
                if (_total > 0)
                {
                    decimal total = _total;
                    decimal passed = _passed;
                    _out.WriteLine("{0} executed, {1} ({2}%) succeeded.", total, passed,
                        Math.Round(passed / total * 100, 2));
                }
                foreach (DataRow row in _testTab.Rows)
                    row[0] = false;
                statusStrip1.Items.Remove(_statusLabel);
                statusStrip1.Items.Remove(_progressBar);
                toolStripStatusLabel1.Text = "Done";
                _progressBar = null;
                _statusLabel = null;
            }
        }

        private void BatchTestThread()
        {
            foreach (DataRow dr in _testTab.Select(""))
            {
                if ((bool)dr[0])
                {
                    XmlElement curr = (XmlElement)dr[5];
                    string id = curr.GetAttribute("name");
                    TextWriter tw = new StringWriter();
                    if (PerformTest(tw, curr, true))
                    {
                        tw.WriteLine("Passed.");
                        Interlocked.Increment(ref _passed);
                    }
                    else
                    {
                        tw.WriteLine("Failed.");
                        _out.Write(tw.ToString());
                    }
                    //Trace.WriteLine(tw.ToString());
                    Interlocked.Increment(ref _total);
                }
            }
            Invoke(new Action(CompleteBatchTest));
        }

        private string GetFilePath(XmlElement node)
        {
            XmlNode queryName = node.SelectSingleNode("ts:query/@name", _nsmgr);
            return _basePath + "\\" + (_queryOffsetPath + node.GetAttribute("FilePath") +
                queryName.Value + _queryFileExtension).Replace('/', '\\');
        }

        private string GetResultPath(XmlElement node, string fileName)
        {
            return _basePath + "\\" + (_resultOffsetPath + node.GetAttribute("FilePath") + fileName).Replace('/', '\\');
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

        private String PrepareQueryText(string text)
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

        struct PreparedXPath
        {
            public XPath2Expression expression;
            public IContextProvider provider;
            public IDictionary<XmlQualifiedName, object> vars;

            public object Evalute()
            {
                return expression.Evaluate(provider, vars);
            }

            public XPath2ResultType GetResultType()
            {
                return expression.GetResultType(vars);
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
            String xpath = PrepareQueryText(textReader.ReadToEnd());
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

        private bool PerformTest(TextWriter tw, XmlElement testCase, bool batchTest)
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
                    res = preparedXPath.Evalute();
                    if (res != Undefined.Value &&
                        preparedXPath.GetResultType() != expectedType)
                    {
                        if (batchTest)
                            _out.Write("{0}: ", testCase.GetAttribute("name"));
                        _out.WriteLine("Expected type '{0}' differs the actual type '{1}'",
                            expectedType, preparedXPath.GetResultType());
                    }
                }
                catch (XPath2Exception)
                {
                    if (testCase.GetAttribute("scenario") == "parse-error" ||
                        testCase.GetAttribute("scenario") == "runtime-error" ||
                        testCase.SelectSingleNode("ts:expected-error", _nsmgr) != null)
                        return true;
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
                                    return true;
                            }
                            else if (compare == "XML")
                            {
                                if (CompareResult(testCase, GetResultPath(testCase, outputFile.InnerText), res, true))
                                    return true;
                            }
                            else if (compare == "Inspect")
                            {
                                _out.WriteLine("{0}: Inspection needed.", testCase.GetAttribute("name"));
                                return true;
                            }
                            else if (compare == "Ignore")
                                continue;
                            else
                                throw new InvalidOperationException();
                        }
                        return false;
                    }
                    else if (testCase.GetAttribute("scenario") == "runtime-error")
                    {
                        if (res is XPath2NodeIterator)
                        {
                            XPath2NodeIterator iter = (XPath2NodeIterator)res;
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
                        return true;
                    throw;
                }
            }
            catch (Exception ex)
            {
                _out.WriteLine();
                _out.WriteLine(ex);
                return false;
            }
        }

        private void TraceIter(XPath2NodeIterator iter)
        {
            iter = iter.Clone();
            foreach (XPathItem item in iter)
            {
                if (item.IsNode)
                    Trace.WriteLine(((XPathNavigator)item).OuterXml);
                else
                    Trace.WriteLine(item.Value);
            }
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
            XmlDocument doc1 = new XmlDocument(_nameTable);
            if (xmlCompare)
                doc1.Load(sourceFile);
            else
            {
                StringBuilder sb = new StringBuilder();
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
            else
                if (value is XPathItem)
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
            bool res = comparer.DeepEqual(doc1.CreateNavigator(), doc2.CreateNavigator());
            return res;
        }

        private void xMLQueryTestSuiteHomeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.w3.org/XML/Query/test-suite/");
        }

        private void standaloneXQueryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://github.com/StefH/XPath2.Net");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Assembly asm = Assembly.GetAssembly(typeof(XPath2Expression));
            MessageBox.Show(
                String.Format("{0} {1}\n", Text, asm.GetName().Version) +
                "Copyright © Semyon A. Chertkov, 2009-2013\n" +
                "http://www.wmhelp.com\n" +
                "e-mail: semyonc@gmail.com",
                "About " + Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.wmhelp.com");
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
        }

        private void toolStripFind_Click(object sender, EventArgs e)
        {
            using (FindForm form = new FindForm())
            {
                form.RowIndex = 0;
                form.Value = _lastFindString;
                form.FindNext += new EventHandler(form_FindNext);
                form.ShowDialog();
            }
        }

        void form_FindNext(object sender, EventArgs e)
        {
            FindForm form = (FindForm)((Button)sender).Parent;
            for (int index = form.RowIndex; index < dataGridView1.Rows.Count; index++)
            {
                string text = dataGridView1.Rows[index].Cells[1].Value.ToString();
                if (text.StartsWith(form.Value))
                {
                    _lastFindString = form.Value;
                    form.RowIndex = index + 1;
                    dataGridView1.CurrentCell = dataGridView1.Rows[index].Cells[1];
                    return;
                }
            }
            form.RowIndex = 0;
            MessageBox.Show("Passed the end of the table.", "Information",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
