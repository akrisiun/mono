# @echo off

$dir = $PWD
$msbuild = "c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"

$vs = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\Tools\VsDevCmd.bat"
& $vs
cd mcs\jay

& $msbuild jay.vcxproj
# & $msbuild mcs\jay\jay.vcxproj

# cd ..\..\msvc\scripts
cd msvc\scripts
# csc prepare.cs
dotnet build prepare.csproj -o . -f net45

cd msvc\scripts
# !!!!!
& ./prepare.exe ..\..\mcs core

cd ..\..
msbuild net_4_x.sln
