CALL SetEnvironment.cmd

@echo Please specify -i initialCapital -u CurrentCapital -b EndDate and optional -p positionFile

call PredicateStockSGDBv9.cmd %*
