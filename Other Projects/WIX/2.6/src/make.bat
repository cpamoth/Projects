@echo off
rem Usage : make [x86 ia64] [debug ship] [featuredep setupdw all full clean]

setlocal

if "%WIX%" == ""   (echo Must set WIX env var) & goto exit
set bld=%BUILDROOT%\wixv1

set PLATFORM=X86
set TYPE=DEBUG

set APPENDLOG=
set LOGFILE=
set CLEAN=
set FULL=
set BUILDME=
set TESTME=

:parseargs

if "%1" == ""          goto execute
if /I "%1" == "-?"       goto syntax
if /I "%1" == "-l"     (set LOGFILE=%2)      & shift & shift & goto parseargs
if /I "%1" == "-a"     (set LOGFILE=%2) & (set APPENDLOG=1) & shift & shift & goto parseargs

if /I "%1" == "debug"  (set TYPE=DEBUG) & shift & goto parseargs
if /I "%1" == "ship"   (set TYPE=SHIP) & shift & goto parseargs

if /I "%1" == "x86"    (set PLATFORM=X86) & shift & goto parseargs
if /I "%1" == "alpha"  (set PLATFORM=ALPHA)  & shift & goto parseargs
if /I "%1" == "axp64"  (set PLATFORM=AXP64)  & shift & goto parseargs
if /I "%1" == "ia64"   (set PLATFORM=IA64)   & shift & goto parseargs

if /I "%1" == "clean"      (set CLEAN=clean) & shift & goto parseargs
if /I "%1" == "full"       (set CLEAN=clean) & shift & goto add_all
if /I "%1" == "all"        shift & goto add_all
if /I "%1" == "darwinbugs" (set BUILDME=%BUILDME% darwinbugs) & shift & goto parseargs
if /I "%1" == "featuredep" (set BUILDME=%BUILDME% featuredep) & shift & goto parseargs
if /I "%1" == "tools"   (set BUILDME=%BUILDME% tools) & shift & goto parseargs
if /I "%1" == "setupdw"    (set BUILDME=%BUILDME% setupdw) & shift & goto parseargs
if /I "%1" == "example" (set TESTME=%TESTME% example.msi) & shift & goto parseargs
if /I "%1" == "testwi" (set TESTME=%TESTME% simple.msi) & shift & goto parseargs
if /I "%1" == "testserver" (set TESTME=%TESTME% server serverexample) & shift & goto parseargs
if /I "%1" == "wixcatest" (set TESTME=%TESTME% wixcatest wixcatest) & shift & goto parseargs
if /I "%1" == "schema"   (set TESTME=%TESTME% schemas) & shift & goto parseargs
if /I "%1" == "schemas"   (set TESTME=%TESTME% schemas) & shift & goto parseargs
goto error

:add_all
rem set BUILDME=%BUILDME% darwinbugs
rem set BUILDME=%BUILDME% featuredep
set BUILDME=%BUILDME% tools
rem set BUILDME=%BUILDME% setupdw

rem set TESTME=%TESTME% example
rem set TESTME=%TESTME% simple.msi
set TESTME=%TESTME% schemas
set TESTME=%TESTME% serverexample

goto parseargs

:execute

if "%CLEAN%%BUILDME%%TESTME%" == "" goto add_all

set LOGFILE=%LOGROOT%\wix.build.%TYPE%.log
set TEE=^|tee -a %LOGFILE%

rem if "%WIN_INC%" == "" set WIN_INC=%DTOOLS%\inc\win
rem if "%WIN_LIB%" == "" set WIN_LIB=%DTOOLS%\lib\%PLATFORM%\win
rem if "%MFC_INC%" == "" set MFC_INC=%DTOOLS%\inc\mfc
rem if "%MFC_LIB%" == "" set MFC_LIB=%DTOOLS%\lib\%PLATFORM%\mfc

rem set include=%mfc_inc%;%win_inc%

md %bld%\%PLATFORM%\%TYPE%\obj 1>nul 2>nul
pushd %bld%\%PLATFORM%\%TYPE%\obj
if errorlevel 1 (echo pushd %bld%\%PLATFORM%\%TYPE%\obj failed & goto bail)

if "%APPENDLOG%" == "" del /q /f %LOGFILE% 2>nul

if NOT "%CLEAN%"=="" nmake -f %WIX%\src\v1\wixv1.mak -nologo clean %1 %2 %3 %4 %5 %6 %7 %8 %9
rem if NOT "%CLEAN%"=="" nmake -f %WIX%\src\v1\data\data.mak -nologo clean %1 %2 %3 %4 %5 %6 %7 %8 %9

:build
echo set PATH=%path% >> %LOGFILE%
echo set INCLUDE=%include% >> %LOGFILE%
echo set PLATFORM=%platform% >> %LOGFILE%
echo set TYPE=%type% >> %LOGFILE%
echo set DEBUG=%debug% >> %LOGFILE%

if NOT "%BUILDME%"=="" nmake -f %WIX%\src\v1\wixv1.mak -nologo %BUILDME% PLATFORM=%PLATFORM% TYPE=%TYPE% %1 %2 %3 %4 %5 %6 %7 %8 %9 2>&1 %TEE%
rem if NOT "%TESTME%"=="" nmake -f %WIX%\src\v1\data\data.mak -nologo %TESTME% PLATFORM=%PLATFORM% TYPE=%TYPE% %1 %2 %3 %4 %5 %6 %7 %8 %9 2>&1 %TEE%
goto exit

:error
echo invalid command line param: %1
echo.
:syntax
echo Syntax: make [x86 ia64] [debug ship] [darwinbugs featuredep setupdw all full clean testwi testserver wixcatest]

:exit
echo.
endlocal

