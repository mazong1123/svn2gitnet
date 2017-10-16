cd src
if %errorlevel% neq 0 exit %errorlevel%

dotnet restore
if %errorlevel% neq 0 exit %errorlevel%

dotnet publish --self-contained -c Release -r win10-x64
if %errorlevel% neq 0 exit %errorlevel%

REM dotnet publish --self-contained -c Release -r win10-x86
REM if %errorlevel% neq 0 exit %errorlevel%

cd ..
if %errorlevel% neq 0 exit %errorlevel%