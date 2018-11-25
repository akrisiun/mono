using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Hosting;

namespace standalone_tests
{
    public class Class
    {
        // [STAThread]
        public static void Main()
        {
            // Loaded from: C:\Program Files\Mono\lib\mono\4.5\mscorlib.dll
            // corlib.dll expected - 1050400003
            // dotnet msbuild -p:Platform=net_4_x
            // E:\Beta\mono64\mono04\mcs\build\common\Consts.cs
            // MonoCorlibVersion = 1050400003; // @MONO_CORLIB_VERSION@;
            // $env:MONO_CORLIB_VERSION=1050400003

            Console.WriteLine("Hello Mono Web"); 
            Console.ReadKey();

            Assembly asmWeb = typeof(SimpleWorkerRequest).Assembly;

            Console.WriteLine($"Asm: {asmWeb.FullName} {asmWeb.Location}");

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
