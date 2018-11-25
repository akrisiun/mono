param([string]$csproj, [string] $a2)

$agent = "--debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555"

& ..\bin\mono.exe --debug --mixed-mode $agent $csproj

# debug-4 TestWeb/bin/net48/TestWeb.exe