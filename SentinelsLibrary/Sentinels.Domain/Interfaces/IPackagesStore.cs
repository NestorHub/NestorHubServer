using System.Collections.Generic;
using NestorHub.Sentinels.Domain.Class;

namespace NestorHub.Sentinels.Domain.Interfaces
{
    public interface IPackagesStore
    {
        List<PackageInfo> GetInstallPackages();
        PackageInfo InstallPackage(string packageName, string packageArchive);
        void MarkPackageToUninstall(string packageName);
        void UninstallPackagesMarkedToDelete();
    }
}