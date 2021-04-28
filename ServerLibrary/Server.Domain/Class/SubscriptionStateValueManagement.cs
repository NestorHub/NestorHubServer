using System.Collections.Generic;
using System.Linq;
using NestorHub.Common.Api;
using NestorHub.Server.Domain.Exceptions;
using NestorHub.Server.Domain.Interfaces;

namespace NestorHub.Server.Domain.Class
{
    public delegate void SubscriptionValueChangedEventHandler(StateValueKey stateValueKey, StateValue value);

    public class SubscriptionStateValueManagement : ISubscriptionStateValueManagement
    {
        public event SubscriptionValueChangedEventHandler SubscriptionValueChanged;

        private readonly IStateValueManagement _stateValueManager;
        private readonly Dictionary<SentinelKey, List<SubscriptionOnStateValue>> _subscriptions = new Dictionary<SentinelKey, List<SubscriptionOnStateValue>>();
        private readonly List<StateValueKey> _stateValueChangedSubscribe = new List<StateValueKey>();

        public SubscriptionStateValueManagement(IStateValueManagement stateValueManager)
        {
            _stateValueManager = stateValueManager;
            _stateValueManager.AddingStateValue += StateValueManagerAddingStateValue;
            _stateValueManager.RemovingStateValue += StateValueManagerRemovingStateValue;
        }

        public Dictionary<SentinelKey, List<SubscriptionOnStateValue>> GetAllSubscriptions()
        {
            return _subscriptions;
        }

        public SubscriptionKey AddSubscription(SentinelKey clientSentinelKey, string stateValueSentinelName, string stateValuePackageName, string stateValueName)
        {
            var stateValueKey = _stateValueManager.GetStateValueKey(stateValueSentinelName, stateValuePackageName, stateValueName);

            var subscriptionOnStateValue = SubscriptionOnStateValue.CreateSubscriptionOnStateValue(
                _stateValueManager.GetStateValueKey(stateValueSentinelName, stateValuePackageName, stateValueName));

            if (_subscriptions.ContainsKey(clientSentinelKey))
            {
                var stateValueKeys = _subscriptions[clientSentinelKey].Where(v => v.StateValueKey == stateValueKey);
                if (stateValueKeys.Any())
                {
                    stateValueKeys.First().Id = subscriptionOnStateValue.Id;
                }
                else
                {
                    _subscriptions[clientSentinelKey].Add(subscriptionOnStateValue);
                }
            }
            else
            {
                _subscriptions.Add(clientSentinelKey, new List<SubscriptionOnStateValue> { subscriptionOnStateValue });
            }

            SubscribeOnValueChangedEventIfStateValueExist(stateValueKey);
            return subscriptionOnStateValue.Id;
        }

        public bool RemoveSubscription(SentinelKey sentinelKey, SubscriptionKey subscriptionId)
        {
            if (_subscriptions.Any(s => s.Key == sentinelKey))
            {
                _subscriptions[sentinelKey].RemoveAll(s => s.Id == subscriptionId);

                RemoveSubscriptionSentinelKeyEntryIfEmpty(sentinelKey);
                return true;
            }
            else
            {
                throw new NoSubscriptionFoundException();
            }
        }

        private void StateValueManagerRemovingStateValue(StateValueKey stateValueKey)
        {
            RemoveStateValueChangedSubscriptionIfStateValueKeyDoesNotExistInSubscriptions(stateValueKey);
        }

        private void StateValueManagerAddingStateValue(StateValueKey stateValueKey)
        {
            SubscribeOnValueChangedEventIfStateValueExist(stateValueKey);
            var stateValue = _stateValueManager.GetStateValueByKey(stateValueKey);
            StateValueChanged(stateValue);
        }

        private void SubscribeOnValueChangedEventIfStateValueExist(StateValueKey stateValueKey)
        {
            if (_stateValueManager.Exist(stateValueKey))
            {
                var stateValue =
                    _stateValueManager.GetStateValueByKey(stateValueKey);

                if (!_stateValueChangedSubscribe.Contains(stateValueKey))
                {
                    stateValue.ValueChanged += StateValueChanged;
                    _stateValueChangedSubscribe.Add(stateValueKey);
                }
            }
        }

        private void StateValueChanged(StateValue stateValue)
        {
            if (SubscriptionValueChanged != null)
            {
                var isSubscribe = false;
                var stateValueKeyToSearch = _stateValueManager.GetStateValueKey(stateValue.SentinelName, stateValue.PackageName,
                    stateValue.Name);
                foreach (var subscription in _subscriptions)
                {
                    var stateValuesKeys = subscription.Value.Where(s => s.StateValueKey == stateValueKeyToSearch);
                    foreach (var stateValueKey in stateValuesKeys)
                    {
                        isSubscribe = true;
                    }
                }

                if (isSubscribe)
                {
                    SubscriptionValueChanged(stateValueKeyToSearch, stateValue);
                }
            }
        }

        private void RemoveSubscriptionSentinelKeyEntryIfEmpty(SentinelKey sentinelKey)
        {
            if (!_subscriptions[sentinelKey].Any())
            {
                _subscriptions.Remove(sentinelKey);
            }
        }

        private void RemoveStateValueChangedSubscriptionIfStateValueKeyDoesNotExistInSubscriptions(StateValueKey stateValueKeyToRemove)
        {
            var stateValueIncluded = false;
            foreach (var subscription in _subscriptions)
            {
                foreach (var subscriptionOnStateValue in subscription.Value)
                {
                    if (subscriptionOnStateValue.StateValueKey == stateValueKeyToRemove)
                    {
                        stateValueIncluded = true;
                    }
                }
            }

            if (!stateValueIncluded)
            {
                var stateValue =
                    _stateValueManager.GetStateValueByKey(stateValueKeyToRemove);
                stateValue.ValueChanged -= StateValueChanged;
                _stateValueChangedSubscribe.Remove(stateValueKeyToRemove);
            }
        }
    }
}