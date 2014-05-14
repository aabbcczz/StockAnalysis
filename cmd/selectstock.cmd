move /Y c:\zd_zxzq\T0002\export\*.txt ..\RawLatestDailyData\ExcludeRight\
CALL .\ProcessDailyData.cmd
CALL .\GenerateMetrics.cmd
dir /s /b ..\ProcessedLatestDailyData\ExcludeRightMetrics\*.* > ..\ProcessedLatestDailyMetricsFileList.txt
..\bin\SelectStocksBasedOnMetrics.exe -l ..\ProcessedLatestDailyMetricsFileList.txt -o ..\SelectedStocks.csv
