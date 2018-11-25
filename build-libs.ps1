# @echo off

# git clone --single-branch -b mono-4.8 https://github.com/akrisiun/mono
# git submodule update --init

$dir = $PWD

git config --global core.autocrlf input
git config --global core.filemode false
git config core.filemode false
git submodule sync


mkdir external\NumericsHashing
mkdir external\System.Private.CoreLib\src\System\Collections
mkdir external\System.Private.CoreLib\src\System\Runtime\CompilerServices
mkdir external\System.Private.CoreLib\src\System

# copy external\corert\src\Common\src\System\Numerics\Hashing\HashHelpers.cs                         external\NumericsHashing\HashHelpers.cs
# copy external\corert\src\System.Private.CoreLib\src\System\Collections\LowLevelComparer.cs         external\System.Private.CoreLib\src\System\Collections\LowLevelComparer.cs
# copy external\corert\src\System.Private.CoreLib\src\System\Collections\ObjectEqualityComparer.cs   external\System.Private.CoreLib\src\System\Collections\ObjectEqualityComparer.cs
# copy external\corert\src\System.Private.CoreLib\src\System\Runtime\CompilerServices\ITuple.cs      external\System.Private.CoreLib\src\System\Runtime\CompilerServices\ITuple.cs
# copy external\corert\src\System.Private.CoreLib\src\System\Runtime\CompilerServices\TupleElementNamesAttribute.cs  external\System.Private.CoreLib\src\System\Runtime\CompilerServices\TupleElementNamesAttribute.cs
# copy external\corert\src\System.Private.CoreLib\src\System\Tuple.cs                                external\System.Private.CoreLib\src\System\Tuple.cs
# copy external\corert\src\System.Private.CoreLib\src\System\TupleExtensions.cs                      external\System.Private.CoreLib\src\System\TupleExtensions.cs
# copy external\corert\src\System.Private.CoreLib\src\System\ValueTuple.cs                           external\System.Private.CoreLib\src\System\ValueTuple.cs

# cd $dir\mcs\jay
# @REM vcbuild

# https://www.mono-project.com/docs/compiling-mono/windows/

# Enable BTLS as cryptographic backend for Windows builds.
# choco install cmake strawberryperl ninja yasm -y

# For Visual Studio 2017 64-bit Mono Runtime build:
# set VisualStudioVersion=15.0
# msvc\run-msbuild.bat "/p:Configuration=Release /p:Platform=x64 /p:MONO_TARGET_GC=sgen /t:Build"

$env:VisualStudioVersion=15.0
get-childitem env: | findstr VS
 
& .\msvc\run-msbuild.bat "/p:Configuration=Debug /p:Platform=x64 /p:MONO_TARGET_GC=sgen /t:Build"
# & msbuild mcs\jay\jay.vcxproj

cd $dir\msvc\scripts
& csc prepare.cs

cd $dir
& ./prepare.exe mcs core

dotnet msbuild /p:GenerateFullPaths=true  /p:Platform=net_4_x mcs\class\corlib\corlib.csproj
# --no-restore
# dotnet msbuild /p:GenerateFullPaths=true  /p:Platform=net_4_x mcs\class\corlib\corlib.csproj --no-restore -f net48 -o $PWD\bin

dotnet msbuild /p:GenerateFullPaths=true  /p:Platform=net_4_x mcs\class\System.Web\System.Web.csproj

# dotnet msbuild /p:GenerateFullPaths=true mcs\System.Web\System.Web.csproj   --no-restore -f net48 -o $PWD\bin
 
# dotnet build mcs\class\corlib\corlib-net_4_x.csproj --no-restore -f net46 -o $PWD\bin\net46
# dotnet build mcs\System.Web\System.Web.csproj   --no-restore -f net46     -o $PWD\bin\net46
 
# msbuild net_4_x.sln
