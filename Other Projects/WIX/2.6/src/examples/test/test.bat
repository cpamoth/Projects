@echo off
setlocal
set FLAVOR=debug

REM redirect the WIX environment variable to WIX20
set WIX=%WIX20%

:Parse_Args
if /i [%1]==[] goto :End_Parse_Args
if /i [%1]==[-notidy] set _TIDY=%1& shift& goto :Parse_Args
if /i [%1]==[/notidy] set _TIDY=%1& shift& goto :Parse_Args
if /i [%1]==[debug] set FLAVOR=%1&shift& goto :Parse_Args
if /i [%1]==[ship] set FLAVOR=%1&shift& goto :Parse_Args
if /i [%1]==[-verbose] set _VERBOSE=%1& shift& goto :Parse_Args
if /i [%1]==[/verbose] set _VERBOSE=%1& shift& goto :Parse_Args
if /i [%1]==[-v] set _VERBOSE=%1& shift& goto :Parse_Args
if /i [%1]==[/v] set _VERBOSE=%1& shift& goto :Parse_Args
set _T=-t%1& shift& goto :Parse_Args
echo.Invalid argument: '%1'
:End_Parse_Args

set _C=%TARGETROOT%\wix20\x86\%FLAVOR%\lang-neutral\candle.exe
set _L=%TARGETROOT%\wix20\x86\%FLAVOR%\lang-neutral\light.exe
set _Y=%TARGETROOT%\wix20\x86\%FLAVOR%\lang-neutral\lit.exe
set _D=%TARGETROOT%\wix20\x86\%FLAVOR%\lang-neutral\dark.exe
set _W=%TARGETROOT%\wix20\x86\%FLAVOR%\lang-neutral\wixcop.exe

echo Using: 
echo    Candle:  %_C%
echo    Light:   %_L%
echo    Lit:     %_Y%
echo    Dark:    %_D%
echo    WixCop:  %_W%
echo.

:run
%TARGETROOT%\wix20\x86\%FLAVOR%\lang-neutral\WixQTest.exe -c%_C% -l%_L% -y%_Y% -d%_D% -w%_W% tests.xml %_T% %_TIDY% %_VERBOSE%

endlocal
