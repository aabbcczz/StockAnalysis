CALL SetEnvironment.cmd

%BINROOT%\EvaluatorCmdClient -t %SETTINGSROOT%\tradingsettings.xml -c %SETTINGSROOT%\strategysettings.xml -s %SETTINGSROOT%\stockdatasettings.xml -o .%SETTINGSROOT%\codefile.txt -v 2 -i 1000000.0 -a 2000-01-01 -b 2013-12-31 -w 300 -n stocktest -k %STOCKROOT%\stockblock.csv -y 3 %*