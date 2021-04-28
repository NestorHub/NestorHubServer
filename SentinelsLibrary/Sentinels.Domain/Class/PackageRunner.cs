using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using NestorHub.Common.Api.Interfaces;
using NestorHub.Common.Api.Mef;
using NestorHub.Sentinels.Domain.Interfaces;

namespace NestorHub.Sentinels.Domain.Class
{
    public class PackageRunner : IPackageRunner
    {
        private readonly string _pathToStore;
        private readonly IPackagesInstances _packagesInstances;
        private readonly IHostingConfiguration _hostingConfiguration;
        private readonly Dictionary<Guid, PackageInstance> _instancesRunning = new Dictionary<Guid, PackageInstance>();

        public PackageRunner(string pathToStore, IPackagesInstances packagesInstances, IHostingConfiguration hostingConfiguration)
        {
            _pathToStore = pathToStore;
            _packagesInstances = packagesInstances;
            _hostingConfiguration = hostingConfiguration;
        }

        public bool IsRunning(Guid instanceId)
        {
            return _instancesRunning.Any(i => i.Key == instanceId);
        }

        public void RunAllInstances()
        {
            var packagesToRun = _packagesInstances.GetAllInstancesToRun();
            RunListOfPackages(packagesToRun);
        }

        public void RunAllInstancesOnServerStart()
        {
            var packagesToRun = _packagesInstances.GetAllInstancesToRun().Where(p => p.RunOnServerStart).OrderBy(p => p.RunOrder);
            RunListOfPackages(packagesToRun);
        }

        public async Task<bool> StopPackage(string packageId)
        {
            var instancesToRemove = new List<Guid>();
            var instancesToStop = _instancesRunning.Where(i => i.Value.PackageToRun.PackageId == packageId);
            foreach (var instanceRunning in instancesToStop)
            {
                if (await StopInstance(instanceRunning.Value.Instance))
                {
                    instancesToRemove.Add(instanceRunning.Key);
                }
            }

            var instanceToStop = instancesToStop.Count();
            foreach (var guid in instancesToRemove)
            {
                _instancesRunning.Remove(guid);
            }

            return instanceToStop == instancesToRemove.Count;
        }

        public async Task<bool> StopAllInstances()
        {
            try
            {
                var allStop = new Dictionary<Guid, bool>();
                var instancesToStop = new List<Guid>(_instancesRunning.Select(i => i.Key));
                foreach (var instance in instancesToStop)
                {
                    var stop = await StopInstance(instance);
                    allStop.Add(instance, stop);
                }
                return allStop.All(a => a.Value != false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> StopInstance(Guid instanceId)
        {
            var instances = _instancesRunning.Where(i => i.Key == instanceId);
            if (instances.Any())
            {
                var instance = instances.First();
                var instanceObject = instance.Value.Instance;
                if (await StopInstance(instanceObject))
                {
                    _instancesRunning.Remove(instance.Key);
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> RunInstance(Guid instanceId)
        {
            if (!_instancesRunning.ContainsKey(instanceId))
            {
                var packageToRun = _packagesInstances.GetInstanceToRunById(instanceId);
                if (packageToRun != null)
                {
                    var assemblies = Directory
                        .GetFiles(_pathToStore, "*.dll", SearchOption.AllDirectories)
                        .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath)
                        .ToList();
                    return await RunInstance(packageToRun, assemblies);
                }
            }

            return false;
        }

        private async void RunListOfPackages(IEnumerable<PackageToRun> packagesToRun)
        {
            var assemblies = Directory
                .GetFiles(_pathToStore, "*.dll", SearchOption.AllDirectories)
                .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath)
                .ToList();

            _instancesRunning.Clear();
            foreach (var packageToRun in packagesToRun.OrderBy(p => p.RunOrder))
            {
                var res = await RunInstance(packageToRun, assemblies);
            }
        }

        private async Task<bool> RunInstance(PackageToRun packageToRun, List<Assembly> assemblies)
        {
            var packageInstance = RunInstanceOfPackage(packageToRun.PackageId, assemblies);

            if (packageInstance != null)
            {
                var running = await packageInstance.Run(packageToRun.InstanceName, packageToRun.PackageId,
                    _hostingConfiguration.GetAddressServer(), _hostingConfiguration.GetPortServer(), _hostingConfiguration.GetUseSsl(),
                    packageToRun.Parameter);
                if (running)
                {
                    _instancesRunning.Add(packageToRun.InstanceId, new PackageInstance(packageToRun, packageInstance));
                }

                return running;
            }

            return false;
        }

        private async Task<bool> StopInstance(ISentinel instance)
        {
            try
            {
                var stopped = await instance.Stop();
                if (stopped)
                {
                    instance = null;
                    GC.Collect();
                }
                return stopped;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static ISentinel RunInstanceOfPackage(string packageId, List<Assembly> assemblies)
        {
            var configuration = new ContainerConfiguration()
                .WithAssemblies(assemblies);
            using (var container = configuration.CreateContainer())
            {
                return container.GetExport<ISentinel>(packageId);
            }
        }
    }

    internal class PackageInstance
    {
        public PackageToRun PackageToRun { get; }
        public ISentinel Instance { get; set; }

        public PackageInstance(PackageToRun packagePackageToRun, ISentinel instance)
        {
            PackageToRun = packagePackageToRun;
            Instance = instance;
        }
    }
}
