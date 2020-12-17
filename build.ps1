#!/usr/bin/env pwsh

#export 
$env:PATH="/usr/local/bin:/Library/Frameworks/Mono.framework/Versions/Current/Commands/:$env:PATH"
$msbuild="/Library/Frameworks/Mono.framework/Versions/Current/Commands/msbuild"

$csproj="$PWD/mcs/class/corlib/"
# /p:DefineConstants="DEBUG;BETA" 
# $debug = "/p:DefineConstants=DEBUG;NET_4_6;"
$debug = "/p:FeatureSet=BETA"

Write-Host msbuild mcs/class/System /p:GenerateFullPaths=true $debug

Write-Host $msbuild $csproj $debug
msbuild $csproj /nologo "/v:m" $debug "/p:GenerateFullPaths=true" "/p:Configuration=Debug" "/p:BuildProjectReferences=false"

$csproj="$PWD/mcs/class/System/"
Write-Host $msbuild $csproj $debug
msbuild $csproj /nologo "/v:m" $debug /p:GenerateFullPaths=true "/p:Configuration=Debug" "/p:BuildProjectReferences=false"



