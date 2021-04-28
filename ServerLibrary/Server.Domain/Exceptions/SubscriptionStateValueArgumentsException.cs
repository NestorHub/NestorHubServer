using System;
using System.Runtime.Serialization;

namespace NestorHub.Server.Domain.Exceptions
{
    [Serializable]
    public class SubscriptionStateValueArgumentsException : Exception
    {
        public SubscriptionStateValueArgumentsException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx)
        { }

        public SubscriptionStateValueArgumentsException()
            : base($"Sentinel key or StateValue cannot not be null or empty")
        { }
    }
}