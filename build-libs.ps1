# @echo off

$dir = $PWD

mkdir external\NumericsHashing
mkdir external\System.Private.CoreLib\src\System\Collections
mkdir external\System.Private.CoreLib\src\System\Runtime\CompilerServices
mkdir external\System.Private.CoreLib\src\System

copy external\corert\src\Common\src\System\Numerics\Hashing\HashHelpers.cs                         external\NumericsHashing\HashHelpers.cs
copy external\corert\src\System.Private.CoreLib\src\System\Collections\LowLevelComparer.cs         external\System.Private.CoreLib\src\System\Collections\LowLevelComparer.cs
copy external\corert\src\System.Private.CoreLib\src\System\Collections\ObjectEqualityComparer.cs   external\System.Private.CoreLib\src\System\Collections\ObjectEqualityComparer.cs
copy external\corert\src\System.Private.CoreLib\src\System\Runtime\CompilerServices\ITuple.cs      external\System.Private.CoreLib\src\System\Runtime\CompilerServices\ITuple.cs
copy external\corert\src\System.Private.CoreLib\src\System\Runtime\CompilerServices\TupleElementNamesAttribute.cs  external\System.Private.CoreLib\src\System\Runtime\CompilerServices\TupleElementNamesAttribute.cs
copy external\corert\src\System.Private.CoreLib\src\System\Tuple.cs                                external\System.Private.CoreLib\src\System\Tuple.cs
copy external\corert\src\System.Private.CoreLib\src\System\TupleExtensions.cs                      external\System.Private.CoreLib\src\System\TupleExtensions.cs
copy external\corert\src\System.Private.CoreLib\src\System\ValueTuple.cs                           external\System.Private.CoreLib\src\System\ValueTuple.cs

cd $dir\mcs\jay

# @REM vcbuild
& msbuild jay.vcxproj

cd $dir\msvc\scripts
& csc prepare.cs

cd $dir
& ./prepare.exe mcs core

dotnet build mcs\class\corlib\corlib-net_4_x.csproj --no-restore
dotnet build mcs\class\corlib\corlib-net_4_x.csproj --no-restore -f net48 -o $PWD\bin

 dotnet build mcs\System.Web\System.Web.csproj 
 
 dotnet build mcs\System.Web\System.Web.csproj   --no-restore -f net48 -o $PWD\bin
 
 dotnet build mcs\class\corlib\corlib-net_4_x.csproj --no-restore -f net46 -o $PWD\bin\net46
 dotnet build mcs\System.Web\System.Web.csproj   --no-restore -f net46     -o $PWD\bin\net46
 
# msbuild net_4_x.sln
