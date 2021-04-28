using NestorHub.Sentinels.Domain.Class;

namespace NestorHub.Sentinels.Domain.Interfaces
{
    public interface IPackageInfoDefinitionStorage
    {
        PackageInfo GetPackageInfoFromDefinitionFile(string directory);
    }
}