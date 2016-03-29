namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public sealed class DataProcessExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; set; }

        public object Context { get; set; }
    }
}
