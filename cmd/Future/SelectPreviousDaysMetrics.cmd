CALL SetEnvironment.cmd

SET FILELIST=%TEMPFOLDER%\LastMetricsFileList.txt

%BINROOT%\SelectStocksBasedOnMetrics.exe -l %FILELIST% -o .\SelectedStocks.csv -k %*
