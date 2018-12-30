using System;
using System.Xml.Linq;
using System.Data.SqlClient;

#if !NETCORE
using System.Xml.XPath;
using System.Data;

namespace Mono.Entity
{
    [Obsolete]
    public static class SqlXEl
    {
        [Obsolete]
        public static XElement ExecXml(this SqlCommand cmd, string RootName = "Root")
        {
            Guard.CheckNotNull(cmd.Connection);
            Guard.Check(cmd.Connection.State == ConnectionState.Open);

            using (var reader = cmd.ExecuteXmlReader())
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
        }
    }
}

#endif 