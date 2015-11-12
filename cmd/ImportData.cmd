CALL SetEnvironment.cmd

%BINROOT%\ImportTable.exe -c %STOCKROOT%\data\资产负债表.csv
%BINROOT%\ImportTable.exe -c %STOCKROOT%\data\跨年度利润表.csv
%BINROOT%\ImportTable.exe -c %STOCKROOT%\data\利润表.csv