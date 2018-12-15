//
// Debug.cs: Private corlib debug implememtation.
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2018  Microsoft Corporation
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

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo ("TestWeb, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]


namespace System.Diagnostics
{
    using System.Diagnostics.Private;

    public static class DebugMono2
    {
        public static bool IsDebug { get; set; }

        public static void Break()
        {
            Debugger.Break();
        }

        public static void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(String.Format(format, args));
        }

    }
}

namespace Mono.Web
{
    using global::System;
    using System.Web;
    using System.Web.Hosting;

    // Mono.Web.DebugWeb
    public class DebugWeb
    {
        public string appId { get; set; }
        public string vpath { get; set; }
        public string ppath { get; set; }

        // Mono.Web.DebugWeb.CreateApplicationHost<HttpApplication>()
        public static T CreateApplicationHost<T>(ApplicationManager manager = null) where T : class
        {
            // http://www.west-wind.com/presentations/aspnetruntime/aspnetruntime.asp
            //  static object CreateApplicationHost(Type hostType, string virtualDir, string physicalDir)

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // SecurityManagerMono.SecurityEnabledMono = false;

            var type = typeof(T);
            var app = ApplicationHost.CreateApplicationHost(type, "/", baseDir);

            return app as T;
        }

        public static HttpApplication CreateApplicationHostApp()
        {
            var app = new HttpApplication();
            return app;
        }

        /*
        at System.Reflection.RuntimeAssembly.InternalLoad(String assemblyString, Evidence assemblySecurity, StackCrawlMark& stackMark, IntPtr pPrivHostBinder, Boolean forIntrospection)
        at System.Reflection.RuntimeAssembly.InternalLoad(String assemblyString, Evidence assemblySecurity, StackCrawlMark& stackMark, Boolean forIntrospection)
        at System.Reflection.Assembly.Load(String assemblyString)
        at System.Runtime.Serialization.FormatterServices.LoadAssemblyFromString(String assemblyName)
        at System.Reflection.MemberInfoSerializationHolder..ctor(SerializationInfo info, StreamingContext context)
        at System.AppDomain.DoCallBack(CrossAppDomainDelegate callBackDelegate)
        at System.Web.Hosting.ApplicationHost.CreateApplicationHost(Type hostType, String virtualDir, String physicalDir) in 
        mcs\class\System.Web\System.Web.Hosting\ApplicationHost.cs:line 272
        */

        public static object GetApplicationHost(ApplicationManager manager)
        {
            var obj = manager ?? ApplicationManager.GetApplicationManager();

            // BareApplicationHost CreateHost(string appId, string vpath, string ppath)
            // var manager = new HttpApplication
            // BareApplicationHost

            return obj;
        }
    }

}

namespace System.Diagnostics.Private
{
    
	//
	// The type is renamed to DebugInternal in the post processing to avoid conficts in IVT assemblies. The proper
	// solution is to have support for IVT for members
	//
	static class Debug2
	{
        /// <summary>
        /// System.Diagnostics.Security.Debug.IsDebug
        /// </summary>
        public static bool IsDebug {
            get => DebugMono2.IsDebug;
            set { DebugMono2.IsDebug = value; }
        }


		[ConditionalAttribute ("DEBUG")]
		public static void Assert (bool condition)
		{
		}

		[ConditionalAttribute ("DEBUG")]
		public static void Assert (bool condition, string message)
		{
		}

		[ConditionalAttribute ("DEBUG")]
		public static void Assert (bool condition, string message, string detailMessage)
		{
		}

		[ConditionalAttribute ("DEBUG")]
		public static void Assert (bool condition, string message, string detailMessageFormat, params object[] args)
		{
		}

		[ConditionalAttribute ("DEBUG")]
		public static void Fail (string message)
		{
		}

		[ConditionalAttribute ("DEBUG")]
		public static void Fail (string message, string detailMessage)
		{
		}
	}
}
