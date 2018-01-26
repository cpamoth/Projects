@echo off
setlocal
set FLAVOR=debug

:Parse_Args
if /i [%1]==[] goto :End_Parse_Args
if /i [%1]==[-instr] set _INSTR=%1& shift& goto :Parse_Args
if /i [%1]==[-notidy] set _TIDY=%1& shift& goto :Parse_Args
if /i [%1]==[/notidy] set _TIDY=%1& shift& goto :Parse_Args
if /i [%1]==[debug] set FLAVOR=%1&shift& goto :Parse_Args
if /i [%1]==[ship] set FLAVOR=%1&shift& goto :Parse_Args
if /i [%1]==[-update] set _UPDATE=-update& shift& goto :Parse_Args
if /i [%1]==[/update] set _UPDATE=-update& shift& goto :Parse_Args
if /i [%1]==[-val] set _VALIDATE=-val& shift& goto :Parse_Args
if /i [%1]==[/val] set _VALIDATE=-val& shift& goto :Parse_Args
if /i [%1]==[-verbose] set _VERBOSE=%1& shift& goto :Parse_Args
if /i [%1]==[/verbose] set _VERBOSE=%1& shift& goto :Parse_Args
if /i [%1]==[-v] set _VERBOSE=%1& shift& goto :Parse_Args
if /i [%1]==[/v] set _VERBOSE=%1& shift& goto :Parse_Args
if /i [%1]==[-?] goto :Help
if /i [%1]==[/?] goto :Help
if /i [%1]==[-help] goto :Help
if /i [%1]==[/help] goto :Help
set _T=%_T% -test:%1& shift& goto :Parse_Args
echo.Invalid argument: '%1'
goto :Help
:End_Parse_Args

%TARGETROOT%\wix\x86\%FLAVOR%\lang-neutral\WixUnit.exe tests.xml %_T% %_TIDY% %_VERBOSE% %_VALIDATE% %_UPDATE%
goto :End

:Help
echo.
echo test.bat - Unit tests for the core WiX toolset.
echo.
echo Usage: test.bat [debug ^| ship] [-notidy] [-update] [-verbose ^| -v] [-val] [test1 test2 ...]
echo.
echo   flavor    Sets the flavor to either debug (default) or ship
echo   -notidy   Leaves the results of the compile/decompile intact after the test has run
echo   -update   Prompt user to auto-update a test if expected and actual output files do not match
echo   -val      Turns on validation
echo   -verbose  Turns on verbose output
echo   -?        Shows this help
echo.

:End
endlocal
