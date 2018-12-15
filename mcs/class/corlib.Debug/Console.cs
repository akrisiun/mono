using System;
using System.Diagnostics;
using System.IO;

// E:\Beta\dot64\mono08\mcs\class\corlib.Debug\Class1.cs

namespace corlib.Debug
{
	public static partial class Console
    {
        static bool IsDebug { get => System.Diagnostics.DebugMono.IsDebug; }

        public static void Break() {

            Debugger.Break();
        }

        public static TextWriter DebugConsole() {

            var stdout = System.Console.DebugStdOut();
            return stdout;
        }

        public static void Load() {

            WriteLine(new char[] { 'H', 'e', 'l', 'l', 'o', (char)0 });
        }

        public static void WriteLine(char[] format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(format.ToString());
        }


    }
}
