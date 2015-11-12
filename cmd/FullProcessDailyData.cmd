CALL SetEnvironment.cmd

dir /s /b %STOCKROOT%\FullDailyData\IncludeRight\*.* > %STOCKROOT%\FullDailyDataFileList.txt
%BINROOT%\ProcessDailyStockData.exe -l %STOCKROOT%\FullDailyDataFileList.txt -o %STOCKROOT%\ProcessedFullDailyData\IncludeRight -n %STOCKROOT%\fullstockname.txt -c %STOCKROOT%\fullcodefile.txt