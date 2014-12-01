CALL SetEnvironment.cmd

dir /s /b %STOCKROOT%\RawFullIndexDailyData\*.* > %STOCKROOT%\FullIndexDailyDataFileList.txt
%BINROOT%\ProcessDailyStockData.exe -l %STOCKROOT%\FullIndexDailyDataFileList.txt -o %STOCKROOT%\ProcessedFullIndexDailyData\ -n %STOCKROOT%\fullindexname.txt -c %STOCKROOT%\fullindexcodefile.txt