CALL SetEnvironment.cmd

SET FILELIST=%TEMPFOLDER%\RawDailyDataFileList.txt

dir /s /b %RAWDATAFOLDER%\Daily\IncludeRight\*.* > %FILELIST%
%BINROOT%\ProcessDailyStockData.exe -l %FILELIST% -o %PROCESSEDDATAFOLDER%\Daily\IncludeRight -n %TEMPFOLDER%\name.txt -c %TEMPFOLDER%\code.txt -f