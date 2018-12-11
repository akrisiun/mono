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
            // & ../../../../bin/mono2.exe --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555 bin/TestWeb.exe
            // & ../../../../bin/mono2.exe --debug bin/TestWeb.exe
            
            // & ./testMono1/bin/testMono1.exe TestMono1/bin/TestWeb.exe --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555
            // & ./testMono1/bin/mono-sgen.exe TestMono1/bin/TestWeb.exe --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555

            // Debugger.Break();
            var text = "Hello Mono Web";

            try {

                System.Diagnostics.DebugMono1.Break();

                char[] arr = text.ToCharArray();
                Console.WriteLine(text);

                var typDebug = typeof(System.Diagnostics.Debug);
                // var p = typDebug.GetProperty("IsDebug", BindingFlags.Static | BindingFlags.Public);
                // p?.SetValue(null, true);
                // System.Diagnostics.Debug.IsDebug = true;

                // Console.WriteLine
                Debugger.Log(0, "", text);
                // System.Diagnostics.Debug.WriteLine("So debug?");
            }
            catch (Exception ex) {
                var arr2 = ex.Message.ToCharArray();
                // corlib.Debug.
                Console.WriteLine(arr2);
            }

            // GetModuleVersionId
            // var spanText = System.MemoryExtensionsDebug.AsSpanWrap(text);

            // C:\WINDOWS\Microsoft.Net\assembly\GAC_64\System.Web\v4.0_4.0.0
            // dot build E:\Beta\mono02\mono02\mcs\class\System.Web\System.Web-net_4_x.csproj
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
    /*
    System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Unhandled Exception: System.MethodAccessException: Attempt 
by security transparent method 'System.Web.Hosting.SimpleWorkerRequest..ctor(System.String, System.String, System.IO.TextWriter)' to access LinkDemand protected method 'System.Web.HttpWorkerRequest..ctor()' failed.  Methods must be security critical or security safe-critical to satisfy a LinkDemand.
   at System.Web.Hosting.SimpleWorkerRequest..ctor(String page, String query, TextWriter output) 
   in E:\Beta\mono02\mono02\mcs\class\System.Web\System.Web.Hosting\SimpleWorkerRequest.cs:line 66
   at standalone_tests.WebTest..ctor(String page, String query)
   at standalone_tests.Class.Main()
   */
            Console.WriteLine(line);

            var test = new WebTest("/");
            try {

                System.Diagnostics.DebugMono1.BreakWeb();

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

/*Unhandled Exception:
  System.TypeLoadException: Invalid type System.Web.Configuration.HttpRuntimeSection for instance field System.Web.HttpRuntime:runtime_section
  at standalone_tests.WebTest..ctor (System.String page, System.String query) [0x00000] in E:\Beta\mono02\mono02\mcs\TestWeb\Main.cs:114
  at standalone_tests.Class.Main () [0x00100] in E:\Beta\mono02\mono02\mcs\TestWeb\Main.cs:86
  */
        public WebTest(string page, string query = "?") : base(page ?? "/", query, (Writer = new StringWriter()))
        {
        }

        public HttpContext Context { get; set; }
    }
}

namespace System.Diagnostics
{
    using System.Diagnostics.Private;
    using System.Web;

    public static class DebugMono1
    {
        public static bool IsDebug { get; set; }

        public static void Break()
        {
            // DebugMono2.Break();
            DebugMono2.IsDebug = true;

            DebugMono2.WriteLine("Hello corlib.Debug");
            DebugMono2.WriteLine($"corlib.Debug : {typeof(DebugMono2).Assembly.Location}");
            DebugMono2.WriteLine($"mscorlib   : {typeof(System.Object).Assembly.Location}");
            DebugMono2.WriteLine($"System.Xml : {typeof(System.Xml.Formatting).Assembly.Location}");
        }

        public static void BreakWeb()
        {
            // DebugMono2.WriteLine($"System.Web : {typeof(Mono.Web.Util.SettingsMappingManager).Assembly.Location}");
            DebugMono2.WriteLine($"System.Web : {typeof(System.Web.HttpContext).Assembly.Location}");
            // corlib.Debug.Console.Break();
            // corlib.Debug.Console.Load();
            // corlib.Debug.Console.WriteLine(arr);

        }
    }
}
