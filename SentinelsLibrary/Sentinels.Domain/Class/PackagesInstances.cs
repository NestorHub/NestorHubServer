using System;
using System.Collections.Generic;
using System.Linq;
using NestorHub.Sentinels.Domain.Interfaces;

namespace NestorHub.Sentinels.Domain.Class
{
    public class PackagesInstances : IPackagesInstances
    {
        private readonly List<PackageToRun> _packagesToRun;
        private readonly IPackageToRunStorage _packageToRunStorage;

        public PackagesInstances(IPackageToRunStorage packageToRunStorage)
        {
            _packageToRunStorage = packageToRunStorage;
            _packagesToRun = _packageToRunStorage.LoadFromPersistence();
        }

        public void AddInstanceToRun(PackageToRun packageToRun)
        {
            var packagesToRun = _packagesToRun.Where(p =>
                    p.PackageId == packageToRun.PackageId && p.InstanceName == packageToRun.InstanceName);

            if (!packagesToRun.Any())
            {
                AddInstance(packageToRun);
            }
            else
            {
                var instancePackage = packagesToRun.First();
                UpdateInstance(packageToRun, instancePackage);
            }
            Persist();
        }

        public void DeleteInstance(Guid id)
        {
            if (id != Guid.Empty)
            {
                _packagesToRun.RemoveAll(p => p.InstanceId == id);
                Persist();
            }
        }

        public void DeleteInstances(IEnumerable<PackageToRun> packagesToDelete)
        {
            var packagesId = packagesToDelete.Select(p => p.PackageId);
            _packagesToRun.RemoveAll(p => packagesId.Contains(p.PackageId));
            Persist();
        }

        public IEnumerable<PackageToRun> GetInstancesToRunByPackageId(string packageId)
        {
            return _packagesToRun.Where(p => p.PackageId == packageId);
        }

        public PackageToRun GetInstanceToRunById(Guid instanceId)
        {
            return _packagesToRun.FirstOrDefault(p => p.InstanceId == instanceId);
        }

        public IEnumerable<PackageToRun> GetAllInstancesToRun()
        {
            return _packagesToRun;
        }

        public int GetNextRunOrderIndex()
        {
            if (!_packagesToRun.Any())
            {
                return 1;
            }
            return _packagesToRun.OrderByDescending(p => p.RunOrder).First().RunOrder + 1;
        }

        private void Persist()
        {
            _packageToRunStorage.Persist(_packagesToRun);
        }

        private void AddInstance(PackageToRun packageToRun)
        {
            AssignRunOrderIndexIfZero(packageToRun);
            _packagesToRun.Add(packageToRun);
        }

        private void UpdateInstance(PackageToRun packageToRunModified, PackageToRun instancePackage)
        {
            instancePackage.InstanceName = packageToRunModified.InstanceName;
            instancePackage.Parameter = packageToRunModified.Parameter;
            instancePackage.RunOnServerStart = packageToRunModified.RunOnServerStart;

            AssignRunOrderIndexIfZero(instancePackage);
            ReorderPackages(packageToRunModified, instancePackage);

            instancePackage.RunOrder = packageToRunModified.RunOrder;
        }

        private void AssignRunOrderIndexIfZero(PackageToRun instancePackage)
        {
            if (instancePackage.RunOrder == 0)
            {
                instancePackage.RunOrder = GetMostHighIndex(instancePackage) + 1;
            }
        }

        private int GetMostHighIndex(PackageToRun instancePackage)
        {
            var packagesNonZero = _packagesToRun.Except(new List<PackageToRun>() {instancePackage}).Where(p => p.RunOrder > 0)
                .OrderByDescending(p => p.RunOrder);
            if (packagesNonZero.Any())
            {
                return packagesNonZero.First().RunOrder;
            }

            return 0;
        }

        private void ReorderPackages(PackageToRun packageToRunModified, PackageToRun instancePackage)
        {
            var instancesExceptModified = _packagesToRun.Except(new List<PackageToRun>() {instancePackage});

            if (packageToRunModified.RunOrder < instancePackage.RunOrder)
            {
                ReorderToUpper(packageToRunModified, instancePackage, instancesExceptModified);
            }
            else
            {
                ReorderToLower(packageToRunModified, instancePackage, instancesExceptModified);
            }
        }

        private static void ReorderToLower(PackageToRun packageToRunModified, PackageToRun instancePackage,
            IEnumerable<PackageToRun> instancesExceptModified)
        {
            var instancesToReorder = instancesExceptModified.Where(i =>
                i.RunOrder >= instancePackage.RunOrder && i.RunOrder <= packageToRunModified.RunOrder);

            foreach (var packageToRun in instancesToReorder)
            {
                packageToRun.RunOrder--;
            }
        }

        private static void ReorderToUpper(PackageToRun packageToRunModified, PackageToRun instancePackage,
            IEnumerable<PackageToRun> instancesExceptModified)
        {
            var instancesToReorder = instancesExceptModified.Where(i =>
                i.RunOrder >= packageToRunModified.RunOrder && i.RunOrder <= instancePackage.RunOrder);

            foreach (var packageToRun in instancesToReorder)
            {
                packageToRun.RunOrder++;
            }
        }
    }
}
