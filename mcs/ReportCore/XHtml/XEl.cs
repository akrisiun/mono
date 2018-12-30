using Mono.Entity;
using System;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Mono.XHtml
{
#if XLSX && !WEB
    using Mono.Internal;
#endif
    using System.Xml.XPath;

    // XElement extensions

    public static class XEl
    {
        public static XElement AsElement(this string text, ILastError errorCase)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            XElement div = null;
            if (text.Contains("<") && (text.Contains("/>") || text.Contains("</")))
                try
                {
                    div = XElement.Parse(text);
                }
                catch { ; }

            if (div == null)
            {
                try
                {
                    var doc = XDocument.Parse(SplitDivLines(text));
                    div = doc.Root;
                }
                catch (Exception ex) {
                    if (errorCase != null)
                        errorCase.LastError = ex;
                }
            }
            return div;
        }

        public static string SplitDivLines(string xmlText)
        {
            if (string.IsNullOrWhiteSpace(xmlText) || xmlText.Length < 8)
                return null;

            var div = new XElement("div");
            var lines = StringConvert.SplitLines(xmlText);
            foreach (var line in lines)
            {
                div.Add(line);
                div.Add(new XElement("br"));
            }

            var result = new StringBuilder(div.ToString());

            result.Replace("&lt;br/&gt;", "<br/>");
            result.Replace("&lt;b&gt;", "<b>");
            result.Replace("&lt;/b&gt;", "</b>");
            result.Replace("&lt;i&gt;", "<i>");
            result.Replace("&lt;/i&gt;", "</i>");

            return result.ToString();
        }

        public static XElement MultipleNodes(this XmlReader reader, string RootName = "Root")
        {
            var doc = new XDocument(new XElement(RootName));

            var xn = new XPathDocument(reader).CreateNavigator();
            XPathNodeIterator iterator = xn.Select("/*");
            foreach (XPathNavigator item in iterator)
            {
                doc.Root.Add(XElement.Load(item.ReadSubtree()));
            }

            return doc.Root;
        }

#region Attr if empty 

        public static T AttrEmpty<T>(this XElement el, string key, object ifEmpty = null) where T : IConvertible
        {
            if (el.HasAttributes)
            {
                if (el.Attribute(key) is XAttribute attr) {
                    if (typeof(T) == typeof(string))
                        return (T)(object)attr.Value;

                    IConvertible conv = attr.Value;
                    return (T)(object)conv.ToType(typeof(T), null);
                }
            }
            return (T)ifEmpty;
        }

        public static T AttrEmptyNull<T>(this XElement el, string key, object ifEmpty = null)
        {
            if (el.HasAttributes)
            {
                if (el.Attribute(key) is XAttribute attr && attr.Value != null) {
                    IConvertible conv = attr.Value;
                    Type u = Nullable.GetUnderlyingType(typeof(T));

                    T res = default(T);
                    IFormatProvider provider = CultureInfo.InvariantCulture;

                    if (u == typeof(Decimal))
                        res = (T)(object)Convert.ToDecimal(conv, provider);
                    else if (u == typeof(double))
                        res = (T)(object)Convert.ToDouble(conv, provider);
                    else
                        res = (T)(object)conv.ToType(u, provider);

                    if (res != null)
                        return res;
                }
            }
            return (T)ifEmpty;
        }

        public static T ElementEmpty<T>(this XElement el, string key, object ifEmpty = null) where T : IConvertible
        {
            if (el.HasElements)
            {
                if (el.Element(key) is XElement attr) {
                    if (typeof(T) == typeof(string))
                        return (T)(object)attr.Value;

                    IConvertible conv = attr.Value;
                    return (T)(object)conv.ToType(typeof(T), null);
                }
            }
            return (T)ifEmpty;
        }

#endregion
    }
}
