using System;
using System.Text;
using System.Xml;
using System.IO;

// #if !NETCORE
#if !NETSTANDARD1_3

using Linq = System.Xml.Linq; // for XElement
using XPath = System.Xml.XPath;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Mono.XHtml
{
    // XPath / XPathDocument helper

    public static class XPathHelper
    {
#region XPath Convert, Select nodes

        public static Linq.XElement XPathSingleNode(this Linq.XElement root, string expression, IXmlNamespaceResolver resolver = null)
        {
            Linq.XElement result = root == null ? null : Extensions.XPathSelectElement(root, expression, resolver);
            return result;
        }

        public static Linq.XElement ToXElement(this XPath.XPathDocument xpathDoc)
        {
            var navigator = xpathDoc.CreateNavigator() as XPathNavigator;

            using (TextWriter writer = new Utf8StringWriter())
            {
                XmlWriter xmlWriter = XmlWriter.Create(writer);

                // was: XmlReader reader = XmlReader.Create(input);
                var element = Linq.XElement.Parse(writer.ToString());
                return element;
            }
        }

        public static XPath.XPathDocument ToXPath(this Linq.XElement element)
        {
            var xpath = new XPathDocument(element.CreateReader());
            return xpath;
        }

        public static XPathDocument XPathDocument(this Linq.XElement element)
        {
            return element == null ? null
                 : new XPathDocument(element.CreateReader());
        }

        // static XPathNavigator SelectSingleNode(this XPathDocument xpathDoc, string nodeXPath)
        // -> Extensions.XPathSelectElement(root, expression, resolver);

        public static Linq.XElement SelectSingleNode(this Linq.XElement node, string nodeXPath)
        {
            return Extensions.XPathSelectElement(node, nodeXPath);
        }

#endregion

#region Transform

        public static Linq.XElement XsltTransform(Linq.XElement source, string xslMarkup,
                    Action<XslCompiledTransform> onXsltLoad, object xsltExtension = null,
                    string urn = "urn:request-info")
        {
            var doc = new Linq.XDocument();

            using (XmlWriter writer = doc.CreateWriter())
            {
                XslCompiledTransform xslt = new XslCompiledTransform();
                XsltArgumentList arguments = new XsltArgumentList();
                if (xsltExtension != null)
                    arguments.AddExtensionObject(urn, xsltExtension);

                // Load the style sheet.
                xslt.Load(XmlReader.Create(new StringReader(xslMarkup)));

#if !MAC
                // Execute the transform and output the results to a writer.
                xslt.Transform(source.CreateNavigator() as IXPathNavigable,
                     arguments: arguments, results: writer, documentResolver: XmlNullResolver.Singleton);
#endif

                writer.Close();
            }
            return doc.Root;
        }

        public static void XTransformToWriter(this Linq.XElement xmlDoc, TextWriter writer,
                    Linq.XElement xmlXslt,
                    object xsltExtension = null, string urn = "urn:request-info")
        {
            XTransformToWriter(xmlDoc, writer,
                               onXsltLoad: (trans) => trans.Load(xmlXslt.CreateReader()),
                               xsltExtension: xsltExtension, urn: urn);
        }

        public static void XTransformToWriter(this Linq.XElement xmlDoc, TextWriter writer,
                    Action<XslCompiledTransform> onXsltLoad, object xsltExtension = null,
                    string urn = "urn:request-info")
        {
            XslCompiledTransform trans = new XslCompiledTransform();
            onXsltLoad(trans);

            if (xsltExtension != null)
            {
                XsltArgumentList xslArg = new XsltArgumentList();
                xslArg.AddExtensionObject(urn, xsltExtension);

                XTransformTo(trans, xmlDoc.CreateReader(), xslArg, writer);
            }
            else
            {
                // trans.Transform(xmlDoc.CreateNavigator() as IXPathNavigable, arguments: null, results: writer);
                XTransformTo(trans, xmlDoc.CreateReader(), null, writer);
            }
        }

        public static void XsltTransform(// TextWriter writer, XPath.XPathDocument doc)
                    Linq.XElement xmlDoc, Stream results,
                    Action<XslCompiledTransform> onXsltLoad, object xsltExtension = null,
                    string urn = "urn:request-info")
        {
            // Transform XLST validation
            XsltArgumentList xslArg = new XsltArgumentList();
            XslCompiledTransform trans = new XslCompiledTransform();

            onXsltLoad(trans);

            //// var xsltFileFull = Xslt;
            //if (Context != null && Xslt.Length > 0 && File.Exists(xsltFileFull))
            //{
            //    string serverUrl = Context.Request.Url.Scheme + "://" + Context.Request.Url.Authority + "/";

            //    XsltIncludeResolver resolver = new XsltIncludeResolver(serverUrl);  // for <xsl:include>

            //    trans.Load(xsltFileFull, XsltSettings.TrustedXslt, resolver as XmlUrlResolver);

            //else
            //    trans.Load(xsltFileFull);

            // Add an object to convert
            if (xsltExtension != null)
                xslArg.AddExtensionObject(urn, xsltExtension);

            // http://referencesource.microsoft.com/#System.Xml/System/Xml/Xslt/XslCompiledTransform.cs

            var navigator = Extensions.CreateNavigator(xmlDoc) as IXPathNavigable;

            using (var xmlwriter = XmlWriter.Create(results))
            {
                trans.Transform(navigator, xslArg, xmlwriter, documentResolver: XmlNullResolver.Singleton);
                xmlwriter.Close();
            }
        }

        public static Linq.XElement XTransform(this XslCompiledTransform trans, XmlReader input, XsltArgumentList arguments = null)
        {
            Guard.CheckArgumentNull(input);
            var result = new Linq.XDocument();

            using (XmlWriter writer = result.CreateWriter())
            {
                trans.Transform(input, arguments, writer, XmlNullResolver.Singleton);
                writer.Close();
            }

            return result.Root;
        }

        public static void XTransformTo(this XslCompiledTransform trans, XmlReader input, XsltArgumentList arguments, TextWriter results)
        {
            Guard.CheckArgumentNull(input);
            Guard.CheckArgumentNull(results);

            var outputSettings = OutputSettings(trans);
            using (XmlWriter writer = XmlWriter.Create(results, outputSettings))
            {
                trans.Transform(input, arguments, writer, XmlNullResolver.Singleton); // XsltConfigSection.CreateDefaultResolver());
                writer.Close();
            }
        }

#endregion

        private static XmlWriterSettings OutputSettings(this XslCompiledTransform trans) { return trans.OutputSettings; }

    }

    // Origin: http://referencesource.microsoft.com/#System.Xml/System/Xml/XmlNullResolver.cs
    internal class XmlNullResolver : XmlResolver
    {
        public static readonly XmlNullResolver Singleton = new XmlNullResolver();

        // Private constructor ensures existing only one instance of XmlNullResolver
        private XmlNullResolver() { }

        public override Object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            System.Diagnostics.Debugger.Break();
            throw new XmlException("GetEntity error");
        }

        public override System.Net.ICredentials Credentials {
            // get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }

    // StringWriter will advertise itself as being in UTF-16. Usually XML is in UTF-8. You can fix this by subclassing StringWriter;
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }

}

/* Transform 

      //------------------------------------------------
        // Transform methods which take an IXPathNavigable
        //------------------------------------------------
 
        public void Transform(IXPathNavigable input, XmlWriter results) {
            CheckArguments(input, results);
            Transform(input, (XsltArgumentList)null, results, XsltConfigSection.CreateDefaultResolver());
        }
 
        public void Transform(IXPathNavigable input, XsltArgumentList arguments, XmlWriter results) {
            CheckArguments(input, results);
            Transform(input, arguments, results, XsltConfigSection.CreateDefaultResolver());
        }
 
        public void Transform(IXPathNavigable input, XsltArgumentList arguments, TextWriter results) {
            CheckArguments(input, results);
            using (XmlWriter writer = XmlWriter.Create(results, OutputSettings)) {
                Transform(input, arguments, writer, XsltConfigSection.CreateDefaultResolver());
                writer.Close();
            }
        }
 
        public void Transform(IXPathNavigable input, XsltArgumentList arguments, Stream results) {
            CheckArguments(input, results);
            using (XmlWriter writer = XmlWriter.Create(results, OutputSettings)) {
                Transform(input, arguments, writer, XsltConfigSection.CreateDefaultResolver());
                writer.Close();
            }
        }
 
        //------------------------------------------------
        // Transform methods which take an XmlReader
        //------------------------------------------------
 
        public void Transform(XmlReader input, XmlWriter results) {
            CheckArguments(input, results);
            Transform(input, (XsltArgumentList)null, results, XsltConfigSection.CreateDefaultResolver());
        }
 
        public void Transform(XmlReader input, XsltArgumentList arguments, XmlWriter results) {
            CheckArguments(input, results);
            Transform(input, arguments, results, XsltConfigSection.CreateDefaultResolver());
        }
 
        public void Transform(XmlReader input, XsltArgumentList arguments, TextWriter results) {
            CheckArguments(input, results);
            using (XmlWriter writer = XmlWriter.Create(results, OutputSettings)) {
                Transform(input, arguments, writer, XsltConfigSection.CreateDefaultResolver());
                writer.Close();
            }
        }
 
        public void Transform(XmlReader input, XsltArgumentList arguments, Stream results) {
            CheckArguments(input, results);
            using (XmlWriter writer = XmlWriter.Create(results, OutputSettings)) {
                Transform(input, arguments, writer, XsltConfigSection.CreateDefaultResolver());
                writer.Close();
            }
        }

*/

#endif