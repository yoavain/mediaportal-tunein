@echo off
cls
Title Building MediaPortal RadioTime (DEBUG)
cd ..

setlocal enabledelayedexpansion

:: Prepare version
for /f "tokens=*" %%a in ('git rev-list HEAD --count') do set REVISION=%%a 
set REVISION=%REVISION: =%
"setup\Tools\sed.exe" -i "s/\$WCREV\$/%REVISION%/g" RadioTime\Properties\AssemblyInfo.cs
"setup\Tools\sed.exe" -i "s/\$WCREV\$/%REVISION%/g" RadioTimeOpmlApi\Properties\AssemblyInfo.cs
"setup\Tools\sed.exe" -i "s/\$WCREV\$/%REVISION%/g" RadioTimePlugin\Properties\AssemblyInfo.cs

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
	:: 64-bit
	set PROGS=%programfiles(x86)%
	goto CONT
:32BIT
	set PROGS=%ProgramFiles%	
:CONT

:: Build
"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" /target:Rebuild /property:Configuration=DEBUG RadioTimePlugin.sln

:: Revert version
git checkout RadioTime\Properties\AssemblyInfo.cs
git checkout RadioTimeOpmlApi\Properties\AssemblyInfo.cs
git checkout RadioTimePlugin\Properties\AssemblyInfo.cs

cd setup
