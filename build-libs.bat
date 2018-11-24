@echo off
cd mcs\jay

@REM # vcbuild 
msbuild jay.vcxproj

@REM # cd msvc\scripts
@REM # csc prepare.cs
@REM # prepare.exe ..\..\mcs core
cd ..\..

prepare.exe mcs core
@REM # & .\prepare.exe mcs core

msbuild net_4_x.sln
