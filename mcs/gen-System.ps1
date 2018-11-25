
$dir = $PWD
# cd E:\Beta\mono64\
# echo E:\Beta\mono64\gen-b.ps1
# E:\Beta\mono64\gen-b.ps1

cd $dir
# c:\bin\html\HtmlGenerator.exe /debug /out:srcWeb corlib.sln 
c:\bin\html\HtmlGenerator.exe /y  /out:../srcWeb/Index3 class\System\System.csproj