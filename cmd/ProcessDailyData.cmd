dir /s /b ..\RawLatestDailyData\ExcludeRight\*.* > ..\LatestDailyDataFileList.txt
..\bin\ProcessDailyStockData.exe -l ..\LatestDailyDataFileList.txt -o ..\ProcessedLatestDailyData\ExcludeRight