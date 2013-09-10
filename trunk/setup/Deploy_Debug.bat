@echo off
cls
Title Deploying MediaPortal TuneIn (DEBUG)
cd ..

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
	:: 64-bit
	set PROGS=%programfiles(x86)%
	goto CONT
:32BIT
	set PROGS=%ProgramFiles%	
:CONT

copy /y "RadioTimePlugin\bin\Debug\RadioTimePlugin.dll" "%PROGS%\Team MediaPortal\MediaPortal\plugins\Windows\"
copy /y "RadioTimePlugin\bin\Debug\RadioTimeOpmlApi.dll" "%PROGS%\Team MediaPortal\MediaPortal\plugins\Windows\"

cd setup
