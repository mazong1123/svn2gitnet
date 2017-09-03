REM Unit test.
cd tests/unittests
if %errorlevel% neq 0 exit %errorlevel%

dotnet test
if %errorlevel% neq 0 exit %errorlevel%

cd ../../
if %errorlevel% neq 0 exit %errorlevel%

REM Preparing for integration test.
call publish.cmd
if %errorlevel% neq 0 exit %errorlevel%

md "integrationtests"
if %errorlevel% neq 0 exit %errorlevel%

Xcopy src\bin\Release integrationtests /s /e
if %errorlevel% neq 0 exit %errorlevel%

REM Integration test.
cd tests/integrationtests
if %errorlevel% neq 0 exit %errorlevel%

dotnet test
if %errorlevel% neq 0 exit %errorlevel%

cd ../../
if %errorlevel% neq 0 exit %errorlevel%

REM Clean up.
rd /s /q "integrationtests"
if %errorlevel% neq 0 exit %errorlevel%