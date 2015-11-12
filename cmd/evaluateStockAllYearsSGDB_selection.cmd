CALL SetEnvironment.cmd

%BINROOT%\EvaluatorCmdClient -t %SETTINGSROOT%\tradingsettings_S_GDB_selection.xml -c %SETTINGSROOT%\strategysettings_S_GDB_selection.xml -s %SETTINGSROOT%\stockdatasettings.xml -o %STOCKROOT%\codefile.txt -v 2 -i 1000000.0 -a 2012-01-01 -b 2015-12-31 -w 60 -n stocktestSGDB_selection -p %*