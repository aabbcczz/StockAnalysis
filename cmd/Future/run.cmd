call DownloadStockData.cmd

IF %ERRORLEVEL% NEQ 0 (
goto quit
)

call preevaluate.cmd
call GenerateMetrics.cmd
call SelectLastMetrics.cmd

:quit