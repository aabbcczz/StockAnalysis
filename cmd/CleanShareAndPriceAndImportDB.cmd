CALL SetEnvironment.cmd

%BINROOT%\CleanCsv.exe %STOCKROOT%\data\shares.txt %STOCKROOT%\data\shares.csv

IF ERRORLEVEL 0 (
%BINROOT%\ImportTable.exe -c %STOCKROOT%\data\shares.csv
)

%BINROOT%\CleanCsv.exe %STOCKROOT%\data\prices.txt %STOCKROOT%\data\prices.csv
IF ERRORLEVEL 0 (
%BINROOT%\ImportTable.exe -c %STOCKROOT%\data\prices.csv
)