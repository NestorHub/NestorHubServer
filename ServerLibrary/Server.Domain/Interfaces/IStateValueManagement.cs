using System.Collections.Generic;
using NestorHub.Common.Api;
using NestorHub.Server.Domain.Class;

namespace NestorHub.Server.Domain.Interfaces
{
    public interface IStateValueManagement
    {
        event AddingStateValueEventHandler AddingStateValue;
        event RemovingStateValueEventHandler RemovingStateValue;
        void AddStateValue(StateValue stateValue);
        bool Exist(StateValueKey stateValueKey);
        List<StateValue> GetAllStateValues();
        StateValue GetStateValueByKey(string sentinelName, string packageName, string name);
        StateValue GetStateValueByKey(StateValueKey stateValueKey);
        void RemoveStateValueByKey(string sentinelName, string packageName, string name);
        void RemoveStateValueByKey(StateValueKey stateValueKey);
        StateValueKey GetStateValueKey(string sentinelName, string packageName, string name);
        void UpdateStateValue(StateValue stateValue);
    }
}