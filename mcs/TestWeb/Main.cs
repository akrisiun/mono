using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Hosting;

// e:\Beta\dot64\mono08\mcs\TestWeb\

namespace standalone_tests
{
    public class Class
    {
        public static void Main()
        {
            // & ../../../../bin/mono8.exe --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555 bin/TestWeb.exe
            
            // & ./testMono1/bin/testMono1.exe TestMono1/bin/TestWeb.exe --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555
            // & ./testMono1/bin/mono-sgen.exe TestMono1/bin/TestWeb.exe --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555

            Debugger.Break();

            var text = "Hello Mono Web";

            try {
                corlib.Debug.Console.Break();
                corlib.Debug.Console.Load();

                char[] arr = text.ToCharArray();
                corlib.Debug.Console.WriteLine(arr);
                Console.WriteLine(text);

                var typDebug = typeof(System.Diagnostics.Debug);
                var p = typDebug.GetProperty("IsDebug", BindingFlags.Static | BindingFlags.Public);
                p?.SetValue(null, true);
                // System.Diagnostics.Debug.IsDebug = true;

                // Console.WriteLine
                Debugger.Log(0, "", text);
                System.Diagnostics.Debug.WriteLine("So debug?");
            }
            catch (Exception ex) {
                var arr2 = ex.Message.ToCharArray();
                corlib.Debug.Console.WriteLine(arr2);
            }

            // GetModuleVersionId
            // var spanText = System.MemoryExtensionsDebug.AsSpanWrap(text);

            // C:\WINDOWS\Microsoft.Net\assembly\GAC_64\System.Web\v4.0_4.0.0
            Assembly asmWeb = typeof(SimpleWorkerRequest).Assembly;
 
            var line = $"Asm: {asmWeb.FullName} {asmWeb.Location}";
            var trace = Environment.StackTrace;
            Console.WriteLine(trace);

            var consolType = typeof(System.Console);
            Stream sout = System.Console.OpenStandardOutput (1024);
            var m = new MemoryStream();
            var str = new StringWriter();
            str.WriteLine("hello mem");
            var s2 = str.ToString();
            var b2= System.Text.UTF8Encoding.Default.GetBytes(s2);
            m.Write(b2, 0, b2.Length);
            m.Position = 0;
            m.WriteTo(sout);

            m.Flush();
            m.Dispose();

            // System.Diagnostics.Private
            // System.Diagnostics.Debug.Fail(line);

            Console.WriteLine(line);

            var test = new WebTest("/");
            try {
                if (Debugger.IsAttached)
                    Debugger.Break();


                HttpContext.Current = new HttpContext(test);

                test.Context = HttpContext.Current;
                test.SendStatus(200, "OK");
            }
            catch (Exception ex) {
                test.LastError = ex;
            }
            if (test.LastError != null)
                Console.WriteLine($"Errors {test.LastError}");
            else
                Console.WriteLine($"Web test success!");

            Console.ReadLine();
        }
    }

    public class WebTest : SimpleWorkerRequest
    {
        public static StringWriter  Writer { get; set; }
        public Exception LastError { get; set; }

        public WebTest(string page, string query = "?") : base(page ?? "/", query, (Writer = new StringWriter()))
        {
        }

        public HttpContext Context { get; set; }
    }
}
