CALL SetEnvironment.cmd

SET FILELIST=%TEMPFOLDER%\ProcessedDailyDataFileList.txt

dir /s /b %PROCESSEDDATAFOLDER%\Daily\IncludeRight\*.* > %FILELIST%
%BINROOT%\GenerateMetrics -l %FILELIST% -o %PROCESSEDDATAFOLDER%\Daily\IncludeRightMetrics -m %TEMPFOLDER%\metricsDefinition.txt -f