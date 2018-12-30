using Mono.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using IO = System.IO;

namespace Mono.XHtml
{
#if XLSX && !WEB && !NPOI_ORIGIN
    using Mono.Internal;
#endif

    // XIO - IO extensions (Safe null values)

    public static class XIO
    {
        static XIO()
        {
            File = new FileWrap();
            Path = new PathWrap();
            Directory = new DirectoryWrap();
            Date = new DateWrap();
            // + static XmlHelper();
        }

        // the Cannot create an instance of static class code issue if an instance creation statement references a static class.
        // static IO.File ioFIle = new { File = System.IO.File };
        public static FileWrap File { get; private set; }
        public static PathWrap Path { get; private set; }
        public static DirectoryWrap Directory { get; private set; }
        public static DateWrap Date { get; private set; }

        // public static IO.StreamReader FileReader(string filePath) { return new IO.StreamReader(filePath); }
        public static IO.StringReader StringReader(string str) { return new IO.StringReader(str); }
        public static string FileReadAllText(string filePath) { return System.IO.File.ReadAllText(filePath); }

        public static IEnumerable<string> SplitLines(this string text, string separator = null)
        {
            foreach (var line in StringConvert.SplitNewLines(text))
                yield return line;
        }

        public static IEnumerable<string> When(this IEnumerable<string> textEnum, Func<string, bool> when)
        {
            foreach (string line in textEnum)
                if (line != null && when(line))
                    yield return line;
        }

        public static StringWriterUtf8 StringWriterUf8() { return new StringWriterUtf8(); }
        public static StringWriterUtf8 StringWriter(StringBuilder sb) { return new StringWriterUtf8(sb); }

        // XmlHelper
        public static string ValidXName(string name) { return name.ValidXName(); }
        public static void XElWriteTo(XElement container, IO.TextWriter writer) { container.WriteTo(writer); }

        // XmlConverter
        static public string ToString(this object[] objects, string separator = " ")
        {
            if (objects.Length == 0)
                return string.Empty;
            string value = objects[0].ToString();
            if (objects.Length > 1)
            {
                StringBuilder sb = new StringBuilder(value);
                for (int i = 1; i < objects.Length; i++)
                {
                    sb.Append(separator);
                    sb.Append(objects[i].ToString());
                }
                value = sb.ToString();
            }
            return value;
        }

        static public string ToString(this IEnumerator objects, string separator = " ")
        {
            if (objects == null
                || (objects.Current == null && !objects.MoveNext())
                || objects.Current == null)
                return string.Empty;

            string value = objects.Current.ToString();
            if (objects.MoveNext())
            {
                StringBuilder sb = new StringBuilder(value);
                do
                {
                    sb.Append(separator);
                    sb.Append(objects.Current.ToString());
                } while (objects.MoveNext());
                value = sb.ToString();
            }
            return value;
        }

    }

    #region Syste.IO helpers

    public class StringWriterUtf8 : IO.StringWriter
    {
        public StringWriterUtf8(StringBuilder sb = null) : base(sb ?? new StringBuilder()) { }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }

    public class FileWrap
    {
        public bool Exists(string path) { return IO.File.Exists(path); }
        public void Delete(string path) { IO.File.Delete(path); }

        public IO.FileStream OpenRead(string path) { return IO.File.OpenRead(path); }
        public IO.FileStream OpenWrite(string path) { return IO.File.OpenWrite(path); }

        // Streams
        public IO.StreamReader OpenText(string path) { return IO.File.OpenText(path); }
    }

    public class PathWrap
    {
        public char PathSeparator { get { return IO.Path.PathSeparator; } }
        public string GetFileName(string path) { return IO.Path.GetFileName(path); }
        public string GetFileNameWithoutExtension(string path) { return IO.Path.GetFileNameWithoutExtension(path); }
        public string GetExtension(string path) { return IO.Path.GetExtension(path); }

        public string GetFullPath(string path) { return IO.Path.GetFullPath(path); }
        public string Combine(params string[] paths) { return IO.Path.Combine(paths); }
    }

    public class DirectoryWrap
    {
        public string CurrentDirectory { get { return IO.Directory.GetCurrentDirectory(); } }
        public void SetCurrentDirectory(string path) { IO.Directory.SetCurrentDirectory(path); }
        public bool Exists(string path) { return IO.Directory.Exists(path); }
        public IO.DirectoryInfo CreateDirectory(string path) { return IO.Directory.CreateDirectory(path); }
    }

    public class DateWrap
    {
        public string NowNDate()
        {
            var now = DateTime.Now.ToLocalTime();
            return string.Format("{0:0000}{1:00}{2:00}", now.Year, now.Month, now.Day);
        }

    }

    #endregion

    public static class XmlHelper
    {
        public static void WriteTo(this XElement container, IO.TextWriter writer)
        {
            if (container == null)
                return;

            using (var xmlWriter = XmlWriter.Create(writer,
                   new XmlWriterSettings { OmitXmlDeclaration = true }))
            {
                container.Save(writer);
            }
            writer.Flush();
        }

        private static char[] whiteSpaceChars = new char[] { ' ', '\t', '\n', '\r' };
        // c_EncodeCharPattern = new Regex("(?<=_)[Xx]([0-9a-fA-F]{4}|[0-9a-fA-F]{8})_");

        public static string ValidXName(this string str)
        {
            var fix = new StringBuilder(str);
            foreach (char c in whiteSpaceChars)
                fix.Replace(c, '_');
            fix.Replace("(", "");
            fix.Replace(")", "");

            string result = null;
            try
            {
                result = XName.Get(fix.ToString()).LocalName;
            }
            catch
            {
                result = XmlConvert.EncodeLocalName(fix.ToString()).ToString();
            }
            return result;
        }

    }
}
