CALL SetEnvironment.cmd

%BINROOT%\CalcMarketValue.exe -s %STOCKROOT%\data\shares.txt -p %STOCKROOT%\data\prices.txt -o %STOCKROOT%\data\marketvalue.csv

IF ERRORLEVEL 0 (
%BINROOT%\ImportTable.exe -c %STOCKROOT%\data\marketvalue.csv
)