# E:\Beta\mono64\mono04\mcs\mono-b.ps1

$env:MONO_CORLIB_VERSION=1051400006

# dotnet msbuild -p:Platform=net_4_x
dotnet msbuild /p:GenerateFullPaths=true  /p:Platform=net_4_x mcs\class\System.Web\System.Web.csproj

$dir = $PWD

Copy-Item -force -Verbose e:\Beta\mono64\mono04\mcs\class\lib\net_4_x-win32\*.*   $dir\lib\mono\4.5
Copy-Item -force e:\Beta\mono64\mono04\mcs\class\lib\net_4_x-win32\Facades\*.*    $dir\lib\mono\4.5\Facades

Copy-Item -force -verbose e:\Beta\mono64\mono04\msvc64\build\sgen\x64\bin\Debug\*.*  $dir\bin
# exit

Copy-Item -force -Verbose e:\Beta\mono64\mono04\mcs\class\lib\net_4_x-win32\*.*         `
                                                "c:\Program Files\Mono\lib\mono\4.5"
Copy-Item -force e:\Beta\mono64\mono04\mcs\class\lib\net_4_x-win32\Facades\*.*  `
                                                "c:\Program Files\Mono\lib\mono\4.5\Facades"

Copy-Item -force -Verbose e:\Beta\mono64\mono04\bin\*.* "c:\Program Files\Mono\bin"

exit

echo e:\Beta\mono64\mono04\mcs\class\lib\net_4_x-win32\*.*         `
                                        "c:\Program Files\Mono\lib\mono\4.8"                                        
Copy-Item -force e:\Beta\mono64\mono04\mcs\class\lib\net_4_x-win32\*.*         `
                                        "c:\Program Files\Mono\lib\mono\4.8"
Copy-Item -force e:\Beta\mono64\mono04\mcs\class\lib\net_4_x-win32\Facades\*.*  `
                                        "c:\Program Files\Mono\lib\mono\4.8\Facades" 
                                        
Copy-Item -force -verbose e:\Beta\mono64\mono04\msvc64\build\sgen\x64\bin\Debug\* `
                                        "c:\Program Files\Mono\bin\"


Copy-Item -force -verbose e:\Beta\mono64\mono04\msvc64\build\sgen\x64\bin\Debug\mono64-sgen.exe `
                                        "c:\Program Files\Mono\bin\mono.exe"
