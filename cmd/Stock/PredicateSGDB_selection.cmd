CALL SetEnvironment.cmd

@echo Please specify -b EndDate and optional -p positionFile

%BINROOT%\PredicatorCmdClient -c %SETTINGSROOT%\strategysettings_S_GDB_selection.xml -s %SETTINGSROOT%\stockdatasettings.xml -o %TEMPFOLDER%\code.txt -v 2 -a 2012-01-01 -w 60 -n _predicateSGDB_selection -i 1000000.0 -u 1000000.0 %*