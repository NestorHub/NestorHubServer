using System.Collections.Generic;
using NestorHub.Sentinels.Domain.Class;

namespace NestorHub.Sentinels.Domain.Interfaces
{
    public interface IPackageToRunStorage
    {
        List<PackageToRun> LoadFromPersistence();
        void Persist(List<PackageToRun> packagesToRun);
    }
}