@echo off
setlocal enabledelayedexpansion
cd "C:\Users\Public\Documents\Unified Project Selector\temp"
for /f "delims=" %%a in (queuewatcher.txt) do (
    set "data=%%a"
    echo Executing !data!
    echo.
    "C:\Program Files\Siemens\Automation\WinCCUnified\bin\SIMATICRuntimeManager.exe" /wait -s !data!
    timeout 3
)
set "configFile=C:\Users\Public\Documents\Unified Project Selector\settings\watcher.cfg"

:: Check if the config file exists
if not exist "%configFile%" (
    exit /b
)

:: Read the "Target" property from the config file
for /f "tokens=2 delims==" %%a in ('findstr "Target=" "%configFile%"') do (
    set "targetDirectory=%%a"
)

:: Check if the "Target" property was found
if not defined targetDirectory (
    exit /b
)

:: Change the current directory to the target directory and delete its contents
cd /d "%targetDirectory%"
del /q *