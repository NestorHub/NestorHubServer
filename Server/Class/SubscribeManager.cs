using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NestorHub.Server.Domain.Class;
using NestorHub.Server.Domain.Interfaces;
using NestorHub.Server.Hubs;
using NestorHub.Server.Interfaces;

namespace NestorHub.Server.Class
{
    public class SubscribeManager : ISubscribeManager
    {
        private readonly ISubscriptionStateValueManagement _subscriptionManager;
        private readonly IHubContext<SubscriptionsHub, ISubscriptionsHub> _subscriptionsHubContext;

        public SubscribeManager(IStateValueManagement stateValueManagement, IHubContext<SubscriptionsHub, ISubscriptionsHub> subscriptionsHubContext)
        {
            _subscriptionManager = new SubscriptionStateValueManagement(stateValueManagement);
            _subscriptionManager.SubscriptionValueChanged += OnSubscriptionValueChanged;
            _subscriptionsHubContext = subscriptionsHubContext;
        }

        public SubscriptionKey Add(Subscription subscription)
        {
            return _subscriptionManager.AddSubscription(subscription.SentinelKey, subscription.StateValueSentinelName, subscription.StateValuePackageName,
                subscription.StateValueName);
        }

        public bool Delete(SentinelKey sentinelKey, SubscriptionKey subscriptionKey)
        {
            return _subscriptionManager.RemoveSubscription(sentinelKey, subscriptionKey);
        }

        public Dictionary<SentinelKey, List<SubscriptionOnStateValue>> GetAllSubscriptions()
        {
            return _subscriptionManager.GetAllSubscriptions();
        }

        public async Task SendStateValueToSubscribers(StateValueKey stateValueKey, StateValue value)
        {
            await _subscriptionsHubContext.Clients.Group(stateValueKey.Key).ValueChanged(stateValueKey, value);
        }

        private async void OnSubscriptionValueChanged(StateValueKey stateValueKey, StateValue value)
        {
            await SendStateValueToSubscribers(stateValueKey, value);
        }
    }
}
