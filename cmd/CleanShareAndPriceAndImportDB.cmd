..\bin\CleanCsv.exe ..\data\shares.txt ..\data\shares.csv

IF ERRORLEVEL 0 (
..\bin\ImportTable.exe -c ..\data\shares.csv
)

..\bin\CleanCsv.exe ..\data\prices.txt ..\data\prices.csv
IF ERRORLEVEL 0 (
..\bin\ImportTable.exe -c ..\data\prices.csv
)