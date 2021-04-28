using System;
using NestorHub.Common.Api;

namespace NestorHub.Server.Domain.Class
{
    public class SubscriptionOnStateValue
    {
        public SubscriptionKey Id { get; set; }
        public StateValueKey StateValueKey { get;  }

        private SubscriptionOnStateValue(SubscriptionKey id, StateValueKey stateValueKey)
        {
            Id = id;
            StateValueKey = stateValueKey;
        }

        public static SubscriptionOnStateValue CreateSubscriptionOnStateValue(StateValueKey stateValueKey)
        {
            return new SubscriptionOnStateValue(new SubscriptionKey(Guid.NewGuid().ToString()), stateValueKey);
        }
    }
}