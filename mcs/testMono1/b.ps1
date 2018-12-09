
dotnet build $PWD\..\class\corlib\corlib.csproj

$msbuild = "c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
& $msbuild /v:m testMono1.vcxproj

# copy-item ./build/sgen/x64\bin\Debug\mono-2.0-sgen.*  ../bin -force -verbose
# copy-item ./build/sgen/x64\bin\Debug\mono-sgen.*      ../bin -force -verbose
