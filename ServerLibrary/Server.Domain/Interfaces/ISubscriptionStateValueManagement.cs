using System.Collections.Generic;
using NestorHub.Common.Api;
using NestorHub.Server.Domain.Class;

namespace NestorHub.Server.Domain.Interfaces
{
    public interface ISubscriptionStateValueManagement
    {
        event SubscriptionValueChangedEventHandler SubscriptionValueChanged;
        SubscriptionKey AddSubscription(SentinelKey clientSentinelKey, string stateValueSentinelName, string stateValuePackageName, string stateValueName);
        bool RemoveSubscription(SentinelKey sentinelKey, SubscriptionKey subscriptionId);
        Dictionary<SentinelKey, List<SubscriptionOnStateValue>> GetAllSubscriptions();
    }
}