using System.Linq;

namespace Mono.XLinq
{
    public class XRaw : System.Xml.Linq.XText
    {
        public XRaw(string text) : base(text) { }
        public XRaw(System.Xml.Linq.XText text) : base(text) { }

        public override void WriteTo(System.Xml.XmlWriter writer)
        {
            writer.WriteRaw(this.Value);
        }
    }
}
