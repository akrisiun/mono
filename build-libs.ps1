# @echo off

$dir = $PWD

cd msvc\prepare\
dotnet build prepare.csproj -o $dir
cd $dir
./prepare.exe $PWD\mcs core


dotnet build mcs\tools\culevel\culevel.csproj 

cd msvc\
call winsetup.bat

$msbuild = "c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"

$vs = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\Tools\VsDevCmd.bat"
& $vs
cd mcs\jay

& $msbuild jay.vcxproj
# & $msbuild mcs\jay\jay.vcxproj

cd msvc
& $msbuild .\libmonoutils.vcxproj
& $msbuild .\mono.vcxproj /v:m /p:Platform=x64 /p:Configuration=Debug /p:MONO_TARGET_GC=sgen
& $msbuild .\mono2.vcxproj /v:m /p:Platform=x64 /p:Configuration=Debug /p:MONO_TARGET_GC=sgen

cd ..\..
msbuild net_4_x.sln

#C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\VC\VCTargets\BuildCustomizations\masm.targets(69,5):
# error MSB3721: The command ""
 # $ml64 = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC\bin\amd64\ml64.exe" 
 $ml64 = "c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC\Tools\MSVC\14.16.27023\bin\Hostx64\x64\ml64.exe"
 cd msvc
 & $ml64 /nologo /Zi /Fo"E:\Beta\mono02\mono02\msvc\.\build\sgen\x64\obj\libmonoutils\Debug\win64.obj" /D"X64" /W3 /errorReport:prompt  /Ta..\mono\utils\win64.asm
 # \c /nologo /Zi /Fo"E:\Beta\mono02\mono02\msvc\.\build\sgen\x64\obj\libmonoutils\Debug\win64.obj" /D"X64" /W3 /errorReport:prompt  /Ta..\mono\utils\win64.asm
 

#1>C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\VC\VCTargets\BuildCustomizations\masm.targets(69,5): error MSB3721: The command ""C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC\bin\amd64\ml64.exe" /c /nologo /Zi /Fo"E:\Beta\mono02\mono02\msvc/./build/sgen/x64\obj\libmonoutils\Debug\win64.obj" /D"X64" /W3 /errorReport:prompt  /Ta..\mono\utils\win64.asm" exited with code 1.
#1>Done building project "libmonoutils.vcxproj" -- FAILED.
