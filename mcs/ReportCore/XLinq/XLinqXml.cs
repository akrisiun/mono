using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Dynamic;

namespace Mono.XLinq
{
    public static class XElemExpando
    {
        public static XElement ReadExpando(this XElement el, object[] obj)
        {
            int index = -1;
            foreach (var objItem in obj)
                el.Add(new XElement(index++.ToString(), objItem));
            return el;
        }

        public static XElement ReadExpando(this XElement el, ExpandoObject obj)
        {
            foreach (var objItem in obj)
                if (objItem.Value != null)
                {
                    // XElement: Name cannot begin with the '0' character, hexadecimal value 0x30.
                    if (objItem.Key[0] > '9') // 'A'
                        el.Add(new XElement(objItem.Key, objItem.Value));
                    else
                        el.Add(new XElement('_' + objItem.Key, objItem.Value));
                }

            return el;
        }
    }
}
