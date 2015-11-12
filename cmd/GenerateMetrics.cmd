CALL SetEnvironment.cmd

dir /s /b %STOCKROOT%\ProcessedLatestDailyData\IncludeRight\*.* > %STOCKROOT%\ProcessedLatestDailyDataFileList.txt
%BINROOT%\GenerateMetrics -l %STOCKROOT%\ProcessedLatestDailyDataFileList.txt -o %STOCKROOT%\ProcessedLatestDailyData\IncludeRightMetrics -m %STOCKROOT%\metricsDefinition.txt