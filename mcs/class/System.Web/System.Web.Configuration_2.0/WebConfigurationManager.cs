//
// System.Web.Configuration.WebConfigurationManager.cs
//
// Authors:
// 	Lluis Sanchez Gual (lluis@novell.com)
// 	Chris Toshok (toshok@ximian.com)
//      Marek Habersack <mhabersack@novell.com>
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (C) 2005-2009 Novell, Inc (http://www.novell.com)
//


using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using Mono.Web.Util;
using System.Xml;
using System.Configuration;
using System.Configuration.Internal;
using _Configuration = System.Configuration.Configuration;
using System.Web.Util;
using System.Threading;
using System.Web.Hosting;
using System.Diagnostics;

namespace System.Web.Configuration {

    // using Debug = System.Diagnostic.DebugMono;

    // System.Web.Configuration.WebConfigurationManager.GetFactory()
    public class WebConfigurationManager
	{
		sealed class ConfigPath 
		{
			public string Path;
			public bool InAnotherApp;

			public ConfigPath (string path, bool inAnotherApp)
			{
				this.Path = path;
				this.InAnotherApp = inAnotherApp;
			}
		}
		
		const int SAVE_LOCATIONS_CHECK_INTERVAL = 6000; // milliseconds
		const int SECTION_CACHE_LOCK_TIMEOUT = 200; // milliseconds

		static readonly char[] pathTrimChars = { '/' };
		static readonly object suppressAppReloadLock = new object ();
		static readonly object saveLocationsCacheLock = new object ();
		static readonly object getSectionLock = new object ();
		
		// See comment for the cacheLock field at top of System.Web.Caching/Cache.cs
		static readonly ReaderWriterLockSlim sectionCacheLock;

		static object // IInternalConfigConfigurationFactory 
               configFactory;

		static Hashtable configurations = Hashtable.Synchronized (new Hashtable ());
		static Hashtable configPaths = Hashtable.Synchronized (new Hashtable ());
		static bool suppressAppReload;
		static Dictionary <string, DateTime> saveLocationsCache;
		static Timer saveLocationsTimer;
		
		static ArrayList extra_assemblies = null;
		static internal ArrayList ExtraAssemblies {
			get {
				if (extra_assemblies == null)
					extra_assemblies = new ArrayList();
				return extra_assemblies;
			}
		}

		const int DEFAULT_SECTION_CACHE_SIZE = 100;
		const string CACHE_SIZE_OVERRIDING_KEY = "MONO_ASPNET_WEBCONFIG_CACHESIZE";
		static LruCache<int, object> sectionCache;
		
        public static Exception CreateError { get; set; }

        static WebConfigurationManager()
        {
            DebugMono.Break();

            sectionCacheLock = new ReaderWriterLockSlim();
            Instance = new WebConfigurationManager();
        }

        public static WebConfigurationManager Instance;

        public WebConfigurationManager()
        {
            var section_cache_size = DEFAULT_SECTION_CACHE_SIZE;
            int section_cache_size_override;
            bool size_overriden = false;
            try
            {
                if (int.TryParse(Environment.GetEnvironmentVariable(CACHE_SIZE_OVERRIDING_KEY), out section_cache_size_override))
                {
                    section_cache_size = section_cache_size_override;
                    size_overriden = true;
                    Console.WriteLine("WebConfigurationManager's LRUcache Size overriden to: {0} (via {1})", section_cache_size_override, CACHE_SIZE_OVERRIDING_KEY);
                }
                sectionCache = new LruCache<int, object>(section_cache_size);
                string eviction_warning = "WebConfigurationManager's LRUcache evictions count reached its max size";
                if (!size_overriden)
                    eviction_warning += String.Format("{0}Cache Size: {1} (overridable via {2})",
                                                       Environment.NewLine, section_cache_size, CACHE_SIZE_OVERRIDING_KEY);
                sectionCache.EvictionWarning = eviction_warning;

                GetFactory();

                // Part of fix for bug #491531
                Type type = Type.GetType("System.Configuration.CustomizableFileSettingsProvider, System", false);
                if (type != null)
                {
                    FieldInfo fi = type.GetField("webConfigurationFileMapType", BindingFlags.Static | BindingFlags.NonPublic);
                    if (fi != null && fi.FieldType == Type.GetType("System.Type"))
                        fi.SetValue(null, typeof(ApplicationSettingsConfigurationFileMap));
                }
            }
            catch (Exception ex) {
                CreateError = ex;
            }

		}

        public static object GetFactory()
        {
            try
            {
                var x = ConfigurationFactory;

                configFactory = ConfigurationManager.ConfigurationFactory2;
                _Configuration.SaveStart += ConfigurationSaveHandler;
                _Configuration.SaveEnd += ConfigurationSaveHandler;

            }
            catch (Exception e1) {
                CreateError = e1;
            }

            return configFactory;
        }

        static void ReenableWatcherOnConfigLocation (object state)
		{
			string path = state as string;
			if (String.IsNullOrEmpty (path))
				return;

			DateTime lastWrite;
			lock (saveLocationsCacheLock) {
				if (!saveLocationsCache.TryGetValue (path, out lastWrite))
					lastWrite = DateTime.MinValue;
			}

			DateTime now = DateTime.Now;
			if (lastWrite == DateTime.MinValue || now.Subtract (lastWrite).TotalMilliseconds >= SAVE_LOCATIONS_CHECK_INTERVAL) {
				saveLocationsTimer.Dispose ();
				saveLocationsTimer = null;
				HttpApplicationFactory.EnableWatcher (VirtualPathUtility.RemoveTrailingSlash (HttpRuntime.AppDomainAppPath), "?eb.?onfig");
			} else
				saveLocationsTimer.Change (SAVE_LOCATIONS_CHECK_INTERVAL, SAVE_LOCATIONS_CHECK_INTERVAL);
		}
		
		static void ConfigurationSaveHandler(object senderObj, ConfigurationSaveEventArgs args)
		{
            _Configuration sender = senderObj as _Configuration;

            try {
				sectionCacheLock.EnterWriteLock ();
				sectionCache.Clear ();
			} finally {
				sectionCacheLock.ExitWriteLock ();
			}
			
			lock (suppressAppReloadLock) {

				string rootConfigPath = WebConfigurationHost.GetWebConfigFileName (HttpRuntime.AppDomainAppPath);

				if (String.Compare (args.StreamPath, rootConfigPath, StringComparison.OrdinalIgnoreCase) == 0) {
					SuppressAppReload (args.Start);
					if (args.Start) {
						HttpApplicationFactory.DisableWatcher (VirtualPathUtility.RemoveTrailingSlash (HttpRuntime.AppDomainAppPath), "?eb.?onfig");

						lock (saveLocationsCacheLock) {
							if (saveLocationsCache == null)
								saveLocationsCache = new Dictionary <string, DateTime> (StringComparer.Ordinal);
							if (saveLocationsCache.ContainsKey (rootConfigPath))
								saveLocationsCache [rootConfigPath] = DateTime.Now;
							else
								saveLocationsCache.Add (rootConfigPath, DateTime.Now);

							if (saveLocationsTimer == null)
								saveLocationsTimer = new Timer (ReenableWatcherOnConfigLocation,
												rootConfigPath,
												SAVE_LOCATIONS_CHECK_INTERVAL,
												SAVE_LOCATIONS_CHECK_INTERVAL);
						}
					}
				}
			}
		}
		
		public static _Configuration OpenMachineConfiguration ()
		{
			return ConfigurationManager.OpenMachineConfiguration ();
		}
		
		[MonoLimitation ("locationSubPath is not handled")]
		public static _Configuration OpenMachineConfiguration (string locationSubPath)
		{
			return OpenMachineConfiguration ();
		}

		[MonoLimitation("Mono does not support remote configuration")]
		public static _Configuration OpenMachineConfiguration (string locationSubPath,
								       string server)
		{
			if (server == null)
				return OpenMachineConfiguration (locationSubPath);

			throw new NotSupportedException ("Mono doesn't support remote configuration");
		}

		[MonoLimitation("Mono does not support remote configuration")]
		public static _Configuration OpenMachineConfiguration (string locationSubPath,
								       string server,
								       IntPtr userToken)
		{
			if (server == null)
				return OpenMachineConfiguration (locationSubPath);
			throw new NotSupportedException ("Mono doesn't support remote configuration");
		}

		[MonoLimitation("Mono does not support remote configuration")]
		public static _Configuration OpenMachineConfiguration (string locationSubPath,
								       string server,
								       string userName,
								       string password)
		{
			if (server == null)
				return OpenMachineConfiguration (locationSubPath);
			throw new NotSupportedException ("Mono doesn't support remote configuration");
		}

		public static _Configuration OpenWebConfiguration (string path)
		{
			return OpenWebConfiguration (path, null, null, null, null, null);
		}
		
		public static _Configuration OpenWebConfiguration (string path, string site)
		{
			return OpenWebConfiguration (path, site, null, null, null, null);
		}
		
		public static _Configuration OpenWebConfiguration (string path, string site, string locationSubPath)
		{
			return OpenWebConfiguration (path, site, locationSubPath, null, null, null);
		}

		public static _Configuration OpenWebConfiguration (string path, string site, string locationSubPath, string server)
		{
			return OpenWebConfiguration (path, site, locationSubPath, server, null, null);
		}

		public static _Configuration OpenWebConfiguration (string path, string site, string locationSubPath, string server, IntPtr userToken)
		{
			return OpenWebConfiguration (path, site, locationSubPath, server, null, null);
		}
		
		public static _Configuration OpenWebConfiguration (string path, string site, string locationSubPath, string server, string userName, string password)
		{
			return OpenWebConfiguration (path, site, locationSubPath, server, null, null, false);
		}

		static _Configuration OpenWebConfiguration (string path, string site, string locationSubPath, 
               string server, string userName, string password, bool fweb)
		{
			if (String.IsNullOrEmpty (path))
				path = "/";

			bool inAnotherApp = false;
			if (!fweb && !String.IsNullOrEmpty (path))
				path = FindWebConfig (path, out inAnotherApp);

			string confKey = path + site + locationSubPath + server + userName + password;
			_Configuration conf = null;
            DebugMono.Break(); // TODO
            try
            {
                if (configurations.Count > 0)
                {
                    conf = configurations[confKey] as _Configuration;
                }
            }
            catch { }

            string locationSub = "";
            if (conf == null) {
                try
                {
                    var typeConfigHost = typeof(WebConfigurationHost);
                    object[] hostInitConfigurationParams = null;
                    System.Array.Resize(ref hostInitConfigurationParams, 10);

                    hostInitConfigurationParams[1] = AppDomain.CurrentDomain.BaseDirectory;  // fullPath 
                    hostInitConfigurationParams[0] = new WebConfigurationFileMap(); //  map
                    hostInitConfigurationParams[7] = false; // inAnotherApp = (bool)

                    var factory = new InternalConfigConfigurationFactory();

                    conf = factory.Create(typeConfigHost, hostInitConfigurationParams);

                    if (conf != null)
                    {
                        configurations[confKey] = conf;
                    }
                }
                catch { }
            }
            return conf;
		}

		public static _Configuration OpenMappedWebConfiguration (WebConfigurationFileMap fileMap, string path)
		{
            DebugMono.Break(); // TODO
            return ConfigurationFactory.Create (typeof(WebConfigurationHost), fileMap, path);
		}
		
		public static _Configuration OpenMappedWebConfiguration (WebConfigurationFileMap fileMap, string path, string site)
		{
            DebugMono.Break(); // TODO
            return ConfigurationFactory.Create (typeof(WebConfigurationHost), fileMap, path, site);
		}
		
		public static _Configuration OpenMappedWebConfiguration (WebConfigurationFileMap fileMap, string path, string site, string locationSubPath)
		{
            DebugMono.Break(); // TODO
            return ConfigurationFactory.Create (typeof(WebConfigurationHost), fileMap, path, site, locationSubPath);
		}
		
		public static _Configuration OpenMappedMachineConfiguration (ConfigurationFileMap fileMap)
		{
            DebugMono.Break(); // TODO
            return ConfigurationFactory.Create (typeof(WebConfigurationHost), fileMap);
		}


		public static _Configuration OpenMappedMachineConfiguration (ConfigurationFileMap fileMap,
									     string locationSubPath)
		{
			return OpenMappedMachineConfiguration (fileMap);
		}

		internal static object SafeGetSection (string sectionName, Type configSectionType)
		{
			try {
				return GetSection (sectionName);
			} catch (Exception) {
				if (configSectionType != null)
					return Activator.CreateInstance (configSectionType);
				return null;
			}
		}
		
		internal static object SafeGetSection (string sectionName, string path, Type configSectionType)
		{
			try {
				return GetSection (sectionName, path);
			} catch (Exception) {
				if (configSectionType != null)
					return Activator.CreateInstance (configSectionType);
				return null;
			}
		}
		
		public static object GetSection (string sectionName)
		{
			HttpContext context = HttpContext.Current;

            // C:\Windows\Microsoft.NET\Framework64\v4.0.30319\config\web.config
            if (sectionName == "system.web/compilation")
            {
                var dom = AppDomain.CurrentDomain;
                var c = dom.GetData(sectionName) as CompilationSection;
                if (c == null)
                {
                    c = new CompilationSection { Debug = true };
                    dom.SetData(sectionName, c);
                }
                return c;
            }

            return GetSection (sectionName, GetCurrentPath (context), context);
		}

		public static object GetSection (string sectionName, string path)
		{
			var obj = GetSection (sectionName, path, HttpContext.Current);
            return obj; // debug?
		}

		static bool LookUpLocation (string relativePath, ref _Configuration defaultConfiguration)
		{
			if (String.IsNullOrEmpty (relativePath))
				return false;

			_Configuration cnew = defaultConfiguration.FindLocationConfiguration (relativePath, defaultConfiguration);
			if (cnew == defaultConfiguration)
				return false;

			defaultConfiguration = cnew;
			return true;
		}
		
		internal static object GetSection (string sectionName, string path, HttpContext context)
		{
			if (String.IsNullOrEmpty (sectionName))
				return null;
			
			_Configuration c = OpenWebConfiguration (path, null, null, null, null, null, false);

            /// $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
            string configPath = c.FilePath; // not: c.ConfigPath;
			int baseCacheKey = 0;
			int cacheKey;
			bool pathPresent = !String.IsNullOrEmpty (path);
			string locationPath = null;

			if (pathPresent)
				locationPath = "location_" + path;
			
			baseCacheKey = sectionName.GetHashCode ();
			if (configPath != null)
				baseCacheKey ^= configPath.GetHashCode ();
			
			try {
				sectionCacheLock.EnterWriteLock ();
				
				object o;
				if (pathPresent) {
					cacheKey = baseCacheKey ^ locationPath.GetHashCode ();
					if (sectionCache.TryGetValue (cacheKey, out o))
						return o;
				
					cacheKey = baseCacheKey ^ path.GetHashCode ();
					if (sectionCache.TryGetValue (cacheKey, out o))
						return o;
				}
				
				if (sectionCache.TryGetValue (baseCacheKey, out o))
					return o;
			} finally {
				sectionCacheLock.ExitWriteLock ();
			}

			string cachePath = null;
			if (pathPresent) {
				string relPath;
				
				if (VirtualPathUtility.IsRooted (path)) {
					if (path [0] == '~')
						relPath = path.Length > 1 ? path.Substring (2) : String.Empty;
					else if (path [0] == '/')
						relPath = path.Substring (1);
					else
						relPath = path;
				} else
					relPath = path;

				HttpRequest req = context != null ? context.Request : null;
				if (req != null) {
					string vdir = VirtualPathUtility.GetDirectory (req.PathNoValidation);
					if (vdir != null) {
						vdir = vdir.TrimEnd (pathTrimChars);
						if (String.Compare (c.FilePath // .ConfigPath
                                , vdir, StringComparison.Ordinal) != 0 && LookUpLocation (vdir.Trim (pathTrimChars), ref c))
							cachePath = path;
					}
				}
				
				if (LookUpLocation (relPath, ref c))
					cachePath = locationPath;
				else
					cachePath = path;
			}

			ConfigurationSection section = null;
			lock (getSectionLock) {
                try
                {
                    section = c.GetSection(sectionName);
                }
                catch { } 
			}
			if (section == null)
				return null;

			object value = SettingsMappingManager.MapSection (section.GetRuntimeObject ());
			if (cachePath != null)
				cacheKey = baseCacheKey ^ cachePath.GetHashCode ();
			else
				cacheKey = baseCacheKey;
			
			AddSectionToCache (cacheKey, value);
			return value;
		}
		
		static string MapPath (HttpRequest req, string virtualPath)
		{
			if (req != null)
				return req.MapPath (virtualPath);

			string appRoot = HttpRuntime.AppDomainAppVirtualPath;
			if (!String.IsNullOrEmpty (appRoot) && virtualPath.StartsWith (appRoot, StringComparison.Ordinal)) {
				if (String.Compare (virtualPath, appRoot, StringComparison.Ordinal) == 0)
					return HttpRuntime.AppDomainAppPath;
				return UrlUtils.Combine (HttpRuntime.AppDomainAppPath, virtualPath.Substring (appRoot.Length));
			}
			
			return null;
		}

		static string GetParentDir (string rootPath, string curPath)
		{
			int len = curPath.Length - 1;
			if (len > 0 && curPath [len] == '/')
				curPath = curPath.Substring (0, len);

			if (String.Compare (curPath, rootPath, StringComparison.Ordinal) == 0)
				return null;
			
			int idx = curPath.LastIndexOf ('/');
			if (idx == -1)
				return curPath;

			if (idx == 0)
				return "/";
			
			return curPath.Substring (0, idx);
		}

		internal static string FindWebConfig (string path)
		{
			bool dummy;

			return FindWebConfig (path, out dummy);
		}
		
		internal static string FindWebConfig (string path, out bool inAnotherApp)
		{
			inAnotherApp = false;
			
			if (String.IsNullOrEmpty (path))
				return path;
				
			if (HostingEnvironment.VirtualPathProvider != null) {
				if (HostingEnvironment.VirtualPathProvider.DirectoryExists (path))
					path = VirtualPathUtility.AppendTrailingSlash (path);
			}
				
			
			string rootPath = HttpRuntime.AppDomainAppVirtualPath;
			ConfigPath curPath = null;
            if (configPaths.Count > 0)
            {
                curPath = configPaths[path] as ConfigPath;
            }
			if (curPath != null) {
				inAnotherApp = curPath.InAnotherApp;
				return curPath.Path;
			}
			
			HttpContext ctx = HttpContext.Current;
			HttpRequest req = ctx != null ? ctx.Request : null;
			string physPath = req != null ? VirtualPathUtility.AppendTrailingSlash (MapPath (req, path)) : null;
			string appDomainPath = HttpRuntime.AppDomainAppPath;
			
			if (physPath != null && appDomainPath != null && !physPath.StartsWith (appDomainPath, StringComparison.Ordinal))
				inAnotherApp = true;
			
			string dir;
			if (inAnotherApp || path [path.Length - 1] == '/')
				dir = path;
			else {
			 	dir = VirtualPathUtility.GetDirectory (path, false);
			 	if (dir == null)
			 		return path;
			}

            if (configPaths.Count > 0)
            {
                curPath = configPaths[dir] as ConfigPath;
            }
			if (curPath != null) {
				inAnotherApp = curPath.InAnotherApp;
				return curPath.Path;
			}
			
			if (req == null)
				return path;

			curPath = new ConfigPath (path, inAnotherApp);
            if (rootPath == null)
            {
                rootPath = "/";
            }
            int iMax = 10;
			while (String.Compare (curPath.Path, rootPath, StringComparison.Ordinal) != 0) {

                physPath = MapPath (req, curPath.Path);
                iMax--;
                if (physPath == null || iMax == 0) {
					curPath.Path = rootPath;
					break;
				}

				if (WebConfigurationHost.GetWebConfigFileName (physPath) != null)
					break;
				
				curPath.Path = GetParentDir (rootPath, curPath.Path);
				if (curPath.Path == null || curPath.Path == "~") {
					curPath.Path = rootPath;
					break;
				}
			}

            if (configPaths.Count > 0)
            {
                if (string.Compare(curPath.Path, path, StringComparison.Ordinal) != 0)
                    configPaths[path] = curPath;
                else
                    configPaths[dir] = curPath;
            }
			
			return curPath.Path;
		}
		
		static string GetCurrentPath (HttpContext ctx)
		{
			HttpRequest req = ctx != null ? ctx.Request : null;
			return req != null ? req.PathNoValidation : HttpRuntime.AppDomainAppVirtualPath;
		}
		
		internal static bool SuppressAppReload (bool newValue)
		{
			bool ret;
			
			lock (suppressAppReloadLock) {
				ret = suppressAppReload;
				suppressAppReload = newValue;
			}

			return ret;
		}
		
		internal static void RemoveConfigurationFromCache (HttpContext ctx)
		{
			configurations.Remove (GetCurrentPath (ctx));
		}

        // System.MissingMethodException: Method not found: 
        // System.Configuration.Internal.IInternalConfigConfigurationFactory System.Configuration.ConfigurationManager.get_ConfigurationFactory()'.
        // at System.Web.Configuration.WebConfigurationManager..cctor()

        // System.Configuration.Internal.IInternalConfigConfigurationFactory ConfigurationFactory()
        // Get the factory used to create and initialize Configuration objects.

        public static string InternalConfigConfigurationFactoryTypeName = "System.Configuration.Internal.InternalConfigConfigurationFactory";
        private static IInternalConfigConfigurationFactory s_configurationFactory;

        static internal IInternalConfigConfigurationFactory ConfigurationFactory {
            // [ReflectionPermission(SecurityAction.Assert, Flags = ReflectionPermissionFlag.MemberAccess)]
            get {

                if (s_configurationFactory == null)
                {

                    Type type = typeof(InternalConfigConfigurationFactory);
                    // Type type = Type.GetType(InternalConfigConfigurationFactoryTypeName, true);

                    s_configurationFactory = (IInternalConfigConfigurationFactory)Activator.CreateInstance(type, true);
                }

                return s_configurationFactory;
            }
        }

        public static object GetWebApplicationSection (string sectionName)
		{
			HttpContext ctx = HttpContext.Current;
			HttpRequest req = ctx != null ? ctx.Request : null;
			string applicationPath = req != null ? req.ApplicationPath : null;
            var path = string.IsNullOrEmpty(applicationPath) ? string.Empty : applicationPath;

            var domain = AppDomain.CurrentDomain;
            if (sectionName == "system.web/compilation")
            {
                var c = domain.GetData(sectionName) as CompilationSection;
                if (c == null)
                {
                    c = new CompilationSection { Debug = true };
                    domain.SetData(sectionName, c);
                }
                if (c != null)
                {
                    return c;
                }
            }
            object obj = null;
            obj = domain.GetData(sectionName);
            if (obj == null)
            { 
                obj = GetSection (sectionName, path);
                if (obj != null)
                {
                    domain.SetData(sectionName, obj);
                }
            }

            return obj;
		}

		public static NameValueCollection AppSettings {
			get { return ConfigurationManager.AppSettings; }
		}

		public static ConnectionStringSettingsCollection ConnectionStrings {
			get { return ConfigurationManager.ConnectionStrings; }
		}

		internal static IInternalConfigConfigurationFactory ConfigurationFactory1 {
			get { return configFactory as IInternalConfigConfigurationFactory; }
		}

		static void AddSectionToCache (int key, object section)
		{
			object cachedSection;

			bool locked = false;
			try {
				if (!sectionCacheLock.TryEnterWriteLock (SECTION_CACHE_LOCK_TIMEOUT))
					return;
				locked = true;

				if (sectionCache.TryGetValue (key, out cachedSection) && cachedSection != null)
					return;

				sectionCache.Add (key, section);
			} finally {
				if (locked) {
					sectionCacheLock.ExitWriteLock ();
				}
			}
		}
		
#region stuff copied from WebConfigurationSettings
		static internal IConfigurationSystem oldConfig;
		static Web20DefaultConfig config;
		//static IInternalConfigSystem configSystem;
		const BindingFlags privStatic = BindingFlags.NonPublic | BindingFlags.Static;
		static readonly object lockobj = new object ();

		internal static void Init ()
		{
			lock (lockobj) {
				if (config != null)
					return;

				/* deal with the ConfigurationSettings stuff */
				{
					Web20DefaultConfig settings = Web20DefaultConfig.GetInstance ();
					Type t = typeof (ConfigurationSettings);
					MethodInfo changeConfig = t.GetMethod ("ChangeConfigurationSystem",
									       privStatic);

					if (changeConfig == null)
						throw new ConfigurationException ("Cannot find method CCS");

					object [] args = new object [] {settings};
					oldConfig = (IConfigurationSystem)changeConfig.Invoke (null, args);
					config = settings;

					config.Init ();
				}

				/* deal with the ConfigurationManager stuff */
				{
					HttpConfigurationSystem system = new HttpConfigurationSystem ();
					Type t = typeof (ConfigurationManager);
					MethodInfo changeConfig = t.GetMethod ("ChangeConfigurationSystem",
									       privStatic);

					if (changeConfig == null)
						throw new ConfigurationException ("Cannot find method CCS");

					object [] args = new object [] {system};
					changeConfig.Invoke (null, args);
					//configSystem = system;
				}
			}
		}
	}

	class Web20DefaultConfig : IConfigurationSystem
	{
		static Web20DefaultConfig instance;

		static Web20DefaultConfig ()
		{
			instance = new Web20DefaultConfig ();
		}

		public static Web20DefaultConfig GetInstance ()
		{
			return instance;
		}

		public object GetConfig (string sectionName)
		{
			object o = WebConfigurationManager.GetWebApplicationSection (sectionName);

			if (o == null || o is IgnoreSection) {
				/* this can happen when the section
				 * handler doesn't subclass from
				 * ConfigurationSection.  let's be
				 * nice and try to load it using the
				 * 1.x style routines in case there's
				 * a 1.x section handler registered
				 * for it.
				 */
				object o1 = WebConfigurationManager.oldConfig.GetConfig (sectionName);
				if (o1 != null)
					return o1;
			}

			return o;
		}

		public void Init ()
		{
			// nothing. We need a context.
		}
	}
#endregion
}

// System.MissingMethodException: Method not found: 'System.Configuration.Internal.IInternalConfigConfigurationFactory
namespace System.Configuration.Internal
{
    using System.Configuration;
    using System.Web.Configuration;

    //
    // Call into System.Configuration.dll to create and initialize a Configuration object.
    //
    [System.Runtime.InteropServices.ComVisible(false)]
    public interface IInternalConfigConfigurationFactory
    {
        // ClassConfiguration Create2(Type typeConfigHost, params object[] hostInitConfigurationParams);

        Configuration Create(Type typeConfigHost, params object[] hostInitConfigurationParams);

        string NormalizeLocationSubPath(string subPath, IConfigErrorInfo errorInfo);
    }

    class InternalConfigurationSystemWeb : IConfigSystem
    {
        IInternalConfigHost host;
        IInternalConfigRoot root;
        object[] hostInitParams;

        public void Init(Type typeConfigHost, params object[] hostInitParams)
        {
            this.hostInitParams = hostInitParams;
            host = (IInternalConfigHost)Activator.CreateInstance(typeConfigHost);
            root = new InternalConfigurationRoot();

            root.Init(host, false);
        }

        public void Init(IInternalConfigHost typeConfigHost, params object[] hostInitParams)
        {
            host = typeConfigHost;
            root = new InternalConfigurationRoot();

            root.Init(host, false);
        }

        // void InitForConfiguration(ref string  locationSubPath, out string configPath, out string locationConfigPath);

        public void InitForConfiguration(ref string locationConfigPath, out string parentConfigPath, out string parentLocationConfigPath)
        {
            host.InitForConfiguration(ref locationConfigPath, out parentConfigPath, out parentLocationConfigPath, root, hostInitParams);
        }

        public IInternalConfigHost Host {
            get { return host; }
        }

        public IInternalConfigRoot Root {
            get { return root; }
        }
    }

    internal sealed class InternalConfigConfigurationFactory : IInternalConfigConfigurationFactory
    {

        public InternalConfigConfigurationFactory() { }

        /*
        ClassConfiguration IInternalConfigConfigurationFactory.Create2(Type typeConfigHost, params object[] hostInitConfigurationParams)
        {
            return new ClassConfiguration(null, typeConfigHost, hostInitConfigurationParams);
        } */

        public _Configuration Create(Type typeConfigHost, params object[] hostInitConfigurationParams)
        {
            DebugMono.Break();

            // var conf2 = ConfigurationFactory.Create2(typeof(WebConfigurationHost), null, path, site, locationSubPath, server, userName, password, inAnotherApp);
            // configurations[confKey] = conf2;

            _Configuration conf = null;
            try
            {
                var conf3 = new WebConfigurationHost();

                var system = new InternalConfigurationSystemWeb();
                system.Init(typeConfigHost, hostInitConfigurationParams);
                conf = new _Configuration(system: system, locationSubPath: "");
            }
            catch (Exception ex) {
                CreateError = ex;
            }
            // IInternalConfigRoot root = new System.Configuration.Internal.InternalConfigRoot(conf); // conf.RootSectionGroup; // , params object[] hostInitParams)
            // conf3.Init(ref locationSub,   root, null);
            return conf;
        }

        public static Exception CreateError;

        // Normalize a locationSubpath argument
        string IInternalConfigConfigurationFactory.NormalizeLocationSubPath(string subPath, IConfigErrorInfo errorInfo)
        {
            return subPath; // BaseConfigurationRecord.NormalizeLocationSubPath(subPath, errorInfo);
        }
    }
}

namespace System.Configuration
{
    using System.Runtime.Versioning;

    public 
        sealed 
        class ClassConfiguration // : _Configuration
    {
        private Type _typeConfigHost;                // type of config host
        private object[] _hostInitConfigurationParams;   // params to init config host
        private ConfigurationSectionGroup _rootSectionGroup;  // section group for the root of all sections
        private ConfigurationLocationCollection _locations;         // support for ConfigurationLocationsCollection
        private ContextInformation _evalContext;       // evaluation context
        private Func<string, string> _TypeStringTransformer = null;
        private Func<string, string> _AssemblyStringTransformer = null;
        private bool _TypeStringTransformerIsSet = false;
        private bool _AssemblyStringTransformerIsSet = false;

        private FrameworkName _TargetFramework = null;
        //private InternalConfigRoot _configRoot;        // root of this configuration hierarchy
        private MgmtConfigurationRecord _configRecord;      // config record for this level in the hierarchy
        private Stack _SectionsStack = null;

        internal ClassConfiguration(string locationSubPath, Type typeConfigHost, params object[] hostInitConfigurationParams)
        {
            _typeConfigHost = typeConfigHost;
            _hostInitConfigurationParams = hostInitConfigurationParams;

            // _configRoot = new InternalConfigRoot(this);
            // IInternalConfigHost configHost = (IInternalConfigHost)TypeUtil.CreateInstanceWithReflectionPermission(typeConfigHost);
            // Wrap the host with the UpdateConfigHost to support SaveAs.
            IInternalConfigHost updateConfigHost = null; // new UpdateConfigHost(configHost);

            // ((IInternalConfigRoot)_configRoot).Init(updateConfigHost, true);

            //
            // Set the configuration paths for this Configuration.
            // We do this in a separate step so that the WebConfigurationHost
            // can use this object's _configRoot to get the <sites> section,
            // which is used in it's MapPath implementation.
            //
            string configPath, locationConfigPath;
            //configHost.InitForConfiguration(ref locationSubPath, out configPath, out locationConfigPath, _configRoot, hostInitConfigurationParams);

            if (!String.IsNullOrEmpty(locationSubPath) && !updateConfigHost.SupportsLocation)
            {
                throw ExceptionUtil.UnexpectedError("Configuration::ctor");
            }

            //if (String.IsNullOrEmpty(locationSubPath) != String.IsNullOrEmpty(locationConfigPath))
            //{
            //    throw ExceptionUtil.UnexpectedError("Configuration::ctor");
            //}

            configPath = AppDomain.CurrentDomain.BaseDirectory;

            // Get the configuration record for this config file.
            // _configRecord = (MgmtConfigurationRecord)_configRoot.GetConfigRecord(configPath);
            _configRecord = new MgmtConfigurationRecord { ConfigurationFilePath = configPath, HasStream = false };

            // Create another MgmtConfigurationRecord for the location that is a child of the above record.
            // Note that this does not match the resolution hiearchy that is used at runtime.
            //
            if (!String.IsNullOrEmpty(locationSubPath))
            {
                //_configRecord = MgmtConfigurationRecord.Create(
                //    _configRoot, _configRecord, locationConfigPath, locationSubPath);
            }

            // Throw if the config record we created contains global errors.
            //
            //_configRecord.ThrowIfInitErrors();
        }

        //
        // Create a new instance of Configuration for the locationSubPath,
        // with the initialization parameters that were used to create this configuration.
        //
        internal ClassConfiguration OpenLocationConfiguration(string locationSubPath)
        {
            return new ClassConfiguration(locationSubPath, _typeConfigHost, _hostInitConfigurationParams);
        }

        // public properties
        public AppSettingsSection AppSettings {
            get {
                return (AppSettingsSection)GetSection("appSettings");
            }
        }

        public ConnectionStringsSection ConnectionStrings {
            get {
                return (ConnectionStringsSection)GetSection("connectionStrings");
            }
        }

        public ConfigurationSectionGroupCollection SectionGroups {
            get {
                DebugMono.Break(); // TODO
                return null; // RootSectionGroup.SectionGroups;
            }
        }

        // public methods
        public ConfigurationSection GetSection(string sectionName)
        {
            ConfigurationSection section = (ConfigurationSection)_configRecord.GetSection(sectionName);

            return section;
        }

        public ConfigurationSectionGroup GetSectionGroup(string sectionGroupName)
        {
            DebugMono.Break(); // TODO
            ConfigurationSectionGroup sectionGroup = null; // TODO _configRecord.GetSectionGroup(sectionGroupName);

            return sectionGroup;
        }

        public string FilePath {
            get {
                return _configRecord.ConfigurationFilePath;
            }
        }

        public bool HasFile {
            get {
                return _configRecord.HasStream;
            }
        }

        public ConfigurationLocationCollection Locations {
            get {
                if (_locations == null)
                {
                    _locations = _configRecord.GetLocationCollection(this);
                }

                return _locations;
            }
        }

        public class MgmtConfigurationRecord
        {
            public string ConfigurationFilePath { get; set; }
            public bool HasStream { get; set; }

            public ConfigurationLocationCollection GetLocationCollection(ClassConfiguration @this)
            {
                return new ConfigurationLocationCollection();
            }

            public static MgmtConfigurationRecord Create(string _configRoot, object _configRecord,
                    string locationConfigPath, string locationSubPath)
            {
                return new MgmtConfigurationRecord(); 
            }

            // https://referencesource.microsoft.com/#System.Configuration/System/Configuration/BaseConfigurationRecord.cs,c3c46caf445cca68 
            public object GetSection(string configKey)
            {
#if true // DBG
            // On debug builds, the config system depends on system.diagnostics,
            // so we must always return a valid result and never throw.
            if (configKey == "system.diagnostics") { // && !ClassFlags[ClassIgnoreLocalErrors]) {
                return GetSection(configKey, true, true);
            }
            else {
                return GetSection(configKey, false, true);
            }
#else

                return GetSection(configKey, false, true);
#endif
            }


            private object GetSection(string configKey, bool getLkg, bool checkPermission)
            {
                object result;
                object resultRuntimeObject;

                //
                // Note that GetSectionRecursive may invalidate this record,
                // so there should be no further references to 'this' after the call.
                //
                GetSectionRecursive(
                        configKey, getLkg, checkPermission, true /* getRuntimeObject */, true /* requestIsHere */,
                        out result, out resultRuntimeObject);

                return resultRuntimeObject;
            }

            private void GetSectionRecursive(
                    string configKey, bool getLkg, bool checkPermission, bool getRuntimeObject, bool requestIsHere,
                    out object result, out object resultRuntimeObject)
            {

                result = null;
                resultRuntimeObject = null;

#if true // DBG
                // Debug.Assert(requestIsHere || !checkPermission, "requestIsHere || !checkPermission");
                if (getLkg) {
                    // Debug.Assert(getRuntimeObject == true, "getRuntimeObject == true");
                    // Debug.Assert(requestIsHere == true, "requestIsHere == true");
                }
#endif

                //
                // Store results in temporary variables, because we don't want to return
                // results if an exception is thrown by CheckPermissionAllowed.
                //
                object tmpResult = null;
                object tmpResultRuntimeObject = null;
                bool requirePermission = true;
                bool isResultTrustedWithoutAptca = true;

                // Throw errors from initial parse, if any.
                if (!getLkg)
                {
                   // ThrowIfInitErrors();
                }

                //
                // check for a cached result
                //

                DebugMono.Break(); // TODO

                /*
                bool hasResult = false;
                SectionRecord sectionRecord = GetSectionRecord(configKey, getLkg);

                if (sectionRecord != null && sectionRecord.HasResult)
                {
                    // Results should never be stored if the section has errors.
                    Debug.Assert(!sectionRecord.HasErrors, "!sectionRecord.HasErrors");

                    // Create the runtime object if requested and does not yet exist.
                    if (getRuntimeObject && !sectionRecord.HasResultRuntimeObject)
                    {
                        try
                        {
                            sectionRecord.ResultRuntimeObject = GetRuntimeObject(sectionRecord.Result);
                        }
                        catch
                        {
                            //
                            // Ignore the error if we are attempting to retreive
                            // the last known good configuration.
                            //
                            if (!getLkg)
                            {
                                throw;
                            }
                        }
                    }

                    // Get the cached result.
                    if (!getRuntimeObject || sectionRecord.HasResultRuntimeObject)
                    {
                        requirePermission = sectionRecord.RequirePermission;
                        isResultTrustedWithoutAptca = sectionRecord.IsResultTrustedWithoutAptca;
                        tmpResult = sectionRecord.Result;
                        if (getRuntimeObject)
                        {
                            tmpResultRuntimeObject = sectionRecord.ResultRuntimeObject;
                        }

                        hasResult = true;
                    }
                }

                //
                // If there is no cached result, get the parent's section,
                // then merge it with our own input if we have any.
                //
                if (!hasResult)
                {
                    FactoryRecord factoryRecord = null;
                    bool hasInput = (sectionRecord != null && sectionRecord.HasInput);

                    //
                    // We want to cache results in a section record if:
                    // - The request is made at this level, and so is likely to be
                    //   made here again.
                    // OR
                    // - The section has input, in which case we want to
                    //   avoid evaluating the same input multiple times.
                    //
                    bool cacheResults = (requestIsHere || hasInput);

                    bool isRootDeclaration;
                    try
                    {
                        //
                        // We need to get a factory record to:
                        // - Check whether the caller has permission to access a section.
                        // - Determine if this is the root declaration of a config section,
                        //   and thus the termination point for recursion.
                        // - Get a factory that can create a configuration section.
                        // 
                        //
                        if (requestIsHere)
                        {
                            //
                            // Ensure that we have a valid factory record and a valid factory
                            // for creating sections when a request for a section is first
                            // made.
                            //
                            factoryRecord = FindAndEnsureFactoryRecord(configKey, out isRootDeclaration);

                            //
                            // If initialization is delayed, complete initialization if:
                            //  - We can't find the requested factory, and it therefore 
                            if (IsInitDelayed
                                && (factoryRecord == null
                                    || _initDelayedRoot.IsDefinitionAllowed(factoryRecord.AllowDefinition, factoryRecord.AllowExeDefinition)))
                            {

                                //
                                // We are going to remove this record, so get any data we need
                                // before the reference to 'this' becomes invalid.
                                //
                                string configPath = this._configPath;
                                InternalConfigRoot configRoot = this._configRoot;

                                // Tell the host to no longer permit delayed initialization.
                                Host.RequireCompleteInit(_initDelayedRoot);

                                // Removed config at the root of where initialization is delayed.
                                _initDelayedRoot.Remove();

                                // Get the config record for this config path
                                BaseConfigurationRecord newRecord = (BaseConfigurationRecord)configRoot.GetConfigRecord(configPath);

                                // Repeat the call to GetSectionRecursive
                                newRecord.GetSectionRecursive(
                                    configKey, getLkg, checkPermission,
                                    getRuntimeObject, requestIsHere,
                                    out result, out resultRuntimeObject);

                                // Return and make no more references to this record.
                                return;
                            }

                            //
                            // For compatibility with previous versions,
                            // return null if the section is not found
                            // or is a group.
                            //
                            if (factoryRecord == null || factoryRecord.IsGroup)
                            {
                                return;
                            }

                            //
                            // Use the factory record's copy of the configKey,
                            // so that we don't store more than one instance
                            // of the same configKey.
                            //
                            configKey = factoryRecord.ConfigKey;
                        }
                        else if (hasInput)
                        {
                            //
                            // We'll need a factory to evaluate the input.
                            //
                            factoryRecord = FindAndEnsureFactoryRecord(configKey, out isRootDeclaration);
                            Debug.Assert(factoryRecord != null, "factoryRecord != null");
                        }
                        else
                        {
                            //
                            // We don't need a factory record unless this is the root declaration.
                            // We know it is not the root declaration if there is no factory
                            // declared here. This is important to avoid a walk up the config
                            // hierachy when there is no input in this record.
                            //
                            factoryRecord = GetFactoryRecord(configKey, false);
                            if (factoryRecord == null)
                            {
                                isRootDeclaration = false;
                            }
                            else
                            {
                                factoryRecord = FindAndEnsureFactoryRecord(configKey, out isRootDeclaration);
                                Debug.Assert(factoryRecord != null, "factoryRecord != null");
                            }
                        }

                        // We need a factory record to check permission.
                        Debug.Assert(!checkPermission || factoryRecord != null, "!checkPermission || factoryRecord != null");

                        //
                        // If this is the root declaration, then we always want to cache
                        // the result, in order to prevent the section default from being
                        // created multiple times.
                        //
                        if (isRootDeclaration)
                        {
                            cacheResults = true;
                        }

                        //
                        // We'll need a section record to cache results,
                        // and maybe to use in creating the section default.
                        //
                        if (sectionRecord == null && cacheResults)
                        {
                            sectionRecord = EnsureSectionRecord(configKey, true);
                        }

                        //
                        // Retrieve the parent's runtime object if the runtimeObject
                        // is requested, and we are not going to merge that input
                        // with input in this section.
                        //
                        bool getParentRuntimeObject = (getRuntimeObject && !hasInput);

                        object parentResult = null;
                        object parentResultRuntimeObject = null;
                        if (isRootDeclaration)
                        {
                            //
                            // Create the default section.
                            //
                            // Use the existing section record to create it if there is no input,
                            // so that the cached result is attached to the correct record.
                            //
                            SectionRecord sectionRecordForDefault = (hasInput) ? null : sectionRecord;
                            CreateSectionDefault(configKey, getParentRuntimeObject, factoryRecord, sectionRecordForDefault,
                                    out parentResult, out parentResultRuntimeObject);
                        }
                        else
                        {
                            //
                            // Get the parent section.
                            //
                            _parent.GetSectionRecursive(
                                    configKey, false, // getLkg
                                    false , // checkPermission
                                    getParentRuntimeObject, false, // requestIsHere
                                    out parentResult, out parentResultRuntimeObject);
                        }

                        if (hasInput)
                        {
                            //
                            // Evaluate the input.
                            //
                            // If Evaluate() encounters an error, it may not throw an exception
                            // when getLkg == true.
                            //
                            // The complete success of the evaluation is determined by the return value.
                            //
                            bool success = Evaluate(factoryRecord, sectionRecord, parentResult, getLkg, getRuntimeObject,
                                    out tmpResult, out tmpResultRuntimeObject);

                            Debug.Assert(success || getLkg, "success || getLkg");

                            if (!success)
                            {
                                Debug.Assert(getLkg == true, "getLkg == true");
                                // Do not cache partial results if getLkg was specified.
                                cacheResults = false;
                            }
                        }
                        else
                        {
                            //
                            // If we are going to cache results here, we will need
                            // to create a copy in the case of MgmtConfigurationRecord -
                            // otherwise we could inadvertently return the parent to the user,
                            // which could then be modified.
                            //
                            if (sectionRecord != null)
                            {
                                tmpResult = UseParentResult(configKey, parentResult, sectionRecord);
                                if (getRuntimeObject)
                                {
                                    //
                                    // If the parent result is the same as the parent runtime object,
                                    // then use the same copy of the parent result for our own runtime object.
                                    //
                                    if (object.ReferenceEquals(parentResult, parentResultRuntimeObject))
                                    {
                                        tmpResultRuntimeObject = tmpResult;
                                    }
                                    else
                                    {
                                        tmpResultRuntimeObject = UseParentResult(configKey, parentResultRuntimeObject, sectionRecord);
                                    }
                                }
                            }
                            else
                            {
                                Debug.Assert(!requestIsHere, "!requestIsHere");

                                //
                                // We don't need to make a copy if we are not storing
                                // the result, and thus not returning the result to the
                                // caller of GetSection.
                                //
                                tmpResult = parentResult;
                                tmpResultRuntimeObject = parentResultRuntimeObject;
                            }
                        }

                        //
                        // Determine which permissions are required of the caller.
                        //
                        if (cacheResults || checkPermission)
                        {
                            requirePermission = factoryRecord.RequirePermission;
                            isResultTrustedWithoutAptca = factoryRecord.IsFactoryTrustedWithoutAptca;

                            //
                            // Cache the results.
                            //
                            if (cacheResults)
                            {
                                if (sectionRecord == null)
                                {
                                    sectionRecord = EnsureSectionRecord(configKey, true);
                                }

                                sectionRecord.Result = tmpResult;
                                if (getRuntimeObject)
                                {
                                    sectionRecord.ResultRuntimeObject = tmpResultRuntimeObject;
                                }

                                sectionRecord.RequirePermission = requirePermission;
                                sectionRecord.IsResultTrustedWithoutAptca = isResultTrustedWithoutAptca;
                            }
                        }

                        hasResult = true;
                    }
                    catch
                    {
                        //
                        // Ignore the error if we are attempting to retreive
                        // the last known good configuration.
                        //
                        if (!getLkg)
                        {
                            throw;
                        }
                    }

                    //
                    // If we don't have a result, ask our parent for its
                    // last known good result.
                    //
                    if (!hasResult)
                    {
                        Debug.Assert(getLkg == true, "getLkg == true");

                        _parent.GetSectionRecursive(
                            configKey, true /* getLkg * /, checkPermission,
                            true /* getRuntimeObject * /, true /* requestIsHere * /,
                            out result, out resultRuntimeObject);

                        return;
                    }
                }

                */

                //
                // Check if permission to access the section is allowed.
                //
                if (checkPermission)
                {
                    // CheckPermissionAllowed(configKey, requirePermission, isResultTrustedWithoutAptca);
                }

                //
                // Return the results.
                //
                result = tmpResult;
                if (getRuntimeObject)
                {
                    resultRuntimeObject = tmpResultRuntimeObject;
                }
            }

        }
    }


    [System.Diagnostics.DebuggerDisplay("SectionRecord {ConfigKey}")]
    internal class SectionRecord
    {
        //
        // Flags constants
        //

        //
        // Runtime flags below 0x10000
        //

        // locked by parent input, either because a parent section is locked,
        // a parent section locks all children, or a location input for this 
        // configPath has allowOverride=false.
        private const int Flag_Locked = 0x00000001;

        // lock children of this section
        private const int Flag_LockChildren = 0x00000002;

        // propagation of FactoryRecord.IsFactoryTrustedWithoutAptca
        private const int Flag_IsResultTrustedWithoutAptca = 0x00000004;

        // propagation of FactoryRecord.RequirePermission
        private const int Flag_RequirePermission = 0x00000008;

        // Look at AddLocationInput for explanation of this flag's purpose
        private const int Flag_LocationInputLockApplied = 0x00000010;

        // Look at AddIndirectLocationInput for explanation of this flag's purpose
        private const int Flag_IndirectLocationInputLockApplied = 0x00000020;

        // The flag gives us the inherited lock mode for this section record without the file input
        // We need this to support SectionInformation.OverrideModeEffective.
        private const int Flag_ChildrenLockWithoutFileInput = 0x00000040;

        //
        // Designtime flags at or above 0x00010000
        //

        // the section has been added to the update list
        private const int Flag_AddUpdate = 0x00010000;

        // result can be null, so we use this object to indicate whether it has been evaluated
        static object s_unevaluated = new object();

        // private SafeBitVector32 _flags;

        // config key
        private string _configKey;

        // The input from location sections
        // This list is ordered to keep oldest ancestors at the front
        private List<SectionInput> _locationInputs;

        // The input from this file
        private SectionInput _fileInput;

        // This special input is used only when creating a location config record.
        // The inputs are from location tags which are found in the same config file as the
        // location config configPath, but point to the parent paths of the location config
        // configPath.  See the comment for VSWhidbey 540184 in Init() in
        // BaseConfigurationRecord.cs for more details.
        private List<SectionInput> _indirectLocationInputs;

        // the cached result of evaluating this section
        private object _result;

        // the cached result of evaluating this section after GetRuntimeObject is called
        private object _resultRuntimeObject;


        internal SectionRecord(string configKey)
        {
            _configKey = configKey;
            _result = s_unevaluated;
            _resultRuntimeObject = s_unevaluated;
        }

        internal string ConfigKey {
            get { return _configKey; }
        }

    }

    [System.Diagnostics.DebuggerDisplay("SectionInput {_sectionXmlInfo.ConfigKey}")]
    internal class SectionInput
    {
        // result can be null, so we use this object to indicate whether it has been evaluated
        private static object s_unevaluated = new object();

        // input from the XML file
        private object // SectionXmlInfo 
            _sectionXmlInfo;

        // Provider to enhance config sources
        private object // ConfigurationBuilder 
            _configBuilder;

        // Has the config provider been determined for this input?
        private bool _isConfigBuilderDetermined;

        // Provider to use for encryption
        private ProtectedConfigurationProvider _protectionProvider;

        // Has the protection provider been determined for this input?
        private bool _isProtectionProviderDetermined;

        // the result of evaluating this section
        private object _result;

        // the result of evaluating this section after GetRuntimeObject is called
        private object _resultRuntimeObject;

        // accummulated errors related to this input
        private List<ConfigurationException> _errors;

        internal SectionInput(
            object // SectionXmlInfo
            sectionXmlInfo, List<ConfigurationException> errors)
        {
            _sectionXmlInfo = sectionXmlInfo;
            _errors = errors;

            _result = s_unevaluated;
            _resultRuntimeObject = s_unevaluated;
        }

        internal object // SectionXmlInfo 
            SectionXmlInfo {
            get { return _sectionXmlInfo; }
        }

        internal bool HasResult {
            get { return _result != s_unevaluated; }
        }

        internal bool HasResultRuntimeObject {
            get { return _resultRuntimeObject != s_unevaluated; }
        }

        internal object Result {
            get {
                // Useful assert, but it fires in the debugger when using automatic property evaluation
                // Debug.Assert(_result != s_unevaluated, "_result != s_unevaluated");

                return _result;
            }

            set { _result = value; }
        }

        internal object ResultRuntimeObject {
            get {
                // Useful assert, but it fires in the debugger when using automatic property evaluation
                // Debug.Assert(_resultRuntimeObject != s_unevaluated, "_resultRuntimeObject != s_unevaluated");

                return _resultRuntimeObject;
            }

            set { _resultRuntimeObject = value; }
        }
    }
}
