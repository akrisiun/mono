using Mono.WebServer.XSP;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;

//// e:\Beta\dot64\mono08\mcs\TestWeb\
//Severity Code    Description Project File Line    Suppression State
//Error NETSDK1047  Assets file 'E:\Beta\mono02\mono02\mcs\TestWeb\obj\project.assets.json' doesn't have a target for 
//    '.NETFramework,Version=v4.6.2/win7-x64'. Ensure that restore has run and that you have included 'net462' in the
//    TargetFrameworks for your project. You may also need to include 'win7-x64' in your project's RuntimeIdentifiers
//        <RuntimeIdentifiers>win7-x64</RuntimeIdentifiers>
//        C:\Program Files\dotnet\sdk\2.1.500\Sdks\Microsoft.NET.Sdk\targets\Microsoft.PackageDependencyResolution.targets    198	

namespace standalone_tests
{
    public class Program
    {
        public static void Main()
        {
            // & ../../../../bin/mono2.exe --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555 bin/TestWeb.exe
            // & ../../../../bin/mono2.exe --debug bin/TestWeb.exe

            // & ./testMono1/bin/testMono1.exe TestMono1/bin/TestWeb.exe --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555
            // & ./testMono1/bin/mono-sgen.exe TestMono1/bin/TestWeb.exe --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555

            // Debugger.Break();
            var text = "Hello Mono Web";
            TestStr.Test1(text);

            Exception err = null;
            Server server = null;
            try
            {
                var host = TestWeb.Test2();

                server = TestWeb.HostXsp2();

                // TestWeb.ManagerTest3();
                // TestWeb.AppTest4();

            }
            catch (Exception ex)
            {
                err = ex.InnerException ?? ex;
                Console.WriteLine($"{err}");
                Console.WriteLine($"{err.StackTrace}");
            }

            Console.ReadLine();
        }
    }

    public class TestStr
    {
        public static void Test1(string text)
        {
            try {

                System.Diagnostics.DebugMono1.Break();

                char[] arr = text.ToCharArray();
                Console.WriteLine(text);

                /*
                #1
                                Asm: System.Web, Version = 4.0.0.0, Culture = neutral, PublicKeyToken = b03f5f7f11d50a3a E:\Beta\mono02\mono02\lib\mono\4.5\System.Web.dll
                      cant resolve internal call to "System.IO.MonoIO::GetFileAttributes(string,System.IO.MonoIOError&)" (tested without signature also)
                #2
                            cant resolve internal call to "System.Runtime.InteropServices.Marshal::copy_from_unmanaged(intptr,int,System.Array,int)" (tested without signature also)
                */
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
        }
    }

    public class TestWeb
    {
        public static WebTest Test2()
        { 
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

            return test;
        }


        public static Server HostXsp2()
        {
            var manager = ApplicationManager.GetApplicationManager();
            var args = Environment.GetCommandLineArgs();

            object webFactory = null;
            object webConfig = null;
            object systemWeb = null;

            string path = AppDomain.CurrentDomain.BaseDirectory + "..\\web.config";
            path = Path.GetFullPath(path);
            try
            {
                webFactory = System.Web.Configuration.WebConfigurationManager.OpenMachineConfiguration();

                webConfig = WebConfigurationManager.OpenWebConfiguration(path);

                var sectionName = "system.web";
                systemWeb = WebConfigurationManager.GetWebApplicationSection(sectionName);
            }
            catch (Exception ex) {

                Console.WriteLine($"GetFactory: {ex}");
            }

            var server = new Server();

            bool quiet = false;
            server.DebugMain(args, true, null, quiet);

            return server;
        }

        public static TestHost ManagerTest3()
        {
            var manager = ApplicationManager.GetApplicationManager();
            IApplicationHost appHost = null;
            TestHost host = new TestHost();
            appHost = host;


            var baseDir = AppDomain.CurrentDomain.BaseDirectory;



            var type = typeof(HttpApplication);
            // http://www.west-wind.com/presentations/aspnetruntime/aspnetruntime.asp
            //  static object CreateApplicationHost(Type hostType, string virtualDir, string physicalDir)

            var app = Mono.Web.DebugWeb.CreateApplicationHost<HttpApplication>(manager);

            // domain.DoCallBack
            // public void DoCallBack(CrossAppDomainDelegate callBackDelegate)
            /*
            System.MissingMethodException: Method not found:
            System.Configuration.Internal.IInternalConfigConfigurationFactory 
                System.Configuration.ConfigurationManager.get_ConfigurationFactory()'.
                at System.Web.Configuration.WebConfigurationManager..cctor()
            
             * object ApplicationHost.CreateApplicationHost 
            appdomain.SetData (".appDomain", "*");
			int l = physicalDir.Length;
			if (physicalDir [l - 1] != Path.DirectorySeparatorChar)
				physicalDir += Path.DirectorySeparatorChar;
			appdomain.SetData (".appPath", physicalDir);
			appdomain.SetData (".appVPath", virtualDir);
			appdomain.SetData (".appId", domain_id);
			appdomain.SetData (".domainId", domain_id);
			appdomain.SetData (".hostingVirtualPath", virtualDir);
			appdomain.SetData (".hostingInstallDir", Path.GetDirectoryName (typeof (Object).Assembly.CodeBase));
			appdomain.SetData ("DataDirectory", Path.Combine (physicalDir, "App_Data"));
			appdomain.SetData (MonoHostedDataKey, "yes");
            */

            IRegisteredObject app2 = manager.CreateObject(appHost, type);
            // var httpApp = app as HttpApplication;
            ApplicationInfo[] apps = manager.GetRunningApplications();

            // BareApplicationHost CreateHost(string appId, string vpath, string ppath)
            // manager.Open();

            // manager.Close();

            return host;
        }

        public static void AppTest4()
        {
            var context = HttpContext.Current ?? new HttpContext(new WebTest("/"));

            var app = context.ApplicationInstance ?? new HttpApplication();

            var isDev = HostingEnvironment.IsDevelopmentEnvironment;
            Console.WriteLine($"IsDevelopmentEnvironment= {isDev}");

            (app as IHttpHandler).ProcessRequest(context);

            app.Dispose();
        }
    }

    public class RemoteDomain : MarshalByRefObject
    {
        public string ProcessRequest(string page, string query)
        {
            using (var sw = new StringWriter())
            {
                var  work = new SimpleWorkerRequest(page, query, sw);
                HttpRuntime.ProcessRequest(work);
                return sw.ToString();
            }
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

    public class TestHost : IApplicationHost, IConfigMapPathFactory
    {

        // Summary:
        //     Enables creation of an System.Web.Configuration.IConfigMapPath interface in the
        //     target application domain.
        // Returns:
        //     An object that is used to map virtual and physical paths of the configuration file.
        public IConfigMapPathFactory GetConfigMapPathFactory()
        {
            return this;
        }

        public IConfigMapPath Create(string virtualPath, string physicalPath)
        {
            return new TestMapPath() { PhysicalPath = physicalPath, VirtPath = virtualPath };
        }

        class TestMapPath : IConfigMapPath
        {
            public string VirtPath { get; set; }
            public string PhysicalPath { get; set; }

            //     Gets the virtual-directory name associated with a specific site.
            // Returns:
            //     The siteID must be unique. No two sites share the same id. The siteID distinguishes
            //     sites that have the same name.
            public string GetAppPathForPath(string siteID, string path) => null;

            //     Populates the default site name and the site ID.
            //   siteID:
            //     A unique identifier for the site.
            public void GetDefaultSiteNameAndID(out string siteName, out string siteID) {
                siteName = null;
                siteID = null;
            }

            //     Gets the machine-configuration file name.
            // Returns:
            //     The machine-configuration file name.
            public string GetMachineConfigFilename() => "Machine.config";

            // Summary:
            //     Populates the directory and name of the configuration file based on the site ID and site path.
            // Parameters:
            //   siteID:
            //     A unique identifier for the site.
            //   baseName:
            //     The name of the configuration file.
            public void GetPathConfigFilename(string siteID, string path, out string directory, out string baseName) {

                directory = Directory.GetCurrentDirectory();
                baseName = "web";
            }

            //     Gets the name of the configuration file at the Web root.
            // Returns:
            //     The name of the configuration file at the Web root.
            public string GetRootWebConfigFilename() => "Web.config";

            //     Gets the physical directory path based on the site ID and URL associated with the site.
            // Returns:
            //     The physical directory path.
            public string MapPath(string siteID, string path) => null;

            // Summary:
            //     Populates the site name and site ID based on a site argument value.
            //     A unique identifier for the site.
            public void ResolveSiteArgument(string siteArgument, out string siteName, out string siteID)
            {
                siteName = "1";
                siteID = "1";
            }
        }


        //     Gets the token for the application host configuration (.config) file.
        //     A Windows handle that contains the Windows security token for the application's
        //     root. The token can be used to open and read the application configuration file.
        public IntPtr GetConfigToken() => IntPtr.Zero;

        // Returns:
        //     The physical path of the application root.
        public string GetPhysicalPath() => null;

        //     Gets the site ID.
        //     The site ID.
        public string GetSiteID() => null;

        //     Gets the site name.
        // Returns:
        //     The site name.
        public string GetSiteName() => null;

        //     Gets the application's root virtual path.
        // Returns:
        //     The application's root virtual path.
        public string GetVirtualPath() => "/";

        // Summary:
        //     Indicates that a message was received.
        public void MessageReceived() { }
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
