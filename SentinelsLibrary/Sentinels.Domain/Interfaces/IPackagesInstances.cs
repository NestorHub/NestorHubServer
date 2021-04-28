using System;
using System.Collections.Generic;
using NestorHub.Sentinels.Domain.Class;

namespace NestorHub.Sentinels.Domain.Interfaces
{
    public interface IPackagesInstances
    {
        void AddInstanceToRun(PackageToRun packageToRun);
        IEnumerable<PackageToRun> GetInstancesToRunByPackageId(string packageId);
        IEnumerable<PackageToRun> GetAllInstancesToRun();
        void DeleteInstance(Guid id);
        void DeleteInstances(IEnumerable<PackageToRun> packageToDelete);
        PackageToRun GetInstanceToRunById(Guid instanceId);
        int GetNextRunOrderIndex();
    }
}