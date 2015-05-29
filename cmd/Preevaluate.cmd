CALL SetEnvironment.cmd

move /Y %TDXROOT%\T0002\export\*.txt %STOCKROOT%\RawLatestDailyData\IncludeRight\

CALL .\ProcessDailyData.cmd

CALL .\GenerateStockBlock.cmd


