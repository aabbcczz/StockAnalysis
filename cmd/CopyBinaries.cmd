CALL SetGlobalEnvironment.cmd

XCOPY /Y /D %BINARYSRCROOT%\CalcMarketValue\bin\release\*.* %BINROOT%\
XCOPY /Y /D %BINARYSRCROOT%\CleanCsv\bin\release\*.* %BINROOT%\
XCOPY /Y /D %BINARYSRCROOT%\GenerateMetrics\bin\release\*.* %BINROOT%\
XCOPY /Y /D %BINARYSRCROOT%\GetFinanceReports\bin\release\*.* %BINROOT%\
XCOPY /Y /D %BINARYSRCROOT%\ImportTable\bin\release\*.* %BINROOT%\
XCOPY /Y /D %BINARYSRCROOT%\ProcessDailyStockData\bin\release\*.* %BINROOT%\
XCOPY /Y /D %BINARYSRCROOT%\ReportParser\bin\release\*.* %BINROOT%\
XCOPY /Y /D %BINARYSRCROOT%\SelectStocksBasedOnMetrics\bin\release\*.* %BINROOT%\
XCOPY /Y /D %BINARYSRCROOT%\EvaluatorCmdClient\bin\release\*.* %BINROOT%\
XCopy /Y /D %BINARYSRCROOT%\PredicatorCmdClient\bin\release\*.* %BINROOT%\
XCopy /Y /D %BINARYSRCROOT%\DTViewer\bin\release\*.* %BINROOT%\
XCopy /Y /D %BINARYSRCROOT%\ConvertTdxBlockData\bin\release\*.* %BINROOT%\

