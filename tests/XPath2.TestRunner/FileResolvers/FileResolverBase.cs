using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Schema;

namespace XPath2.TestRunner.FileResolvers
{
    public abstract class FileResolverBase
    {
        protected readonly TextWriter _out;
        protected readonly string _queryOffsetPath;
        protected readonly string _resultOffsetPath;
        protected readonly string _queryFileExtension;
        protected readonly XmlNamespaceManager _namespaceManager;
        
        public XmlDocument Catalog { get; }

        protected FileResolverBase(TextWriter tw, string uri, XmlNamespaceManager namespaceManager)
        {
            _out = tw;
            _namespaceManager = namespaceManager;

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
            settings.NameTable = namespaceManager.NameTable;
            settings.ValidationFlags = XmlSchemaValidationFlags.ProcessSchemaLocation | XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationType = ValidationType.Schema;

            Catalog = new XmlDocument(namespaceManager.NameTable);

            using (var reader = XmlReader.Create(uri, settings))
            {
                Catalog.Load(reader);
                reader.Close();
            }

            _queryOffsetPath = Catalog.DocumentElement.GetAttribute("XQueryQueryOffsetPath");
            _resultOffsetPath = Catalog.DocumentElement.GetAttribute("ResultOffsetPath");
            _queryFileExtension = Catalog.DocumentElement.GetAttribute("XQueryFileExtension");            
        }
    }
}