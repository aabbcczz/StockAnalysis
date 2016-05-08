namespace ParallelFastRank
{
    public enum MessageType
    {
        None = 0,
        TimeSync,
        WorkerMachineInfo,  // master aggregate message
        WorkerMachineList,  // master broadcast message
        PhaseIndex,  // worker broadcast message
        StateIterations, // worker broadcast message
        LocalFeatureBoundary,    // worker broadcast message
        LocalFeatureMD5Hash,     // master aggregate message
        GlobalUsedFeatureIdx,    // master broadcast message
        GlobalActiveFeatureIdx,  // master broadcast message
        SubHistogram,    // worker-to-worker message
        SplitInfo,       // worker broadcast message
        EndSplitInfo,   // worker broadcast message
        LocalRegressionTree,  // worker broadcast message
        LocalTestResults,
        FeatureVector,
        FeatureVectorPackage,
        GlobalBestSplitInfo,
        GlobalBestSplitInfoPackage,
        AskForStop,
        SubHistogramPackage,  // merge multi message send to same worker to one package
        SplitInfoPackage,
        FeatureBinPackage,

        /// <summary>
        /// The tree selection metric message for multi-task learning. It's a worker broadcast message.
        /// </summary>
        TreeSelectionMetric, 

        /// <summary>
        /// The message for syncing training state of tasks for multi-task learning. It's a worker broadcast message.
        /// </summary>
        TaskIterationSync,

        DataScale,

        TypeCount
    }
}
