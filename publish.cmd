cd src
dotnet restore
dotnet publish --self-contained -c Release -r ubuntu.16.04-x64
dotnet publish --self-contained -c Release -r ubuntu.15.10-x64
dotnet publish --self-contained -c Release -r win7-x64
cd ..