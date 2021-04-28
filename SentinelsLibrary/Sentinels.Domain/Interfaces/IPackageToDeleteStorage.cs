using System.Collections.Generic;
using NestorHub.Sentinels.Domain.Class;

namespace NestorHub.Sentinels.Domain.Interfaces
{
    public interface IPackageToDeleteStorage
    {
        List<PackageInfo> Load();
        void Persist(List<PackageInfo> packagesInfo);
        void DeleteStorage();
    }
}