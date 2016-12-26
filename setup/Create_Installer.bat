@echo off
cls
Title Creating MediaPortal RadioTime Installer

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
	:: 64-bit
	set PROGS=%programfiles(x86)%
	goto CONT
:32BIT
	set PROGS=%ProgramFiles%	
:CONT

IF NOT EXIST "%PROGS%\Team MediaPortal\MediaPortal\" SET PROGS=C:

:: Get version from DLL
FOR /F "tokens=1-3" %%i IN ('tools\sigcheck.exe "..\RadioTimePlugin\bin\Release\RadioTimePlugin.dll"') DO ( IF "%%i %%j"=="File version:" SET version=%%k )

:: trim version
SET version=%version:~0,-1%
ECHO %version%
:: Temp xmp2 file
copy RadioTime.xmp2 RadioTimeTemp.xmp2

:: Sed "update-{VERSION}.xml" from xmp2 file
Tools\sed.exe -i "s/update-{VERSION}.xml/update-%version%.xml/g" RadioTimeTemp.xmp2

:: Build MPE1
"%PROGS%\Team MediaPortal\MediaPortal\MPEMaker.exe" RadioTimeTemp.xmp2 /B /V=%version% /UpdateXML

:: Cleanup
del RadioTimeTemp.xmp2

:: Sed "RadioTime-{VERSION}.MPE1" from update.xml
tools\sed.exe -i "s/RadioTime-{VERSION}.MPE1/RadioTime-%version%.MPE1/g" update-%version%.xml

:: Parse version (Might be needed in the futute)
FOR /F "tokens=1-4 delims=." %%i IN ("%version%") DO ( 
	SET major=%%i
	SET minor=%%j
	SET build=%%k
	SET revision=%%l
)

:: Rename MPE1
if exist "builds\RadioTime-%major%.%minor%.%build%.%revision%.MPE1" del "builds\RadioTime-%major%.%minor%.%build%.%revision%.MPE1"
rename builds\RadioTime-MAJOR.MINOR.BUILD.REVISION.MPE1 "RadioTime-%major%.%minor%.%build%.%revision%.MPE1"
