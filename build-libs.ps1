# @echo off

$dir = $PWD

cd mono08\msvc\
&  .\winsetup.bat
cd $dir

$msbuild = "c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"

$vs = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\Tools\VsDevCmd.bat"
& $vs

cd $dir
cd mono08\mcs\jay

& $msbuild jay.vcxproj
# & $msbuild mcs\jay\jay.vcxproj

# cd ..\..\msvc\scripts
cd $dir
cd mono08\msvc\scripts
# csc prepare.cs
dotnet build prepare.csproj -o . -f net45

cd $dir
cd mono08\msvc\scripts
# !!!!!
& ./prepare.exe ..\..\mcs core

cd $dir
cd mono08\msvc
& $msbuild .\libmonoutils.vcxproj
& $msbuild .\mono.vcxproj /v:m /p:Platform=x64 /p:Configuration=Debug /p:MONO_TARGET_GC=sgen
& $msbuild .\mono8.vcxproj /v:m /p:Platform=x64 /p:Configuration=Debug /p:MONO_TARGET_GC=sgen

cd $dir
# cd ..\..
# msbuild net_4_x.sln

$ml64 = "c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC\Tools\MSVC\14.16.27023\bin\Hostx64\x64\ml64.exe"
cd mono08\msvc
& $ml64 /nologo /Zi /Fo"E:\Beta\mono02\mono02\msvc\.\build\sgen\x64\obj\libmonoutils\Debug\win64.obj" /D"X64" /W3 /errorReport:prompt  /Ta..\mono\utils\win64.asm
# \c /nologo /Zi /Fo"E:\Beta\mono02\mono02\msvc\.\build\sgen\x64\obj\libmonoutils\Debug\win64.obj" /D"X64" /W3 /errorReport:prompt  /Ta..\mono\utils\win64.asm
 
cd $dir
cd mcs\

dotnet build corLite.sln