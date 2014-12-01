CALL SetEnvironment.cmd

%BINROOT%\EvaluatorCmdClient -t %SETTINGSROOT%\tradingsettings.xml -c %SETTINGSROOT%\strategysettings.xml -s %SETTINGSROOT%\stockdatasettings.xml -o %STOCKROOT%\codefile.txt -v 2 -i 1000000.0 -a 2012-01-01 -b 2015-12-31 -w 300 -n stocktest -p -k %STOCKROOT%\stockblock.csv %*