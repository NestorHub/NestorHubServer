using System;
using System.Runtime.Serialization;

namespace NestorHub.Server.Domain.Exceptions
{
    [Serializable]
    public class StateValueNotUniqueException : Exception
    {
        protected StateValueNotUniqueException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx)
        { }

        public StateValueNotUniqueException()
            : base("Sentinel key is not unique in manager")
        { }
    }
}