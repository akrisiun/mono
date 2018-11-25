$env:MONO_CORLIB_VERSION=1050400003

# ProjectConfiguration Include="Debug|x64">
# <Configuration>Debug</Configuration>
# <Platform>x64</Platform>

$msbuild = "c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
& $msbuild msvc\mono.vcxproj  /p:Configuration=Debug  /p:Platform=x64
