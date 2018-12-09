//
// Array Reverse extension.Console.cs
// Mono.Cecil\AssemblyReader.cs:line 3609
//System.MissingMethodException: Method not found: 'Void System.Array.Reverse(!!0[])'.
//    .PE.ByteBuffer.ReadDouble()
//     at Mono.Cecil.SignatureReader.ReadPrimitiveValue(ElementType type) i

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
    using System.Un;

    public static class ArrayEx
    {
        static Type OriginType { get => typeof(System.Array);  }

        public static void ReverseEx(this Array array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            ReverseEx(array, array.GetLowerBound(0), array.Length);
        }

        // Reverses the elements in a range of an array. Following a call to this
        // method, an element in the range given by index and count
        // which was previously located at index i will now be located at
        // index index + (index + count - i - 1).
        // Reliability note: This may fail because it may have to box objects.
        // 
        public static void ReverseEx(this Array array, int index, int length)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            int lowerBound = array.GetLowerBound(0);
            if (index < lowerBound || length < 0)
                throw new ArgumentOutOfRangeException((index < lowerBound ? nameof(index) : nameof(length)),
                         "ArgumentOutOfRange_NeedNonNegNum");
            if (array.Length - (index - lowerBound) < length)
                throw new ArgumentException("Argument_InvalidOffLen");
            if (array.Rank != 1)
                throw new RankException("Rank_MultiDimNotSupported");

            //Object[] objArray = array as Object[];
            //if (objArray != null)
            //{
            //    Array.Reverse<object>(objArray, index, length);
            //}
            //else
            {
                int i = index;
                int j = index + length - 1;
                while (i < j)
                {
                    Object temp = array.GetValue(i);
                    array.SetValue(array.GetValue(j), i);
                    array.SetValue(temp, j);
                    i++;
                    j--;
                }
            }
        }

        public static void Reverse<T>(T[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            ReverseEx(array, 0, array.Length);
        }

        public static void ReverseEx<T>(this T[] array, int index, int length)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (index < 0 || length < 0)
                throw new ArgumentOutOfRangeException((index < 0 ? nameof(index) : nameof(length)), 
                          "ArgumentOutOfRange_NeedNonNegNum");
            if (array.Length - index < length)
                throw new ArgumentException("Argument_InvalidOffLen");


            int i = index;
            int j = index + length - 1;
            while (i < j)
            {
                Object temp = array.GetValue(i);
                array.SetValue(array.GetValue(j), i);
                array.SetValue(temp, j);
                i++;
                j--;
            }

            //ref T p = ref Unsafe.As<byte, T>(ref safe.GetRawSzArrayData(array));
            //int i = index;
            //int j = index + length - 1;
            //while (i < j)
            //{
            //    T temp = Unsafe.Add(ref p, i);
            //    Unsafe.Add(ref p, i) = Unsafe.Add(ref p, j);
            //    Unsafe.Add(ref p, j) = temp;
            //    i++;
            //    j--;
            //}
        }

      

    }
}

namespace System.Un
{
    using nint = System.Int64;

    [StructLayout(LayoutKind.Sequential)]
    class RawData
    {
        public IntPtr Count; // Array._numComponents padded to IntPtr
        public byte Data;
    }

    public class safe {

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref byte GetRawSzArrayData(object array)
        {
            // Debug.Assert(IsSzArray);
            return ref Unsafe.As<RawData>(array).Data;
        }

    }

    public class Unsafe {

        public static unsafe void* Add<T>(void* source, int elementOffset)
        {
            return (byte*)source + (elementOffset * (nint)Marshal.SizeOf(typeof(T))); // SizeOf<T>());
        }

        public static ref TTo As<TFrom, TTo>(ref TFrom source)
        {
            throw new PlatformNotSupportedException();

            // ldarg.0
            // ret
        }

        // [Intrinsic]
        // [NonVersionable]
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T As<T>(object value) where T : class
        {
            throw new PlatformNotSupportedException();

            // ldarg.0
            // ret
        }

    }

}