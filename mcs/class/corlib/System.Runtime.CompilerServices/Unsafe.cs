//
// Unsafe.cs: Compile only stubs for Unsafe implementation
//
// Authors:
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) 2017 Microsoft Corporation (http://microsoft.com)
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

using System.Runtime.Versioning;

namespace System.Runtime.CompilerServices
{
	static partial class Unsafe
	{
		public static ref T Add_<T> (ref T source, int elementOffset)
		{
			throw new NotImplementedException ();
		}
		
		public static ref T Add<T> (ref T source, System.IntPtr elementOffset)
		{
			throw new NotImplementedException ();
		}

		public unsafe static void* Add<T> (void* source, int elementOffset)
		{
            return new IntPtr((int)source + (int)elementOffset).ToPointer();
		}

		public static ref T AddByteOffset1<T> (ref T source, System.IntPtr byteOffset)
		{
			throw new NotImplementedException ();
		}
		
		public static bool AreSame<T> (ref T left, ref T right)
		{
			throw new NotImplementedException ();
		}
		


		public static ref T As<T> (object o) where T : class
		{
            // return (void*)(o as T);
            //Object.ReferenceEquals 	 
            throw new NotImplementedException ();
		}

        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/unsafe-code-pointers/pointer-conversions

        public static unsafe void* AsPointer<T>(object value, int adjust = 0, int delta = 0)
        {
            T val = (T)value;
            IntPtr ret = IntPtr.Zero;
            //fixed (int* p1 = &ret) {
            //    int* ret = ref ret;
            //    ret = p1;
            //}
            ret = Pin.DataPtr<T>((T)value, delta, true);

            return ret.ToPointer();
        }

        public static unsafe void* AsPointer<T>(ref object value, uint index)
            => throw new NotImplementedException();
            // => (int)((int*)value) + index;

        public static unsafe IntPtr As3<T>(object o) where T : class
        {
            unsafe {
                T val = o as T;
                var ref1 = Pin.DataPtr<T>(val);
                return ref1; // (IntPtr)((void*)ref1;
            }
        }

		public static ref TTo As<TFrom, TTo>(ref TFrom source)
		{
			throw new NotImplementedException ();
		}
		
		public unsafe static void* // ref T 
            AsRef<T> (void* source)
		{
            return source;
			// throw new NotImplementedException ();
		}

        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static ref T AsRef2<T> (void* source)
		{
            //var ptr = (IntPtr)source;
            //InteropServices.Marshal.AddRef
            throw new PlatformNotSupportedException();
        }
		
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static System.IntPtr ByteOffset1<T> (ref T origin, ref T target)
		{
			throw new NotImplementedException ();
		}

        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.IntPtr ByteOffset2<T> (ref T origin, ref T target)
		{
            unsafe {
                void* refPtr = Pin.DataPtr<T>(origin, 0, false).ToPointer();
                var ptr = new IntPtr(refPtr);
                return ptr;
            }
            // throw new PlatformNotSupportedException();
		}

        public static System.IntPtr ByteOffset3<T>(IntPtr origin, ref T target)
            => origin;


        [MethodImpl(MethodImplOptions.InternalCall)]
		internal extern static void copy_to_unmanaged (Array source, int startIndex,
							        IntPtr destination, int length);

		public static void CopyBlock (ref byte destination, ref byte source, uint byteCount)
		{
			throw new NotImplementedException ();
            // InteropServices.Marshal.copy_to_unmanaged (source, startIndex, destination, length);
		}

		public static void InitBlockUnaligned (ref byte startAddress, byte value, uint byteCount)
		{
			throw new NotImplementedException ();
		}

		public unsafe static void InitBlockUnaligned (void* startAddress, byte value, uint byteCount)
		{
			throw new NotImplementedException ();
		}

		public unsafe static T Read<T> (void* source)
		{
			throw new NotImplementedException ();
		}

        public static T ReadPtr<T>(IntPtr source)
        {
            throw new NotImplementedException ();
        }

		public static T ReadUnaligned<T> (ref byte source)
		{
			throw new NotImplementedException ();
		}

		public static int SizeOf<T> ()
		{
#if !MONO
            throw new NotImplementedException ();
#else
            return InteropServices.Marshal.SizeOf<T>();
#endif
        }

		public static ref T Subtract<T> (ref T source, int elementOffset)
		{
			throw new NotImplementedException ();
		}

		public static void WriteUnaligned<T> (ref byte destination, T value)
		{
			throw new NotImplementedException ();
		}
	}
}
