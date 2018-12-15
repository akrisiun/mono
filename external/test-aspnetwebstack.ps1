
$dir = $PWD
cd aspnetwebstack\

dotnet test  test\System.Web.Razor.Test                -v n
dotnet test  test\System.Net.Http.Formatting.Test.Unit -v n
dotnet test  test\System.Web.Http.Test                 -v n

cd $dir