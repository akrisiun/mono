using System;
using System.Collections.Specialized;
using System.Diagnostics;
#if !NETCORE30
using System.Configuration;
#endif

namespace Mono
{
    public static class AppConfig
    {

//#if !NETCORE30
//        public static NameValueCollection AppSettings
//        { [DebuggerStepThrough] get { return ConfigurationManager.AppSettings; } }

//#if NET46 || NETCORE20
//        // #if $(OS)' != 'Windows_NT'
//        public static ConnectionStringSettingsCollection ConnectionStrings
//        { [DebuggerStepThrough] get { return ConfigurationManager.ConnectionStrings; } }
//#endif
//#endif

#if (WEB || WPF) && !NETCORE30
        private static WebCfg _web = null;
        public static WebCfg Web { get {  return _web ?? (_web = new WebCfg()); } }

        public class WebCfg
        {
            //public T Server<T>(string key = "web.server") where T : class
            //{ return (T)ConfigurationManager.GetSection(key); }

            //public T Config<T>(string key = "web.config") where T : class
            //{ return (T)ConfigurationManager.GetSection(key); }
        }
#endif

    }
}

namespace Mono.Entity
{
    public interface ILastError
    {
        Exception LastError { get; set; }
    }
}
