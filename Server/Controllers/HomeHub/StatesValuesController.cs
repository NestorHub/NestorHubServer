using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NestorHub.Server.Domain.Exceptions;
using NestorHub.Server.Domain.Interfaces;
using NestorHub.Server.Hubs;

namespace NestorHub.Server.Controllers.HomeHub
{
    [Route("homehub/[controller]")]
    [ApiController]
    public class StatesValuesController : ControllerBase
    {
        private readonly IStateValueManagement _stateManager;
        private readonly ILogger<HomeControllerLogCategory> _logger;
        private readonly IHubContext<StateValueHub, IStateValueHub> _stateValueHubContext;

        public StatesValuesController(IStateValueManagement stateManager, ILogger<HomeControllerLogCategory> logger, IHubContext<StateValueHub, IStateValueHub> stateValueHubContext)
        {
            _stateManager = stateManager;
            _logger = logger;
            _stateValueHubContext = stateValueHubContext;
        }

        [HttpGet]
        public IEnumerable<StateValue> Get()
        {
            return _stateManager.GetAllStateValues().OrderBy(s => s.SentinelName).ThenBy(s => s.Name).ThenBy(s => s.LastUpdate);
        }

        [HttpPost]
        public ActionResult<StateValueKey> Post([FromBody] StateValue stateValue)
        {
            var stateValueKey =
                _stateManager.GetStateValueKey(stateValue.SentinelName, stateValue.PackageName, stateValue.Name);
            if (!_stateManager.Exist(_stateManager.GetStateValueKey(stateValue.SentinelName, stateValue.PackageName, stateValue.Name)))
            {
                _stateManager.AddStateValue(stateValue);
                _logger.LogInformation($"State value {stateValueKey} with value {stateValue.Value}, last update : {stateValue.LastUpdate}, was added to store");
            }
            else
            {
                _stateManager.UpdateStateValue(stateValue);
                _logger.LogInformation($"State value {stateValueKey} with value {stateValue.Value}, last update : {stateValue.LastUpdate}, was updated to store");
            }

            _stateValueHubContext.Clients.All.AddStateValue(stateValueKey, stateValue);
            return stateValueKey;
        }

        [HttpDelete("{statevaluekey}")]
        public IActionResult Delete(string stateValueKey)
        {
            try
            {
                var stateValueKeyObject = new StateValueKey(stateValueKey);
                if (!_stateManager.Exist(stateValueKeyObject))
                {
                    _stateManager.RemoveStateValueByKey(stateValueKeyObject);
                    _logger.LogInformation($"State value {stateValueKey} was remove to store");

                }
                return Ok(true);
            }
            catch (Exception ex) when (ex is StateValueKeyBadFormatException || ex is NoStateValueFoundException || ex is StateValueKeyEmptyException)
            {
                LogErrorOnDelete(stateValueKey, ex);
                return Ok(false);
            }
        }

        private void LogErrorOnDelete(string stateValueKey, Exception ex)
        {
            if (ex is StateValueKeyBadFormatException)
            {
                _logger.LogError($"{stateValueKey} is a bad format for state value key");
            }

            if (ex is NoStateValueFoundException)
            {
                _logger.LogError($"{stateValueKey} not found in store");
            }

            if (ex is StateValueKeyEmptyException)
            {
                _logger.LogError($"An empty key is not acceptable for state value key");
            }
        }
    }
}
