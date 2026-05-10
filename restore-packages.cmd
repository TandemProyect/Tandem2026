@echo off
setlocal EnableDelayedExpansion
cd /d "%~dp0"

echo Restaurando paquetes NuGet para Design.sln
echo Raiz del repo: "%cd%"
echo Carpeta de paquetes (nuget.config): "%cd%\packages"
echo Si vas a BORRAR la carpeta packages, cierra Visual Studio antes ^(evita bloqueo en DotNetCompilerPlatformTasks.dll^).
echo.

set "NUGET=%~dp0nuget.exe"
if not exist "%NUGET%" (
  echo Descargando nuget.exe en la raiz del repo...
  powershell -NoProfile -ExecutionPolicy Bypass -Command "Invoke-WebRequest -Uri 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe' -OutFile '%NUGET%' -UseBasicParsing"
)
if not exist "%NUGET%" (
  echo ERROR: No se pudo crear nuget.exe. Descarguela desde:
  echo https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
  echo Guardela como: %NUGET%
  exit /b 1
)

echo Ejecutando: nuget restore Design.sln
"%NUGET%" restore "%cd%\Design.sln" -NonInteractive
set ERR=!ERRORLEVEL!

REM Opcional: algunos tipos de proyecto usan tambien esto
set "MSBUILD="
for %%E in (Community Professional Enterprise Insiders BuildTools) do (
  if exist "%ProgramFiles%\Microsoft Visual Studio\2022\%%E\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\%%E\MSBuild\Current\Bin\MSBuild.exe"
    goto :msb
  )
)
for %%E in (Community Professional Enterprise Insiders) do (
  if exist "%ProgramFiles%\Microsoft Visual Studio\18\%%E\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\18\%%E\MSBuild\Current\Bin\MSBuild.exe"
    goto :msb
  )
)
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" (
  set "MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
)

:msb
if defined MSBUILD (
  echo.
  echo MSBuild /restore ^(complemento^)
  "!MSBUILD!" "%cd%\Design.sln" /restore /v:m
)

exit /b %ERR%
