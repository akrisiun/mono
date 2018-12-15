//
// System.Web.Util.ICalls
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

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

using System.Reflection;
using System.IO;
using System.Runtime.CompilerServices;

namespace System.Web.Util
{
    public class ICalls
    {
        public ICalls() { }

        static public string GetMachineConfigPath()
        {
            return System.Runtime.InteropServices.RuntimeEnvironment.SystemConfigurationFile;
        }

        static public string GetMachineInstallDirectory()
            => Path.GetDirectoryName(GetMachineConfigPath());

        // extern static public bool GetUnmanagedResourcesPtr(Assembly assembly, out IntPtr ptr, out int length);
    }

    class ICallsInternal
    {
        ICallsInternal() {}

#if TARGET_DOTNET
		static public string GetMachineConfigPath () {
			return System.Runtime.InteropServices.RuntimeEnvironment.SystemConfigurationFile;
		}
#else
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static public string GetMachineConfigPath ();
#endif

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static public string GetMachineInstallDirectory ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static public bool GetUnmanagedResourcesPtr (Assembly assembly, out IntPtr ptr, out int length);
	}
}

