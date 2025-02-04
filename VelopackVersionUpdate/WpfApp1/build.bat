
@echo off
setlocal enabledelayedexpansion

if "%~1"=="" (
    echo Version number is required.
    echo Usage: build.bat [version]
    exit /b 1
)

set "version=%~1"

echo.
echo Compiling WpfApp1 with dotnet...
dotnet   publish -c Release -o %~dp0publish


echo.
xcopy /e /s /h /y /z  %~dp0bin\Release\  %~dp0publish\
echo publicacion ok



echo.
echo Building Velopack Release v%version%
vpk pack  -u WpfApp1  -v %version% -o %~dp0releases -p %~dp0publish -f net48