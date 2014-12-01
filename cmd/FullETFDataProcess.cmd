CALL SetEnvironment.cmd

dir /s /b %STOCKROOT%\RawFullEtfDailyData\*.* > %STOCKROOT%\FullEtfDailyDataFileList.txt
%BINROOT%\ProcessDailyStockData.exe -l %STOCKROOT%\FullEtfDailyDataFileList.txt -o %STOCKROOT%\ProcessedFullEtfDailyData\ -n %STOCKROOT%\fulletfname.txt -c %STOCKROOT%\fulletfcodefile.txt