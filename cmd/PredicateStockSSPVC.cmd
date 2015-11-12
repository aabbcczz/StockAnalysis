CALL SetEnvironment.cmd

@echo Please specify -b EndDate and optional -p positionFile

%BINROOT%\PredicatorCmdClient -c %SETTINGSROOT%\strategysettings_SS_PVC.xml -s %SETTINGSROOT%\stockdatasettings.xml -o %STOCKROOT%\codefile.txt -v 2 -a 2012-01-01 -w 300 -n stockpredicateSSPVC -k %STOCKROOT%\stockblock.csv -i 1000000.0 -u 1000000.0 %*