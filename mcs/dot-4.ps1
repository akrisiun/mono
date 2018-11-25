# dotnet net4_x build 
param([string]$a, [string] $a2)

if ($a -eq "") {
    dotnet msbuild -p:Platform=net_4_x
}
else {
    echo "dotnet msbuild -p:Platform=net_4_x $a"
    dotnet msbuild -p:Platform=net_4_x $a
}