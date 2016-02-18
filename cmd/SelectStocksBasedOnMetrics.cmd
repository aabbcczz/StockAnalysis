CALL SetEnvironment.cmd

SET FILELIST=%TEMPFOLDER%\ProcessedDailyMetricsFileList.txt

dir /s /b %PROCESSEDDATAFOLDER%\Daily\IncludeRightMetrics\*.* > %FILELIST%
%BINROOT%\SelectStocksBasedOnMetrics.exe -l %FILELIST% -o %TEMPFOLDER%\SelectedStocks.csv -k 3
