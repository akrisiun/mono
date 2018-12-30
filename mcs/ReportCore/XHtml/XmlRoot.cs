
namespace Mono.XHtml
{
    public static class XmlRoot
    {
#if NET451
        [System.Obsolete]
        public static System.Xml.XmlDocument RootXml(string encoding = null)
        {
            var xml = new System.Xml.XmlDocument();

            var elRoot = xml.CreateElement("Root");
            xml.AppendChild(elRoot);

            var xmlDeclaration = xml.CreateXmlDeclaration("1.0", encoding ?? "utf-8", null);
            xml.InsertBefore(xmlDeclaration, elRoot);

            return xml;
        }
#endif
    }
}
