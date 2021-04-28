using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NestorHub.Sentinels.Domain.Interfaces;

namespace NestorHub.Sentinels.Domain.Class
{
    public class PackagesStore : IPackagesStore
    {
        private readonly string _pathToStore;
        private readonly IPackageToDeleteStorage _packageToDeleteStorage;
        private readonly IPackageInfoDefinitionStorage _packageInfoDefinitionStorage;

        private readonly List<PackageInfo> _packagesInStore;
        private readonly List<PackageInfo> _packagesToDelete;

        public PackagesStore(string pathToStore, IPackageToDeleteStorage packageToDeleteStorage, IPackageInfoDefinitionStorage packageInfoDefinitionStorage)
        {
            _pathToStore = pathToStore;
            _packageToDeleteStorage = packageToDeleteStorage;
            _packageInfoDefinitionStorage = packageInfoDefinitionStorage;
            _packagesInStore = new List<PackageInfo>();
            _packagesToDelete = new List<PackageInfo>();
            LoadPackageInStore();
        }

        public List<PackageInfo> GetInstallPackages()
        {
            return _packagesInStore.Except(_packagesToDelete).ToList();
        }

        public PackageInfo InstallPackage(string packageName, string packageArchive)
        {
            var packageFileInfo = new FileInfo(packageArchive);
            var destinationPath = Path.Combine(_pathToStore, packageName);
            ZipFile.ExtractToDirectory(packageArchive, destinationPath);

            LoadPackageInStore();

            return _packageInfoDefinitionStorage.GetPackageInfoFromDefinitionFile(destinationPath);
        }

        public void MarkPackageToUninstall(string packageId)
        {
            var packagesInfo = _packagesInStore.Where(p => p.Id == packageId);
            if (packagesInfo.Any())
            {
                MarkPackageToDelete(packageId);
            }
        }

        public void UninstallPackagesMarkedToDelete()
        {
            var packagesToDelete = _packageToDeleteStorage.Load();
            foreach (var packageInfo in packagesToDelete)
            {
                DeletePackageDirectory(packageInfo.Id);
                RemovePackageToPackagesInstalled(packageInfo);
            }
            _packageToDeleteStorage.DeleteStorage();
        }

        private void RemovePackageToPackagesInstalled(PackageInfo packageInfo)
        {
            _packagesInStore.RemoveAll(p => p.Id == packageInfo.Id);
        }

        private void MarkPackageToDelete(string packageId)
        {
            if (!_packagesToDelete.Any(p => p.Id == packageId))
            {
                var packagesToDelete =_packagesInStore.Where(p => p.Id == packageId);
                _packagesToDelete.AddRange(packagesToDelete);
                _packageToDeleteStorage.Persist(_packagesToDelete);
            }
        }

        private void DeletePackageDirectory(string packageId)
        {
            var packagesInfo = _packagesInStore.Where(p => p.Id == packageId);
            if (packagesInfo.Any())
            {
                var destinationPath = Path.Combine(_pathToStore, packageId);
                Console.WriteLine(destinationPath);
                Directory.Delete(destinationPath, true);
            }
        }

        private void LoadPackageInStore()
        {
            _packagesInStore.Clear();

            var directories = Directory.EnumerateDirectories(_pathToStore);
            foreach (var directory in directories)
            {
                var packageInfo = _packageInfoDefinitionStorage.GetPackageInfoFromDefinitionFile(directory);
                if (packageInfo != null)
                {
                    _packagesInStore.Add(packageInfo);
                }
            }
        }
    }
}
