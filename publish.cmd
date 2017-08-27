cd src
dotnet restore
dotnet publish --self-contained -c Release -r win7-x64
cd ..