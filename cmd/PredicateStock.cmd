CALL SetEnvironment.cmd

@echo Please specify -i initialCapital -u CurrentCapital -b EndDate and optional -p positionFile

%BINROOT%\PredicatorCmdClient -c %SETTINGSROOT%\strategysettings.xml -s %SETTINGSROOT%\stockdatasettings.xml -o %STOCKROOT%\codefile.txt -v 2 -a 2012-01-01 -w 300 -n stockpredicate -k %STOCKROOT%\stockblock.csv %*