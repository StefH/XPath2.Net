using System.Xml;

namespace XPath2.TestRunner
{
    internal class TreeNodeValue
    {
        public string Text { get; set; }

        public XmlNode Tag { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}