namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [Serializable]
    public class InvalidMessageException : System.Exception
    {
        public InvalidMessageException()
            : base()
        {
        }

        public InvalidMessageException(string message)
            : base(message)
        {
        }

        public InvalidMessageException(string message, System.Exception inner)
            : base(message, inner)
        {
        }

        protected InvalidMessageException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
