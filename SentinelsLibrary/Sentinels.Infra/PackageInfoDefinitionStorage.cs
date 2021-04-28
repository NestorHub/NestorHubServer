using System.IO;
using System.Linq;
using NestorHub.Sentinels.Domain.Class;
using NestorHub.Sentinels.Domain.Interfaces;
using Newtonsoft.Json;

namespace NestorHub.Sentinels.Infra
{
    public class PackageInfoDefinitionStorage : IPackageInfoDefinitionStorage
    {
        private const string DefinitionPackageFilename = "Package.json";
        private const string DefinitionPackageFileExtension = "*.json";

        public PackageInfo GetPackageInfoFromDefinitionFile(string directory)
        {
            var jsonReader = GetDefinitionFileTextReader(directory);
            if (jsonReader != null)
            {
                return DeserializePackageInfo(jsonReader);
            }
            return null;
        }

        private static PackageInfo DeserializePackageInfo(JsonTextReader jsonReader)
        {
            var serializer = new JsonSerializer();
            var packageInfo = serializer.Deserialize<PackageInfo>(jsonReader);
            jsonReader.Close();
            return packageInfo;
        }

        private JsonTextReader GetDefinitionFileTextReader(string directory)
        {
            var directoryInfo = new DirectoryInfo(directory);
            var filesJson = directoryInfo.EnumerateFiles(DefinitionPackageFileExtension);
            if (filesJson.Any(f => f.Name == DefinitionPackageFilename))
            {
                return new JsonTextReader(new StreamReader(filesJson.First(f => f.Name == DefinitionPackageFilename).FullName));
            }
            return null;
        }
    }
}
