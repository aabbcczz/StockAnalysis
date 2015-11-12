CALL SetEnvironment.cmd

dir /s /b %STOCKROOT%\FullDailyData1990-20140513\IncludeRight\*.* > %STOCKROOT%\FullDailyDataFileList.txt
%BINROOT%\ProcessDailyStockData.exe -l %STOCKROOT%\FullDailyDataFileList.txt -o %STOCKROOT%\ProcessedFullDailyData\IncludeRight -n %STOCKROOT%\fullstockname.txt -c %STOCKROOT%\fullcodefile.txt