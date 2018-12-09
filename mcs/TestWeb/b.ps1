dotnet build ../class/corlib.Debug/corlib.Debug.csproj
# dotnet build ../class/corlib.Debug -r win_x64 -o ./bin

dotnet build -r win_x64 -o ../testMono1/bin
dotnet build -r win_x64 -o ./bin