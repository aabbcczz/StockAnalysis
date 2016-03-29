namespace ParallelFastRank
{
    public static class CommunicationTimeout
    {
        public const int ThousandMillisecond = 1000;

#if SHORT_TIMEOUT_TEST
        public const int PrepareTimeout = ThousandMillisecond * 60; // 60 sec for receive each feature distribution and MD5Hash
        public const int PhaseTimeout = ThousandMillisecond * 2;  // 2 sec for phase information sync-up
        public const int RoundTimeout = ThousandMillisecond * 2;  // 2 sec for round information (like tree) sync-up
        public const int RestoreStateTimeout = ThousandMillisecond * 30; // 30 seconds for restoring/syncing parallel training state
        public const int StepTimeout = ThousandMillisecond; // 1 sec for step information (like histogram) sync-up
        
        static CommunicationTimeout()
        {
            ConnectionTimeout = ThousandMillisecond * 30; // 30 sec for master-worker connection built up
        }

        public static int ConnectionTimeout { get; set; } 
#else
        public const int PrepareTimeout = ThousandMillisecond * 7200; // 2 hours for receive each feature distribution and MD5Hash
        public const int PhaseTimeout = ThousandMillisecond * 1800;  // 30 min for phase information sync-up
        public const int RoundTimeout = ThousandMillisecond * 1800;  // 30 min for round information (like tree) sync-up
        public const int RestoreStateTimeout = ThousandMillisecond * 7200; // 2 hours for restoring/syncing parallel training state
        public const int StepTimeout = ThousandMillisecond * 600;  // 10 min for step information (like histogram) sync-up
        public const int DataScaleTimeout = ThousandMillisecond * 1800;  // 30 min for data scale information sync-up
        public const int TaskSyncTimeOut = ThousandMillisecond * 1 * 3600; // 5 hours for receiving task (like tree selection) syncing
        
        static CommunicationTimeout()
        {
            ConnectionTimeout = ThousandMillisecond * 3600;  // 1 hour for master-worker connection built up
        }

        public static int ConnectionTimeout { get; set; }
#endif
    }
}