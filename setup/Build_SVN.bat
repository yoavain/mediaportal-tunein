@echo off
cls
Title Building MediaPortal RadioTime (RELEASE)
cd ..

:: Prepare version
for /f "tokens=*" %%a in ('git rev-list HEAD --count') do set REVISION=%%a 
set REVISION=%REVISION: =%
subwcrev . RadioTime\Properties\AssemblyInfo.cs RadioTime\Properties\AssemblyInfo.cs
subwcrev . RadioTimeOpmlApi\Properties\AssemblyInfo.cs RadioTimeOpmlApi\Properties\AssemblyInfo.cs
subwcrev . RadioTimePlugin\Properties\AssemblyInfo.cs RadioTimePlugin\Properties\AssemblyInfo.cs

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
	:: 64-bit
	set PROGS=%programfiles(x86)%
	goto CONT
:32BIT
	set PROGS=%ProgramFiles%	
:CONT

:: Build
"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" /target:Rebuild /property:Configuration=RELEASE /fl /flp:logfile=RadioTimePlugin.log;verbosity=diagnostic RadioTimePlugin.sln

:: Revert version
svn revert RadioTime\Properties\AssemblyInfo.cs
svn revert RadioTimeOpmlApi\Properties\AssemblyInfo.cs
svn revert RadioTimePlugin\Properties\AssemblyInfo.cs

cd setup
