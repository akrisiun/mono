
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
