using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NestorHub.Server.Domain.Interfaces;
using NestorHub.Server.Interfaces;

namespace NestorHub.Server.Controllers.HomeHub
{
    [Route("homehub/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscribeManager _subscriptionManager;
        private readonly IStateValueManagement _stateValueManager;
        private readonly ILogger<HomeControllerLogCategory> _logger;

        public SubscriptionController(ISubscribeManager subscriptionManager, IStateValueManagement stateValueManager, ILogger<HomeControllerLogCategory> logger)
        {
            _subscriptionManager = subscriptionManager;
            _stateValueManager = stateValueManager;
            _logger = logger;
        }

        [HttpGet]
        public List<SentinelSubscriptions> Get()
        {
            var subscriptionWithSentinel = new List<SentinelSubscriptions>();
            var subscriptions = _subscriptionManager.GetAllSubscriptions();
            foreach (var subscription in subscriptions)
            {
                var sentinelSubscription = new SentinelSubscriptions(subscription.Key);
                var subs = subscription.Value.Select(s => s.StateValueKey);
                sentinelSubscription.StateValueKeys.AddRange(subs);
                subscriptionWithSentinel.Add(sentinelSubscription);
            }
            return subscriptionWithSentinel;
        }

        [HttpPost]
        public async Task<ActionResult<string>> Post([FromBody] Subscription subscription)
        {
            var subscriptionKey = _subscriptionManager.Add(subscription);
            if (subscriptionKey.IsValid())
            {
                _logger.LogInformation($"New subscription for sentinel : {subscription.SentinelKey.Key} on {subscription.StateValueSentinelName} - {subscription.StateValuePackageName} - {subscription.StateValueName}");
            }

            await SendStateValueIfExistAfterSubscribiption(subscription);
            return subscriptionKey.Key;
        }

        [HttpDelete("{subscription}")]
        public IActionResult Delete(string subscription)
        {
            var subscriptionToRemove = ExtractSubscriptionFromUrlData(subscription);
            var isDeleted = _subscriptionManager.Delete(subscriptionToRemove.Item1, subscriptionToRemove.Item2);
            if (isDeleted)
            {
                _logger.LogInformation($"Subscription for sentinel : {subscriptionToRemove.Item1.Key} - Subscription ID {subscriptionToRemove.Item2.ToString()} as deleted");
            }
            return Ok(isDeleted);
        }

        private static Tuple<SentinelKey, SubscriptionKey> ExtractSubscriptionFromUrlData(string subscription)
        {
            var subscriptionData = subscription.Split('.');
            return new Tuple<SentinelKey, SubscriptionKey>(new SentinelKey(subscriptionData[0]), new SubscriptionKey(subscriptionData[1]));
        }

        private async Task SendStateValueIfExistAfterSubscribiption(Subscription subscription)
        {
            var stateValueKey = new StateValueKey(subscription.StateValueSentinelName,
                subscription.StateValuePackageName, subscription.StateValueName);

            if (_stateValueManager.Exist(stateValueKey))
            {
                var stateValue = _stateValueManager.GetStateValueByKey(stateValueKey);
                await _subscriptionManager.SendStateValueToSubscribers(stateValueKey, stateValue);
            }
        }
    }

    public class SentinelSubscriptions
    {
        public SentinelKey SentinelKey { get; set; }
        public List<StateValueKey> StateValueKeys { get; set; }

        public SentinelSubscriptions(SentinelKey sentinelKey)
        {
            SentinelKey = sentinelKey;
            StateValueKeys = new List<StateValueKey>();
        }
    }
}
