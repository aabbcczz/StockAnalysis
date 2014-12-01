CALL SetEnvironment.cmd

dir /s /b %STOCKROOT%\ProcessedLatestDailyData\IncludeRightMetrics\*.* > %STOCKROOT%\ProcessedLatestDailyMetricsFileList.txt
%BINROOT%\SelectStocksBasedOnMetrics.exe -l %STOCKROOT%\ProcessedLatestDailyMetricsFileList.txt -o %STOCKROOT%\SelectedStocks.csv -k 3
