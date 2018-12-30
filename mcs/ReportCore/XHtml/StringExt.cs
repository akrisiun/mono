using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Mono.XHtml
{
    public static class StringExt
    {
        public static string StrExtract(this string str, string from, string till = "", string fromEnd = "")
        {
            int pos1 = str.IndexOf(from);
            if (pos1 < 0) return string.Empty;

            string strRest = str.Substring(pos1 + from.Length);
            if (fromEnd.Length > 0)
            {
                pos1 = strRest.IndexOf(fromEnd);
                if (pos1 < 0)
                    return string.Empty;

                strRest = strRest.Substring(pos1 + fromEnd.Length);
            }

            int pos2 = strRest.IndexOf(till);
            if (pos2 < 0) return strRest;

            return strRest.Substring(0, pos2);
        }

        public static string ProperInvariant(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;
            return char.ToUpperInvariant(str[0]) + str.Substring(1).ToLower();
        }

        public static string Proper(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;
            return char.ToUpper(str[0], System.Globalization.CultureInfo.CurrentCulture)
                 + str.Substring(1).ToLower();
        }

        public static Dictionary<string, string> ParseParamDict(string param)
        {
            Dictionary<string, string> mm = new Dictionary<string, string>();
            if (param == null)
                return mm;
            char[] sep = { '&' };
            string[] toks = param.Split(sep);
            foreach (string s in toks)
            {
                int idx = s.IndexOf("=");
                if (idx > 0)
                {
                    string name = s.Substring(0, idx);
                    string value = s.Substring(idx + 1);
                    mm[name] = value;
                }
            }

            return mm;
        }

        public static ExpandoObject ParseParam(string param)
        {
            IDictionary<string, object> mm = new ExpandoObject();
            if (param == null)
                return mm as ExpandoObject;
            char[] sep = { '&' };
            string[] toks = param.Split(sep);
            foreach (string s in toks)
            {
                int idx = s.IndexOf("=");
                if (idx > 0)
                {
                    string name = s.Substring(0, idx);
                    string value = s.Substring(idx + 1);
                    mm[name] = value;
                }
            }

            return mm as ExpandoObject;
        }
        
        public static string EncodeParam(this Dictionary<string, string> paramDict)
        {
            StringBuilder param = new StringBuilder();
            foreach (var item in paramDict)
            {
                if (param.Length > 0)
                    param.Append('&');
                param.Append(item.Key);
                param.Append('=');
                param.Append(item.Value);
            }
            return param.ToString();
        }

        //public static XmlDocument ReplaceSelf(this XmlElement el, XmlReader reader)
        //{
        //    var doc = el.OwnerDocument;
        //    XPathNavigator xn = new XPathDocument(reader).CreateNavigator();
        //    if (xn.HasChildren)
        //        el.CreateNavigator().ReplaceSelf(xn);
        //    else
        //        el.RemoveAll();
        //    return doc;
        //}

    }
}
