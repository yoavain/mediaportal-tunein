echo off
cls
Title Deploying MediaPortal TuneIn (RELEASE)
cd ..

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
	:: 64-bit
	set PROGS=%programfiles(x86)%
	goto CONT
:32BIT
	set PROGS=%ProgramFiles%
:CONT

copy /y "RadioTimePlugin\bin\Release\RadioTimePlugin.dll" "%PROGS%\Team MediaPortal\MediaPortal\plugins\Windows\"
copy /y "RadioTimePlugin\bin\Release\RadioTimeOpmlApi.dll" "%PROGS%\Team MediaPortal\MediaPortal\plugins\Windows\"

cd setup