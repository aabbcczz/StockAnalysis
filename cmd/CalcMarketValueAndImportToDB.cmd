..\bin\CalcMarketValue.exe -s ..\data\shares.txt -p ..\data\prices.txt -o ..\data\marketvalue.csv

IF ERRORLEVEL 0 (
..\bin\ImportTable.exe -c ..\data\marketvalue.csv
)