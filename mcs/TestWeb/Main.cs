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
            // "c:\Program Files\Mono\bin\mono-sgen.exe" TestWeb/bin/net48/TestWeb.exe
            // 1051400006
            // mono --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:51478 TestWeb/bin/net48/TestWeb.exe


            // Loaded from: C:\Program Files\Mono\lib\mono\4.5\mscorlib.dll
            // corlib.dll expected - 1050400003
            // dotnet msbuild -p:Platform=net_4_x
            // E:\Beta\mono64\mono04\mcs\build\common\Consts.cs
            // MonoCorlibVersion = 1050400003; // @MONO_CORLIB_VERSION@;
            // $env:MONO_CORLIB_VERSION=1051400006

            // Console.WriteLine
            var text = "Hello Mono Web";
            Debugger.Log(0, "", text); 

            // var spanText = System.MemoryExtensionsDebug.AsSpanWrap(text);

            // Assertion at ..\mono\mini\method-to-ir.c:13203, condition `ins->opcode >= MONO_CEE_LAST' not met
            // E:\Beta\mono64\mono04\msvc\build\sgen\x64\bin\Debug\mono-sgen.exe
            // E:\Beta\mono64\mono04\msvc\build\sgen\x64\bin\Debug\

            // ..\bin\mono --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555 TestWeb/bin/net48/TestWeb.exe

            Assembly asmWeb = typeof(SimpleWorkerRequest).Assembly;

            var line = $"Asm: {asmWeb.FullName} {asmWeb.Location}";

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
