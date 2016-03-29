namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using FastRank;
    using Microsoft.TMSN.CommandLine;

    /// <summary>
    /// This class represents configuration of communication component
    /// </summary>
    public sealed class CommunicationConfiguration : CommandLineLikeComponentConfiguration
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.StyleCop.CSharp.MaintainabilityRules",
            "SA1401:FieldsMustBePrivate",
            Justification = "limit of command line parser")]
        [Argument(ArgumentType.LastOccurenceWins, HelpText = "max connection buildup time (sec), set <0 for infinite", ShortName = "mbt")]
        public int MaxConnectionBuildupTime = -1;

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.StyleCop.CSharp.MaintainabilityRules",
            "SA1401:FieldsMustBePrivate",
            Justification = "limit of command line parser")]
        [Argument(ArgumentType.LastOccurenceWins, HelpText = "number of messages accumulated before sending out", ShortName = "acm")]
        public int AccumulatedMessageCountBeforeSentOut = 20;

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.StyleCop.CSharp.MaintainabilityRules",
            "SA1401:FieldsMustBePrivate",
            Justification = "limit of command line parser")]
        [Argument(ArgumentType.LastOccurenceWins, HelpText = "times for messages accumulated before sending out (ms)", ShortName = "atm")]
        public int AccumulatedMessageTimeBeforeSentOut = 30;

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.StyleCop.CSharp.MaintainabilityRules",
            "SA1401:FieldsMustBePrivate",
            Justification = "limit of command line parser")]
        [Argument(ArgumentType.LastOccurenceWins, HelpText = "enable tcp Nagle algorithm", ShortName = "delay")]
        public bool TcpDelay = false;

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.StyleCop.CSharp.MaintainabilityRules",
            "SA1401:FieldsMustBePrivate",
            Justification = "limit of command line parser")]
        [Argument(ArgumentType.LastOccurenceWins, HelpText = "the number of thread to be reserved for message receiving", ShortName = "rtr")]
        public int ReceiveThreadReserved = 1;

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.StyleCop.CSharp.MaintainabilityRules",
            "SA1401:FieldsMustBePrivate",
            Justification = "limit of command line parser")]
        [Argument(ArgumentType.LastOccurenceWins, HelpText = "tcp send buffer size in KB", ShortName = "tcpsbs")]
        public int TcpSendBufferSizeInKB = 256;

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.StyleCop.CSharp.MaintainabilityRules",
            "SA1401:FieldsMustBePrivate",
            Justification = "limit of command line parser")]
        [Argument(ArgumentType.LastOccurenceWins, HelpText = "tcp receive buffer size in KB", ShortName = "tcprbs")]
        public int TcpReceiveBufferSizeInKB = 256;

        private const string ComponentName = "comm";

        public override string GetComponentName()
        {
            return CommunicationConfiguration.ComponentName;
        }

        public override void CheckParameters()
        {
            StaticRuntimeContext.Stdout.WriteLine("{0}", this);
        }
    }
}
