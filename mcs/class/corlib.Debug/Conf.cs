
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Hosting;
using System.Web.Configuration;

namespace System
{
    // using _Configuration = System.Configuration.Configuration;

    public static class Conf
    {
        public static object NewConfiguration(object system, string locationSubPath = "")
        {
            object conf = null;
            // var conf = _Configuration.NewConfiguration(system: system, locationSubPath: locationSubPath);
            return conf;
        }

        public static HttpApplication CreateApplicationHost(ApplicationManager manager) 
        {
            HttpApplication app = null; // 
            app = Mono.Web.DebugWeb.CreateApplicationHost<HttpApplication>(manager);
            return app;
        }

        public static object OpenMachineConfiguration() {
            object webFactory = null; 
            // webFactory = System.Web.Configuration.WebConfigurationManager.OpenMachineConfiguration();
            return webFactory;
        }

        public static object OpenWebConfiguration(string path) {
            object cfg = null; 
            // cfg = WebConfigurationManager.OpenWebConfiguration(path);
            return cfg;
        }

    }
}