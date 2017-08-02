CALL SetEnvironment.cmd

REM remove all previously exported data
del /F /Q %TDXROOT%\T0002\export\*.txt

%BINROOT%\AutoExportStockData.exe %TDXROOT%\tdxw.exe future

IF %ERRORLEVEL% NEQ 0 (
@ECHO Failed to export stock data
EXIT /B 1
)