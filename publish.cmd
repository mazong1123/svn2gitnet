cd src
dotnet restore
dotnet publish --self-contained -c Release -r win10-x64
cd ..