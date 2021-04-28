using System.Collections.Generic;
using System.Threading.Tasks;
using NestorHub.Server.Domain.Class;

namespace NestorHub.Server.Interfaces
{
    public interface ISubscribeManager
    {
        SubscriptionKey Add(Subscription subscription);
        bool Delete(SentinelKey sentinelKey, SubscriptionKey subscriptionKey);
        Dictionary<SentinelKey, List<SubscriptionOnStateValue>> GetAllSubscriptions();
        Task SendStateValueToSubscribers(StateValueKey stateValueKey, StateValue value);
    }
}