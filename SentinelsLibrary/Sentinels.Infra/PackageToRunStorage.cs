using System.Collections.Generic;
using System.IO;
using NestorHub.Sentinels.Domain.Class;
using NestorHub.Sentinels.Domain.Interfaces;
using Newtonsoft.Json;

namespace NestorHub.Sentinels.Infra
{
    public class PackageToRunStorage : IPackageToRunStorage
    {
        private const string DefinitionPackageToRunFilename = "PackagesToRun.json";
        private readonly string _pathToStore;
        private StreamReader _packageToRunTextReader;

        public PackageToRunStorage(string pathToStore)
        {
            _pathToStore = pathToStore;
        }

        public List<PackageToRun> LoadFromPersistence()
        {
            var packages = new List<PackageToRun>();

            var jsonReader = GetDefinitionFileTextReader(_pathToStore);
            if (jsonReader != null)
            {
                var packagesToRun = DeserializePackagesToRun(jsonReader);
                if (packagesToRun != null)
                {
                    packages.AddRange(packagesToRun);
                }
                _packageToRunTextReader.Close();
                jsonReader.Close();
            }
            return packages;
        }

        public void Persist(List<PackageToRun> packagesToRun)
        {
            var data = JsonConvert.SerializeObject(packagesToRun, Formatting.Indented);
            var jsonTextWriter = new JsonTextWriter(new StreamWriter(GetPackagesToRunFile(_pathToStore)));
            jsonTextWriter.WriteRaw(data);
            jsonTextWriter.Flush();
            jsonTextWriter.Close();
        }

        private static List<PackageToRun> DeserializePackagesToRun(JsonTextReader jsonReader)
        {
            var serializer = new JsonSerializer();
            var packageToRun = serializer.Deserialize<List<PackageToRun>>(jsonReader);
            return packageToRun;
        }

        private JsonTextReader GetDefinitionFileTextReader(string directory)
        {
            _packageToRunTextReader = new StreamReader(GetPackagesToRunFile(directory));
            return new JsonTextReader(_packageToRunTextReader);
        }

        private static string GetPackagesToRunFile(string directory)
        {
            var directoryInfo = new DirectoryInfo(directory);
            var packagesToRunFilePath = Path.Combine(directoryInfo.FullName, DefinitionPackageToRunFilename);
            if (!File.Exists(packagesToRunFilePath))
            {
                var file = File.Create(packagesToRunFilePath);
                file.Close();
            }
            return packagesToRunFilePath;
        }
    }
}