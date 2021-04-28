using System;
using System.Runtime.Serialization;

namespace NestorHub.Server.Domain.Exceptions
{
    [Serializable]
    public class NoSubscriptionFoundException : Exception
    {
        protected NoSubscriptionFoundException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx)
        { }

        public NoSubscriptionFoundException()
            : base($"No subscription found")
        { }
    }
}