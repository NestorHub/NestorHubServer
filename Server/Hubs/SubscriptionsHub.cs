using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace NestorHub.Server.Hubs
{
    public class SubscriptionsHub : Hub<ISubscriptionsHub>
    {
        public async Task AddToGroup(string stateValueKey)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, stateValueKey);
        }

        public async Task RemoveToGroup(string stateValueKey)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, stateValueKey);
        }
    }

    public interface ISubscriptionsHub
    {
        Task ValueChanged(StateValueKey stateValueKey, object value);
    }
}
