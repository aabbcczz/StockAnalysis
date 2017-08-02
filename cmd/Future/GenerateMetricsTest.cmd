CALL SetEnvironment.cmd

SET FILELIST=%TEMPFOLDER%\ProcessedDailyDataFileListTest.txt

%BINROOT%\GenerateMetrics -l %FILELIST% -o %PROCESSEDDATAFOLDER%\Daily\IncludeRightMetrics -m %TEMPFOLDER%\metricsDefinition.txt