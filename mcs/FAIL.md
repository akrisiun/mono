## 2008-08 build mscorlib.dll 4.6.57

S E:\Beta\dot64\mono08\mcs> & ../bin/mono-sgen.exe --debug --debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555 TestWeb/bin/net48/TestWeb.exe
WARNING: The runtime version supported by this application is unavailable.
Using default runtime: v4.0.30319

Unhandled Exception:
Nested exception detected.
Original Exception: at standalone_tests.Class.Main () [0x00040] in E:\Beta\dot64\mono08\mcs\TestWeb\Main.cs:43

Nested exception:at System.Runtime.CompilerServices.Unsafe.Add<char> (char&,int) [0x00006] in <3082ab59ebf24389844543ad94b7113a>:0
at System.MemoryExtensions.AsSpan (string,int,int) [0x00058] in <3082ab59ebf24389844543ad94b7113a>:0
at System.Text.StringBuilder.AppendFormatHelper (System.IFormatProvider,string,System.ParamsArray) [0x003ad] in <3082ab59ebf24389844543ad94b7113a>:0
at System.Text.StringBuilder.AppendFormat (string,object) [0x00009] in <3082ab59ebf24389844543ad94b7113a>:0
at System.Diagnostics.StackTrace.AddFrames (System.Text.StringBuilder,bool,bool&) [0x00124] in <3082ab59ebf24389844543ad94b7113a>:0
at System.Diagnostics.StackTrace.ToString () [0x00088] in <3082ab59ebf24389844543ad94b7113a>:0
at System.Diagnostics.StackTrace.ToString (System.Diagnostics.StackTrace/TraceFormat) [0x00002] in <3082ab59ebf24389844543ad94b7113a>:0
at System.Environment.GetStackTrace (System.Exception,bool) [0x0001c] in <3082ab59ebf24389844543ad94b7113a>:0
at System.Exception.GetStackTrace (bool) [0x00058] in <3082ab59ebf24389844543ad94b7113a>:0
at System.Exception.ToString (bool,bool) [0x0009b] in <3082ab59ebf24389844543ad94b7113a>:0
at System.Exception.ToString (bool,bool) [0x0006f] in <3082ab59ebf24389844543ad94b7113a>:0
at System.Exception.ToString () [0x00004] in <3082ab59ebf24389844543ad94b7113a>:0


[ERROR] FATAL UNHANDLED EXCEPTION: Nested exception detected.
Original Exception: at standalone_tests.Class.Main () [0x00040] in E:\Beta\dot64\mono08\mcs\TestWeb\Main.cs:43

## other

 E:\Beta\mono64\mono04\mcs> dot-r .\TestWeb\
Attempting to cancel the build...
PS E:\Beta\mono64\mono04\mcs> C:\Program Files\dotnet\sdk\2.1.500\NuGet.targets(114,5): error : Restore canceled! [E:\Beta\mono64\mono04\mcs\corlib.sln]PS E:\Beta\mono64\mono04\mcs> dot-4 .\TestWeb\dotnet msbuild -p:Platform=net_4_x .\TestWeb\
Microsoft (R) Build Engine version 15.9.20+g88f5fadfbe for .NET Core
Copyright (C) Microsoft Corporation. All rights reserved.

  TestWeb -> E:\Beta\mono64\mono04\mcs\TestWeb\bin\net48\TestWeb.exe
PS E:\Beta\mono64\mono04\mcs> ./debug-4 .\TestWeb\bin\net48\TestWeb.exe
WARNING: The runtime version supported by this application is unavailable.
Using default runtime: v4.0.30319

Unhandled Exception:
System.TypeInitializationException: TypeInitialization_Type ---> System.TypeInitializationException: TypeInitialization_Type ---> System.NullReferenceException: Object reference not set to an instance of an object
  at System.Runtime.InteropServices.Marshal.SizeOf[T] (T structure) [0x00001] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Pin.Sizeof[X] (X str) [0x00000] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Pin.DataPtr[X] (X str, System.Int32 delta, System.Boolean withSize) [0x00001] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Runtime.CompilerServices.Unsafe.As3[T] (System.Object o) [0x0000e] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.MemoryExtensions.MeasureStringAdjustment () [0x00019] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.MemoryExtensions..cctor () [0x00000] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
   Exception_EndOfInnerExceptionStack
  at System.Globalization.CompareInfo.CompareOrdinalIgnoreCase (System.String strA, System.Int32 indexA, System.Int32 lengthA, System.String strB, System.Int32 indexB, System.Int32 lengthB) [0x00001] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.String.Compare (System.String strA, System.String strB, System.StringComparison comparisonType) [0x000f8] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Globalization.EncodingTable.internalGetCodePageFromName (System.String name) [0x00014] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Globalization.EncodingTable.GetCodePageFromName (System.String name) [0x00034] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Text.Encoding.GetEncoding (System.String name) [0x00014] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Text.EncodingHelper.GetDefaultEncoding () [0x00016] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Text.Encoding.CreateDefaultEncoding () [0x00001] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Text.Encoding.get_Default () [0x00010] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Console..cctor () [0x0002f] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
   Exception_EndOfInnerExceptionStack
  at standalone_tests.Class.Main () [0x00040] in E:\Beta\mono64\mono04\mcs\TestWeb\Main.cs:43
[ERROR] FATAL UNHANDLED EXCEPTION: System.TypeInitializationException: TypeInitialization_Type ---> System.TypeInitializationException: TypeInitialization_Type ---> System.NullReferenceException: Object reference not set to an instance of an object
  at System.Runtime.InteropServices.Marshal.SizeOf[T] (T structure) [0x00001] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Pin.Sizeof[X] (X str) [0x00000] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Pin.DataPtr[X] (X str, System.Int32 delta, System.Boolean withSize) [0x00001] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Runtime.CompilerServices.Unsafe.As3[T] (System.Object o) [0x0000e] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.MemoryExtensions.MeasureStringAdjustment () [0x00019] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.MemoryExtensions..cctor () [0x00000] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
   Exception_EndOfInnerExceptionStack
  at System.Globalization.CompareInfo.CompareOrdinalIgnoreCase (System.String strA, System.Int32 indexA, System.Int32 lengthA, System.String strB, System.Int32 indexB, System.Int32 lengthB) [0x00001] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.String.Compare (System.String strA, System.String strB, System.StringComparison comparisonType) [0x000f8] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Globalization.EncodingTable.internalGetCodePageFromName (System.String name) [0x00014] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Globalization.EncodingTable.GetCodePageFromName (System.String name) [0x00034] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Text.Encoding.GetEncoding (System.String name) [0x00014] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Text.EncodingHelper.GetDefaultEncoding () [0x00016] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Text.Encoding.CreateDefaultEncoding () [0x00001] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Text.Encoding.get_Default () [0x00010] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
  at System.Console..cctor () [0x0002f] in <be1c8c308c9d4885a7763b6e0a0ff57a>:0
   Exception_EndOfInnerExceptionStack
  at standalone_tests.Class.Main () [0x00040] in E:\Beta\mono64\mono04\mcs\TestWeb\Main.cs:43

  PS E:\Beta\mono64\mono04\mcs>
