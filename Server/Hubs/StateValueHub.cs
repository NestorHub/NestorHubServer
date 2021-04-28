using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace NestorHub.Server.Hubs
{
    public class StateValueHub : Hub<IStateValueHub>
    {}

    public interface IStateValueHub
    {
        Task AddStateValue(StateValueKey stateValueKey, StateValue stateValue);
    }
}
