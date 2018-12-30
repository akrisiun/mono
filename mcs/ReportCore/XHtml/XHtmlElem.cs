using Mono.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Mono.XHtml
{

    public class XHtmlAttr : IComparable, IXmlLineInfo
    {
        public string Key { get; private set; }
        public string Value { get; private set; }

        public XHtmlAttr(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public XHtmlAttr(string key, object value)
        {
            Key = key;
            Value = value.ToString();
        }

        public override string ToString()
        {
            return " " + Key + "=\"" + Value + "\"";
        }

        // Zero This instance occurs in the same position in the sort order as obj. 
        // Greater than zero This instance follows obj in the sort order.
        public int CompareTo(object obj)
        {
            if (obj == null || this.ToString() == obj.ToString())
                return 0;
            return 1;
        }

        // XObject interface
        public bool HasLineInfo() { return false; }
        public virtual int LineNumber { get { return 0; } }
        public virtual int LinePosition { get { return 0; } }

    }

    public class XHtmlElem : IComparable, IXmlLineInfo, ILastError 
    {
        public string Tag { get; private set; }
        public List<object> Content { get; set; }
        public Exception LastError {get; set;}

        public XHtmlElem(string name, params object[] content)
        {
            Tag = name;
            Content = new List<object>();
            if (content != null)
                foreach (var obj in content)
                    Content.Add(obj);
        }

        public void AddAttr(string attr, string value)
        {
            Content.Add(new XHtmlAttr(attr, value));
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Tag))
                return String.Empty;

            return ToString(SaveOptions.DisableFormatting);
        }

        public virtual string ToString(SaveOptions options)
        {   
            var result = new StringWriter(new StringBuilder());
            WriteBeginTag(result);
            WriteContent(result);
            WriteEndTag(result);
            
            return result.ToString();
        }

        public void WriteBeginTag(TextWriter result)
        { 
            var headStart = "<" + Tag;
            var headEnd = ">";
            result.Write(headStart);
            if (Content != null)
                foreach (var attr in Content)
                {
                    if (attr is XHtmlAttr)
                        result.Write(attr.ToString());
                }
            result.WriteLine(headEnd);
        }

        public void WriteContent(TextWriter result)
        {
            if (Content != null)
                foreach (var obj in Content)
                {
                    if (!(obj is XHtmlAttr))
                    {
                        string objContent = null;
                        if (obj is XNode)
                        {
                            var node = obj as XNode;
                            try
                            {
                                var writer = XmlWriter.Create(result, new XmlWriterSettings { NewLineOnAttributes = true });

                                node.WriteTo(writer);
#if !NET40
                                writer.Dispose(); 
#else
                                writer.Close();
#endif
                                node = null;
                            }
                            catch (Exception ex) { LastError = ex; }

                            if (node != null && node is XElement)
                            {
                                objContent = (node as XElement).Value;
                            }
                        }
                        else
                        {
                            objContent = obj.ToString();
                        }

                        if (objContent != null)
                            result.WriteLine(objContent);
                    }
                }
        }

        public void WriteEndTag(TextWriter result)
        {
            var tagEnd = "</" + Tag + ">";
            result.WriteLine(tagEnd);
        }

        public int CompareTo(object obj)
        {
            var str = this.ToString();
            var objStr = obj.ToString();
            return String.Compare(str, objStr);
        }

        // XObject interface
        public bool HasLineInfo() { return false; }
        public virtual int LineNumber { get { return 0; } }
        public virtual int LinePosition { get { return 0; } }
    }

    public class XHtmlDocument
    {
        public XHtmlElem Root { get; set; }

        public XHtmlDocument(XHtmlElem root = null)
        {
            Root = root;
        }

        public override string ToString()
        {
            if (Root != null)
                return Root.ToString();
            return String.Empty;
        }

        // XObject interface
        public bool HasLineInfo() { return false; }
        public virtual int LineNumber { get { return 0; } }
        public virtual int LinePosition { get { return 0; } }

    }
}
