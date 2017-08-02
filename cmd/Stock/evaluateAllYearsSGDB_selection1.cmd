CALL SetEnvironment.cmd

%BINROOT%\EvaluatorCmdClient -t %SETTINGSROOT%\tradingsettings_S_GDB_selection.xml -c %SETTINGSROOT%\strategysettings_S_GDB_selection.xml -s %SETTINGSROOT%\stockdatasettings.xml -o %TEMPFOLDER%\code.txt -v 2 -i 1000000.0 -w 60 -n _evaluateSGDB_selection -p %*