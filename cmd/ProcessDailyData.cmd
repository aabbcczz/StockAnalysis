CALL SetEnvironment.cmd

dir /s /b %STOCKROOT%\RawLatestDailyData\IncludeRight\*.* > %STOCKROOT%\LatestDailyDataFileList.txt
%BINROOT%\ProcessDailyStockData.exe -l %STOCKROOT%\LatestDailyDataFileList.txt -o %STOCKROOT%\ProcessedLatestDailyData\IncludeRight -n %STOCKROOT%\stockname.txt -c %STOCKROOT%\codefile.txt