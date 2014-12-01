CALL SetEnvironment.cmd

COPY /Y %TDXROOT%\T0002\hq_cache\block_zs.dat %STOCKROOT%\
COPY /Y %TDXROOT%\T0002\hq_cache\tdxzs.cfg %STOCKROOT%\

%BINROOT%\ConvertTdxBlockData.exe -z %STOCKROOT%\block_zs.dat -b %STOCKROOT%\tdxzs.cfg -o %STOCKROOT%\StockBlock.csv