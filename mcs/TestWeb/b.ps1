# dotnet build ../class/corlib.Debug/corlib.Debug.csproj
# dotnet build ../class/corlib.Debug -r win_x64 -o ./bin
# dot build E:\Beta\mono02\mono02\mcs\class\System.Web\System.Web-net_4_x.csproj

dotnet build -r win_x64 -c Debug -o ../testMono1/bin
dotnet build -r win_x64 -c Debug -o ./bin