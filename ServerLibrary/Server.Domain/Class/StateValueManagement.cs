using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NestorHub.Common.Api;
using NestorHub.Server.Domain.Exceptions;
using NestorHub.Server.Domain.Interfaces;

namespace NestorHub.Server.Domain.Class
{
    public delegate void AddingStateValueEventHandler(StateValueKey stateValueKey);
    public delegate void RemovingStateValueEventHandler(StateValueKey stateValueKey);

    public class StateValueManagement : IStateValueManagement
    {
        public event AddingStateValueEventHandler AddingStateValue;
        public event RemovingStateValueEventHandler RemovingStateValue;

        private readonly ConcurrentDictionary<StateValueKey, StateValue> _stateValues = new ConcurrentDictionary<StateValueKey, StateValue>();

        public void AddStateValue(StateValue stateValue)
        {
            var stateValueKey = CreateKeyForStateValue(stateValue);
            if (_stateValues.ContainsKey(stateValueKey))
            {
                throw new StateValueNotUniqueException();
            }
            _stateValues.AddOrUpdate(stateValueKey, stateValue, (key, value) => value);
            AddingStateValue?.Invoke(stateValueKey);
        }

        public bool Exist(StateValueKey stateValueKey)
        {
            return _stateValues.ContainsKey(stateValueKey);
        }

        public List<StateValue> GetAllStateValues()
        {
            return _stateValues.Select(s => s.Value).ToList();
        }

        public StateValue GetStateValueByKey(string sentinelName, string packageName, string name)
        {
            var stateValueKey = new StateValueKey(sentinelName, packageName, name);
            return GetStateValueByKey(stateValueKey);
        }

        public StateValue GetStateValueByKey(StateValueKey stateValueKey)
        {
            if (_stateValues.ContainsKey(stateValueKey))
            {
                return _stateValues[stateValueKey];
            }
            throw new NoStateValueFoundException(stateValueKey.Key);
        }

        public void UpdateStateValue(StateValue stateValue)
        {
            var stateValueKey = CreateKeyForStateValue(stateValue);
            if (!_stateValues.ContainsKey(stateValueKey))
            {
                throw new StateValueNotExistException(stateValueKey);
            }
            _stateValues[stateValueKey].Value = stateValue.Value;
        }

        public void RemoveStateValueByKey(string sentinelName, string packageName, string name)
        {
            var stateValueKey = GetStateValueKey(sentinelName, packageName, name);
            RemoveStateValueByKey(stateValueKey);
        }

        public void RemoveStateValueByKey(StateValueKey stateValueKey)
        {
            if (!_stateValues.ContainsKey(stateValueKey))
            {
                throw new NoStateValueFoundException(stateValueKey.Key);
            }

            StateValue valueRemoved;
            _stateValues.TryRemove(stateValueKey, out valueRemoved);    
            RemovingStateValue?.Invoke(stateValueKey);
        }

        public StateValueKey GetStateValueKey(string sentinelName, string packageName, string name)
        {
            return new StateValueKey(sentinelName, packageName, name);
        }

        private static StateValueKey CreateKeyForStateValue(StateValue stateValue)
        {
            return new StateValueKey(stateValue);
        }
    }
}