using NestorHub.Common.Api;
using NestorHub.Common.Api.Enum;
using NestorHub.Server.Domain.Class;
using NestorHub.Server.Domain.Exceptions;
using NFluent;
using Xunit;

namespace NestorHub.ServerLibrary.Tests
{
    public class SubscriptionStateValueManagementShould
    {
        [Fact]
        public void return_0_subscription()
        {
            var subscriptionManager = new SubscriptionStateValueManagement(new StateValueManagement());
            Check.That(subscriptionManager.GetAllSubscriptions().Count).IsEqualTo(0);
        }

        [Fact]
        public void return_at_least_1_when_adding_a_new_subscription()
        {
            var stateManager = new StateValueManagement();
            stateManager.AddStateValue(new StateValue("sentinel1", "package1", "statename", 3, TypeOfValue.Int));

            var subscriptionManager = new SubscriptionStateValueManagement(stateManager);
            subscriptionManager.AddSubscription(new SentinelKey("sentinel.package"), "sentinel1", "package1", "statename");
            Check.That(subscriptionManager.GetAllSubscriptions().Count).IsStrictlyGreaterThan(0);
        }

        [Fact]
        public void return_0_when_adding_and_then_remove_subscription()
        {
            var stateManager = new StateValueManagement();
            stateManager.AddStateValue(new StateValue("sentinel1", "package1", "statename", 3, TypeOfValue.Int));

            var subscriptionManager = new SubscriptionStateValueManagement(stateManager);
            var subscriptionId = subscriptionManager.AddSubscription(new SentinelKey("sentinel.package"), "sentinel1", "package1", "statename");
            subscriptionManager.RemoveSubscription(new SentinelKey("sentinel.package"), subscriptionId);

            Check.That(subscriptionManager.GetAllSubscriptions().Count).IsEqualTo(0);
        }

        [Fact]
        public void return_1_when_adding_and_then_remove_one_subscription()
        {
            var stateManager = new StateValueManagement();
            stateManager.AddStateValue(new StateValue("sentinel1", "package1", "statename", 3, TypeOfValue.Int));
            stateManager.AddStateValue(new StateValue("sentinel1", "package2", "statename", 3, TypeOfValue.Int));

            var subscriptionManager = new SubscriptionStateValueManagement(stateManager);
            var subscription1Id = subscriptionManager.AddSubscription(new SentinelKey("sentinel.package"), "sentinel1", "package1", "statename");
            var subscription2Id = subscriptionManager.AddSubscription(new SentinelKey("sentinel.package"), "sentinel1", "package2", "statename");
            subscriptionManager.RemoveSubscription(new SentinelKey("sentinel.package"), subscription1Id);

            Check.That(subscriptionManager.GetAllSubscriptions()[new SentinelKey("sentinel.package")].Count).IsEqualTo(1);
        }

        [Fact]
        public void throw_no_subscription_found_exception_when_adding_a_subscription()
        {
            var stateManager = new StateValueManagement();
            stateManager.AddStateValue(new StateValue("sentinel1", "package1", "statename", 3, TypeOfValue.Int));

            var subscriptionManager = new SubscriptionStateValueManagement(stateManager);
            var subscriptionId = subscriptionManager.AddSubscription(new SentinelKey("sentinel.package"), "sentinel1", "package1",
                "statename");

            Check.ThatCode(() => subscriptionManager.RemoveSubscription(new SentinelKey("sentinel.package1"), subscriptionId)
                ).Throws<NoSubscriptionFoundException>();
        }
    }
}
