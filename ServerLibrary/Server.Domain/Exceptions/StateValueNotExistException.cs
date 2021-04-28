using System;
using System.Runtime.Serialization;
using NestorHub.Common.Api;

namespace NestorHub.Server.Domain.Exceptions
{
    [Serializable]
    public class StateValueNotExistException : Exception
    {
        protected StateValueNotExistException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx)
        { }

        public StateValueNotExistException(StateValueKey stateValueKey)
            : base($"State value {0} doesn't exist")
        { }
    }
}