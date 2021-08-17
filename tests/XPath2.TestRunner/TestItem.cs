using System.Xml;

namespace XPath2.TestRunner
{
    internal class TestItem
    {
        public bool Selected { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string FilePath { get; set; }

        public string Scenario { get; set; }

        public string Creator { get; set; }

        public XmlElement Node { get; set; }
    }
}