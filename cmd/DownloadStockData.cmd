CALL SetEnvironment.cmd

%BINROOT%\AutoExportStockData.exe %TDXROOT%\tdxw.exe

IF %ERRORLEVEL% NEQ 0 (
@ECHO Failed to export stock data
EXIT /B 1
)