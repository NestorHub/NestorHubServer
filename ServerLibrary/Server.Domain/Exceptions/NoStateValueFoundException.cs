using System;
using System.Runtime.Serialization;

namespace NestorHub.Server.Domain.Exceptions
{
    [Serializable]
    public class NoStateValueFoundException : Exception
    {
        protected NoStateValueFoundException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx)
        {}

        public NoStateValueFoundException(string stateValueKey)
            : base($"No state value found with key : {stateValueKey}")
        {}
    }
}
