call DownloadStockData.cmd

IF %ERRORLEVEL% NEQ 0 (
goto quit
)

call preevaluate.cmd
call predicateSGDB_selection.cmd -b %1

:quit