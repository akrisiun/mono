using System;
using System.Xml;
using System.Xml.Linq;

#if !NETCORE

namespace Mono.XHtml
{
    public static class XmlEl
    {
        static XmlEl() { xmlDoc = null; }
        public static XmlDocument xmlDoc;

#region Element content

        public static XmlElement CreateElement(string name)
        {
            if (xmlDoc == null)
                xmlDoc = new XmlDocument();

            var el = xmlDoc.CreateElement(name);
            return el;
        }

        public static XmlElement Append(this XmlElement el, XmlNode newChild)
        {
            el.AppendChild(newChild);
            return el;
        }

        public static XmlElement SetContent(this XmlElement el, string text)
        {
            el.InnerText = text;
            return el;
        }
        public static XmlElement SetContent(this XmlElement el, object obj)
        {
            el.InnerText = obj.ToString() ?? "";
            return el;
        }

#endregion

        public static XmlElement SetFormat(this XmlElement el, decimal? number, string format)
        {
            if (number == null)
                el.InnerText = "";
            else
                el.InnerText = string.Format(format, number);

            return el;
        }

        public static XmlElement SetStyle(this XmlElement el, string value)
        {
            el.SetAttribute("style", value);
            return el;
        }

        public static XmlElement SetAttr(this XmlElement el, string name, string value)
        {
            el.SetAttribute(name, value);
            return el;
        }

        public static XmlNode SelectXmlNode(this XmlDocument doc, string nodeXPath)
        {
            var node = doc.SelectSingleNode(nodeXPath);
            return node;
        }

    }

}

#endif