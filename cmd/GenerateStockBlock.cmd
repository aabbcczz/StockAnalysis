CALL SetEnvironment.cmd

COPY /Y %TDXROOT%\T0002\hq_cache\block_zs.dat %TEMPFOLDER%\
COPY /Y %TDXROOT%\T0002\hq_cache\tdxzs.cfg %TEMPFOLDER%\
COPY /Y %TDXROOT%\T0002\hq_cache\tdxhy.cfg %TEMPFOLDER%\


%BINROOT%\ConvertTdxBlockData.exe -z %TEMPFOLDER%\block_zs.dat -b %TEMPFOLDER%\tdxzs.cfg -o %TEMPFOLDER%\StockBlock.csv

%BINROOT%\ConvertTdxBlockData.exe -h %TEMPFOLDER%\tdxhy.cfg -b %TEMPFOLDER%\tdxzs.cfg -o %TEMPFOLDER%\StockBlock_HangYe.csv
