//
// System.RuntimeMethodHandle.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace System
{
	[ComVisible (true)]
	[Serializable]
	public struct RuntimeMethodHandle : ISerializable
	{
		IntPtr value;

		internal RuntimeMethodHandle (IntPtr v)
		{
			value = v;
		}

		RuntimeMethodHandle (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			MonoMethod mm = ((MonoMethod) info.GetValue ("MethodObj", typeof (MonoMethod)));
			value = mm.MethodHandle.Value;
			if (value == IntPtr.Zero)
				throw new SerializationException (Locale.GetText ("Insufficient state."));
		}

		public IntPtr Value {
			get {
				return value;
			}
		}

		// This is from ISerializable
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			if (value == IntPtr.Zero)
				throw new SerializationException ("Object fields may not be properly initialized");

			info.AddValue ("MethodObj", (MonoMethod) MethodBase.GetMethodFromHandle (this), typeof (MonoMethod));
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		static extern IntPtr GetFunctionPointer (IntPtr m);

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public IntPtr GetFunctionPointer ()
		{
			return GetFunctionPointer (value);
		}

        /// <summary>
        ///  $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        internal static IntPtr GetFunctionPointer(RuntimeMethodHandleInternal handle)
        {
            return GetFunctionPointer(handle.Value);
        }

        /*
        https://source.dot.net/#System.Private.CoreLib/src/System/RuntimeHandles.cs,a6494e59afc3e504
        [DllImport(JitHelpers.QCall, CharSet = CharSet.Unicode)]
        internal static extern IntPtr GetFunctionPointer(RuntimeMethodHandleInternal handle);

        public IntPtr GetFunctionPointer()
        {
            IntPtr ptr = GetFunctionPointer(EnsureNonNullMethodInfo(m_value).Value);
            GC.KeepAlive(m_value);
            return ptr;
        }
        */

        // E:\Beta\mono02\mono02\mono08\mono\metadata\icall-def.h
        // ICALL_TYPE(RTH, "System.RuntimeTypeHandle", RTH_1)
        // HANDLES(ICALL(RTH_3, "GetAttributes", ves_icall_RuntimeTypeHandle_GetAttributes))
    
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern MethodAttributes GetAttributes(RuntimeMethodHandleInternal method);

        internal static MethodAttributes GetAttributes(IRuntimeMethodInfo method)
        {
            MethodAttributes retVal = RuntimeMethodHandle.GetAttributes(method.Value);
            GC.KeepAlive(method);
            return retVal;
        }


        [ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public override bool Equals (object obj)
		{
			if (obj == null || GetType () != obj.GetType ())
				return false;

			return value == ((RuntimeMethodHandle)obj).Value;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public bool Equals (RuntimeMethodHandle handle)
		{
			return value == handle.Value;
		}

		public override int GetHashCode ()
		{
			return value.GetHashCode ();
		}

		public static bool operator == (RuntimeMethodHandle left, RuntimeMethodHandle right)
		{
			return left.Equals (right);
		}

		public static bool operator != (RuntimeMethodHandle left, RuntimeMethodHandle right)
		{
			return !left.Equals (right);
		}

		internal static string ConstructInstantiation (RuntimeMethodInfo method, TypeNameFormatFlags format)
		{
			var sb = new StringBuilder ();
			var gen_params = method.GetGenericArguments ();
			sb.Append ("[");
			for (int j = 0; j < gen_params.Length; j++) {
				if (j > 0)
					sb.Append (",");
				sb.Append (gen_params [j].Name);
			}
			sb.Append ("]");
			return sb.ToString ();
		}

		internal bool IsNullHandle ()
		{
			return value == IntPtr.Zero;
		}
	}
}
