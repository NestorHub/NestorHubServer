using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NestorHub.Sentinels.Domain.Class;
using NestorHub.Sentinels.Domain.Interfaces;

namespace NestorHub.Server.Controllers.PackagesHub
{
    [Route("packageshub/[controller]")]
    [ApiController]
    public class PackagesInstancesController : ControllerBase
    {
        private readonly IPackagesInstances _packagesInstances;
        private readonly IPackageRunner _packageRunner;

        public PackagesInstancesController(IPackagesInstances packagesInstances, IPackageRunner packageRunner)
        {
            _packagesInstances = packagesInstances;
            _packageRunner = packageRunner;
        }

        [HttpGet]
        public IEnumerable<PackageToRunWithStatus> Get()
        {
            var instancesWithStatus = new List<PackageToRunWithStatus>();
            var instances = _packagesInstances.GetAllInstancesToRun();
            foreach (var packageToRun in instances)
            {
                instancesWithStatus.Add(new PackageToRunWithStatus(packageToRun, _packageRunner.IsRunning(packageToRun.InstanceId)));    
            }
            return instancesWithStatus.OrderBy(i => i.Instance.RunOrder);
        }

        [HttpGet]
        [Route("nextrunorderindex")]
        public int GetNewRunOrderIndex()
        {
            return _packagesInstances.GetNextRunOrderIndex();
        }

        [HttpPost]
        public void Post([FromBody] PackageToRun instance)
        {
            _packagesInstances.AddInstanceToRun(instance);
        }

        [HttpGet]
        [Route("runall")]
        public void RunAll()
        {
            _packageRunner.StopAllInstances();
            _packageRunner.RunAllInstances();
        }

        [HttpGet]
        [Route("stopall")]
        public void StopAll()
        {
            _packageRunner.StopAllInstances();
        }

        [HttpPost]
        [Route("stop")]
        public void Stop([FromBody] Guid id)
        {
            if (id != Guid.Empty)
            {
                _packageRunner.StopInstance(id);
            }
        }

        [HttpPost]
        [Route("start")]
        public void Start([FromBody] Guid id)
        {
            if (id != Guid.Empty)
            {
                _packageRunner.RunInstance(id);
            }
        }

        [HttpPost]
        [Route("delete")]
        public void Delete([FromBody] Guid id)
        {
            _packagesInstances.DeleteInstance(id);
        }
    }
}
