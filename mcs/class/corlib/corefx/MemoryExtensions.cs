using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System
{
    public static class MemoryExtensionsDebug
    {
        public static Exception LastError { get; set; }

        // MemoryExtensionsDebug
        public static SpanWrap<char> AsSpanWrap(this string text)
        {
            IntPtr adjust = IntPtr.Zero;
            char[] buf = new char[] { };

            Span<char> span;
            try {
                if (Debugger.IsAttached) {
                    Debugger.Break(); // $$$$$
                }

                // adjust = MemoryExtensions.StringAdjustment;
                unsafe {
                    span = Span<char>.DangerousCreate(
                        Pin.DataPin<char>(text[0], text.Length), ref text.GetRawStringData(), text.Length);
                        //Unsafe.AsPointer<Pinnable<char>>(text), adjust, text.Length);
                }
            }
            catch (Exception ex) {
                LastError = ex;
            }

            buf = new char[] { };
            Array.Resize<char>(ref buf, text.Length);
            text.CopyTo(0, buf, 0, buf.Length);

            var wrap = new SpanWrap<char>(buf, adjust, text.Length);
            return wrap;
        }
    }

	partial class MemoryExtensions
	{
		// System.MemoryExtensions.AsSpan(text);
		public static ReadOnlySpan<char> AsSpan (this string text)
		{
			if (text == null)
				return default;

			return new ReadOnlySpan<char> (
				Unsafe.As<Pinnable<char>> (text), StringAdjustment, text.Length);
		}
	}
}
