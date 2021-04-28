using System.Collections.Generic;
using System.IO;
using NestorHub.Sentinels.Domain.Class;
using NestorHub.Sentinels.Domain.Interfaces;
using Newtonsoft.Json;

namespace NestorHub.Sentinels.Infra
{
    public class PackageToDeleteStorage : IPackageToDeleteStorage
    {
        private const string PackagesToDeleteFilename = "PackageToDelete.json";
        private readonly string _pathToStore;

        public PackageToDeleteStorage(string pathToStore)
        {
            _pathToStore = pathToStore;
        }

        public List<PackageInfo> Load()
        {
            var listPackageToDelete = new List<PackageInfo>();
            var pathFilePackagesToDelete = Path.Combine(_pathToStore, PackagesToDeleteFilename);
            if (File.Exists(pathFilePackagesToDelete))
            {
                listPackageToDelete.AddRange(DeserializePackagesToDelete());
            }
            return listPackageToDelete;
        }

        public void Persist(List<PackageInfo> packagesInfo)
        {
            var data = JsonConvert.SerializeObject(packagesInfo, Formatting.Indented);
            var jsonTextWriter = new JsonTextWriter(new StreamWriter(GetPackagesToDeleteFile(), false));
            jsonTextWriter.WriteRaw(data);
            jsonTextWriter.Flush();
            jsonTextWriter.Close();
        }

        public void DeleteStorage()
        {
            if (File.Exists(GetPathFile()))
            {
                File.Delete(GetPathFile());
            }
        }

        private List<PackageInfo> DeserializePackagesToDelete()
        {
            using (var filesToDeleteTextReader = new JsonTextReader(new StreamReader(GetPathFile())))
            {
                var serializer = new JsonSerializer();
                var packagesToDelete = serializer.Deserialize<List<PackageInfo>>(filesToDeleteTextReader);
                filesToDeleteTextReader.Close();
                return packagesToDelete;
            }
        }

        private string GetPackagesToDeleteFile()
        {
            var pathFilePackagesToDelete = GetPathFile();
            if (!File.Exists(pathFilePackagesToDelete))
            {
                var file = File.Create(pathFilePackagesToDelete);
                file.Close();
            }
            return pathFilePackagesToDelete;
        }

        private string GetPathFile()
        {
            return Path.Combine(_pathToStore, PackagesToDeleteFilename);
        }
    }
}